import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from typing import Generator, Any
import datetime

from .main import app, get_db
from .database import Base, create_db_and_tables as create_actual_db_tables
from .models import StudySession, Reservation
from .auth import get_current_active_user, User, fake_users_db, DUMMY_TEACHER, DUMMY_STUDENT
from .schemas import StudySessionRead, ReservationRead

# --- Test Database Setup ---
SQLALCHEMY_DATABASE_URL = "sqlite:///./test_study_sessions.db"
engine = create_engine(SQLALCHEMY_DATABASE_URL, connect_args={"check_same_thread": False})
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# Apply migrations to the test database
Base.metadata.create_all(bind=engine)

# --- Test Dependencies Override ---
def override_get_db() -> Generator[Any, Any, None]:
    db = TestingSessionLocal()
    try:
        yield db
    finally:
        db.close()

async def override_get_current_active_user_teacher() -> User:
    return DUMMY_TEACHER

async def override_get_current_active_user_student() -> User:
    return DUMMY_STUDENT

async def override_get_current_active_user_student2() -> User:
    return User(id="student2", email="student2@example.com", roles=["student"])

# Fixture to provide a TestClient instance
@pytest.fixture(scope="module")
def client() -> Generator[TestClient, Any, None]:
    # Create tables in the test database before tests run
    Base.metadata.drop_all(bind=engine) # Ensure clean state
    Base.metadata.create_all(bind=engine)

    app.dependency_overrides[get_db] = override_get_db

    with TestClient(app) as c:
        yield c

    # Optional: Drop tables after tests if needed, or leave for inspection
    # Base.metadata.drop_all(bind=engine)


# Fixture to automatically clean up database tables after each test
@pytest.fixture(autouse=True)
def db_cleanup():
    db = TestingSessionLocal()
    try:
        # Clean relevant tables before each test
        db.query(Reservation).delete()
        db.query(StudySession).delete()
        db.commit()
        yield # this is where the test runs
    finally:
        db.close()

# --- Test Cases ---

# Helper to create a session directly for setup
def create_session_direct(db, title, teacher_id, capacity=10, hours_offset=1):
    start_time = datetime.datetime.utcnow() + datetime.timedelta(hours=hours_offset)
    end_time = start_time + datetime.timedelta(hours=1)
    session = StudySession(
        title=title,
        description="Test session description",
        start_time=start_time,
        end_time=end_time,
        capacity=capacity,
        teacher_id=teacher_id
    )
    db.add(session)
    db.commit()
    db.refresh(session)
    return session

# == Study Session Tests ==

def test_create_study_session_as_teacher(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_teacher
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher # Ensure is_teacher uses the override

    start_time = datetime.datetime.utcnow() + datetime.timedelta(days=1)
    end_time = start_time + datetime.timedelta(hours=2)

    response = client.post(
        "/api/study-sessions",
        json={
            "title": "Maths Revision",
            "description": "Covering algebra basics.",
            "start_time": start_time.isoformat(),
            "end_time": end_time.isoformat(),
            "capacity": 20
        }
    )
    assert response.status_code == 201
    data = response.json()
    assert data["title"] == "Maths Revision"
    assert data["teacher_id"] == DUMMY_TEACHER.id
    assert data["capacity"] == 20
    assert "id" in data

def test_create_study_session_as_student_forbidden(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_student # This will now fail role check

    start_time = datetime.datetime.utcnow() + datetime.timedelta(days=1)
    end_time = start_time + datetime.timedelta(hours=2)

    response = client.post(
        "/api/study-sessions",
        json={
            "title": "Illegal Session by Student",
            "start_time": start_time.isoformat(),
            "end_time": end_time.isoformat(),
            "capacity": 5
        }
    )
    assert response.status_code == 403 # Forbidden

def test_create_study_session_invalid_times(client: TestClient):
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher
    start_time_past = datetime.datetime.utcnow() - datetime.timedelta(days=1)
    start_time_future = datetime.datetime.utcnow() + datetime.timedelta(days=1)

    response_past = client.post("/api/study-sessions", json={
        "title": "Past Session", "start_time": start_time_past.isoformat(),
        "end_time": (start_time_past + datetime.timedelta(hours=1)).isoformat(), "capacity": 5
    })
    assert response_past.status_code == 400
    assert "Start time must be in the future" in response_past.json()["detail"]

    response_end_before_start = client.post("/api/study-sessions", json={
        "title": "End Before Start", "start_time": start_time_future.isoformat(),
        "end_time": (start_time_future - datetime.timedelta(hours=1)).isoformat(), "capacity": 5
    })
    assert response_end_before_start.status_code == 400
    assert "End time must be after start time" in response_end_before_start.json()["detail"]

def test_list_study_sessions_no_filters(client: TestClient):
    # Setup: Create a couple of sessions directly in the DB for this test
    db = TestingSessionLocal()
    create_session_direct(db, "Session 1", DUMMY_TEACHER.id)
    create_session_direct(db, "Session 2", "another_teacher")
    db.close()

    response = client.get("/api/study-sessions")
    assert response.status_code == 200
    data = response.json()
    assert len(data) >= 2 # Could be more if other tests left data, db_cleanup should prevent
    assert any(s["title"] == "Session 1" for s in data)
    assert any(s["title"] == "Session 2" for s in data)

def test_list_study_sessions_with_teacher_filter(client: TestClient):
    db = TestingSessionLocal()
    create_session_direct(db, "My Session", DUMMY_TEACHER.id)
    create_session_direct(db, "Other Teacher Session", "other_teacher_id")
    db.close()

    response = client.get(f"/api/study-sessions?teacher_id={DUMMY_TEACHER.id}")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 1
    assert data[0]["title"] == "My Session"
    assert data[0]["teacher_id"] == DUMMY_TEACHER.id

def test_get_specific_study_session(client: TestClient):
    db = TestingSessionLocal()
    session = create_session_direct(db, "Specific Session", DUMMY_TEACHER.id)
    db.close()

    response = client.get(f"/api/study-sessions/{session.id}")
    assert response.status_code == 200
    data = response.json()
    assert data["title"] == "Specific Session"
    assert data["id"] == session.id
    assert data["reservations_count"] == 0

def test_update_study_session_as_owner(client: TestClient):
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher
    db = TestingSessionLocal()
    session = create_session_direct(db, "Original Title", DUMMY_TEACHER.id)
    db.close()

    new_title = "Updated Title by Owner"
    response = client.put(
        f"/api/study-sessions/{session.id}",
        json={"title": new_title, "capacity": 15}
    )
    assert response.status_code == 200
    data = response.json()
    assert data["title"] == new_title
    assert data["capacity"] == 15
    assert data["teacher_id"] == DUMMY_TEACHER.id

def test_update_study_session_as_non_owner_forbidden(client: TestClient):
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher # Current user is DUMMY_TEACHER

    db = TestingSessionLocal()
    # Session created by a different teacher
    session = create_session_direct(db, "Other's Session", "another_teacher_id")
    db.close()

    response = client.put(
        f"/api/study-sessions/{session.id}",
        json={"title": "Attempted Update"}
    )
    assert response.status_code == 403 # Forbidden, DUMMY_TEACHER is not 'another_teacher_id'

def test_delete_study_session_as_owner(client: TestClient):
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher
    db = TestingSessionLocal()
    session = create_session_direct(db, "To Be Deleted", DUMMY_TEACHER.id)
    db.close()

    response = client.delete(f"/api/study-sessions/{session.id}")
    assert response.status_code == 204

    # Verify it's gone
    get_response = client.get(f"/api/study-sessions/{session.id}")
    assert get_response.status_code == 404

# == Reservation Tests ==

def test_make_reservation_as_student(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student

    db = TestingSessionLocal()
    session = create_session_direct(db, "Reservable Session", "some_teacher_id", capacity=1)
    db.close()

    response = client.post(f"/api/study-sessions/{session.id}/reserve")
    assert response.status_code == 200
    data = response.json()
    assert data["session_id"] == session.id
    assert data["student_id"] == DUMMY_STUDENT.id
    assert "id" in data # Reservation ID

    # Verify reservation count on session
    session_details_resp = client.get(f"/api/study-sessions/{session.id}")
    assert session_details_resp.json()["reservations_count"] == 1


def test_make_reservation_session_full(client: TestClient):
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student

    db = TestingSessionLocal()
    session = create_session_direct(db, "Full Session Test", "teacher_for_full_session", capacity=1)
    # First student reserves
    client.post(f"/api/study-sessions/{session.id}/reserve")
    db.close()

    # Second student tries to reserve (override user to student2)
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student2
    response_full = client.post(f"/api/study-sessions/{session.id}/reserve")
    assert response_full.status_code == 400
    assert response_full.json()["detail"] == "Session capacity exceeded."

def test_make_reservation_already_reserved(client: TestClient):
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student
    db = TestingSessionLocal()
    session = create_session_direct(db, "Double Reserve Test", "teacher_double_reserve", capacity=2)
    db.close()

    client.post(f"/api/study-sessions/{session.id}/reserve") # First reservation
    response_again = client.post(f"/api/study-sessions/{session.id}/reserve") # Same student tries again
    assert response_again.status_code == 400
    assert response_again.json()["detail"] == "You have already reserved a seat in this session."


def test_make_reservation_past_session(client: TestClient):
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student
    db = TestingSessionLocal()
    # Create a session that has already ended
    session = create_session_direct(db, "Past Session Reserve Test", "teacher_past_session", hours_offset=-2)
    db.close()

    response = client.post(f"/api/study-sessions/{session.id}/reserve")
    assert response.status_code == 400
    assert response.json()["detail"] == "Cannot reserve a seat in a past session."


def test_list_my_reservations(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student

    db = TestingSessionLocal()
    session1 = create_session_direct(db, "My Reserved Session 1", "teacher_res1")
    session2 = create_session_direct(db, "My Reserved Session 2", "teacher_res2")
    # Student makes reservations
    client.post(f"/api/study-sessions/{session1.id}/reserve")
    client.post(f"/api/study-sessions/{session2.id}/reserve")
    db.close()

    response = client.get("/api/my-reservations")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 2
    assert any(r["session_id"] == session1.id and r["student_id"] == DUMMY_STUDENT.id for r in data)
    assert any(r["session_id"] == session2.id and r["student_id"] == DUMMY_STUDENT.id for r in data)

def test_cancel_own_reservation_as_student(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student

    db = TestingSessionLocal()
    session = create_session_direct(db, "Cancellable Session", "teacher_cancel")
    # Student makes a reservation
    reservation_resp = client.post(f"/api/study-sessions/{session.id}/reserve")
    reservation_id = reservation_resp.json()["id"]
    db.close()

    cancel_response = client.delete(f"/api/reservations/{reservation_id}")
    assert cancel_response.status_code == 204

    # Verify reservation is gone
    # One way: try to get it, or check "my reservations"
    my_reservations_resp = client.get("/api/my-reservations")
    assert len(my_reservations_resp.json()) == 0

    # Verify session reservation count decreased
    session_details_resp = client.get(f"/api/study-sessions/{session.id}")
    assert session_details_resp.json()["reservations_count"] == 0


def test_cancel_reservation_as_teacher_owner(client: TestClient):
    # Teacher DUMMY_TEACHER creates a session
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_teacher
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher

    db = TestingSessionLocal()
    session = create_session_direct(db, "Session by DummyTeacher", DUMMY_TEACHER.id)

    # Student DUMMY_STUDENT makes a reservation
    # Temporarily switch active user for reservation
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student # for reserve endpoint
    reservation_resp = client.post(f"/api/study-sessions/{session.id}/reserve")
    reservation_id = reservation_resp.json()["id"]
    db.close()

    # Switch back to DUMMY_TEACHER to cancel
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_teacher
    # No need to override is_student or is_teacher for the delete reservation endpoint itself,
    # as it uses get_current_active_user and checks roles internally.

    cancel_response = client.delete(f"/api/reservations/{reservation_id}")
    assert cancel_response.status_code == 204

    # Verify reservation count on session
    session_details_resp = client.get(f"/api/study-sessions/{session.id}")
    assert session_details_resp.json()["reservations_count"] == 0

def test_cancel_reservation_as_non_owner_teacher_forbidden(client: TestClient):
    # Session created by "teacher_A"
    db = TestingSessionLocal()
    session_owner_id = "teacher_A_owns_this"
    session = create_session_direct(db, "Session by Teacher A", session_owner_id)

    # Student DUMMY_STUDENT makes a reservation
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student
    reservation_resp = client.post(f"/api/study-sessions/{session.id}/reserve")
    reservation_id = reservation_resp.json()["id"]
    db.close()

    # Current active user is DUMMY_TEACHER (who is a teacher, but not session_owner_id)
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_teacher

    cancel_response = client.delete(f"/api/reservations/{reservation_id}")
    assert cancel_response.status_code == 403 # Forbidden
    assert "Not authorized to cancel this reservation" in cancel_response.json()["detail"]


# --- Edge Case Tests ---
def test_update_session_reduce_capacity_below_reservations(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_teacher
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher

    db = TestingSessionLocal()
    session = create_session_direct(db, "Capacity Test Session", DUMMY_TEACHER.id, capacity=2)

    # Student1 makes a reservation
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student
    client.post(f"/api/study-sessions/{session.id}/reserve")

    # Student2 makes a reservation
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student2
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student2
    client.post(f"/api/study-sessions/{session.id}/reserve")
    db.close()

    # Teacher tries to reduce capacity to 1
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_teacher
    app.dependency_overrides[auth.is_teacher] = override_get_current_active_user_teacher

    response = client.put(
        f"/api/study-sessions/{session.id}",
        json={"capacity": 1}
    )
    assert response.status_code == 400
    assert "New capacity (1) is less than current reservations (2)" in response.json()["detail"]

def test_cancel_non_existent_reservation(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    response = client.delete("/api/reservations/99999") # Non-existent ID
    assert response.status_code == 404
    assert "Reservation not found" in response.json()["detail"]

def test_reserve_non_existent_session(client: TestClient):
    app.dependency_overrides[auth.get_current_active_user] = override_get_current_active_user_student
    app.dependency_overrides[auth.is_student] = override_get_current_active_user_student
    response = client.post("/api/study-sessions/88888/reserve") # Non-existent ID
    assert response.status_code == 404
    assert "Study session not found" in response.json()["detail"]


# Concurrency tests are harder to write as unit/integration tests without specialized libraries
# or actual concurrent execution environments. The note about `with_for_update()` in `crud.py`
# is the primary mechanism to handle this at the DB level for supported databases.
# For SQLite, true concurrency safety for capacity checks under high load is limited.

# Reset dependency overrides after all tests in this file (or module) are done
# This is more for hygiene if other test files exist and might be affected.
# Pytest fixtures with `yield` handle teardown more cleanly.
# @pytest.fixture(scope="module", autouse=True)
# def cleanup_overrides():
#     yield
#     app.dependency_overrides = {}
