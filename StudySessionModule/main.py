from fastapi import FastAPI, Depends, HTTPException, status, Query
from sqlalchemy.orm import Session
from typing import List, Optional
import datetime

from . import crud, models, schemas, auth
from .database import SessionLocal, engine, create_db_and_tables

# Create database tables if they don't exist
create_db_and_tables()

app = FastAPI(
    title="Study Sessions & Reservations API",
    description="API for managing study sessions and student reservations.",
    version="1.0.0"
)

# Dependency to get DB session
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

# --- API Endpoints ---

# Study Sessions
@app.post("/api/study-sessions", response_model=schemas.StudySessionRead, status_code=status.HTTP_201_CREATED,
          summary="Create New Study Session (Teacher Only)",
          description="Allows authenticated teachers to create a new study session. Session times should be in UTC.")
async def create_study_session(
    session: schemas.StudySessionCreate,
    db: Session = Depends(get_db),
    current_user: auth.User = Depends(auth.is_teacher)
):
    """
    Create a new study session.
    - **title**: Title of the session.
    - **description**: Optional description.
    - **start_time**: Session start time (ISO 8601 format, e.g., YYYY-MM-DDTHH:MM:SSZ).
    - **end_time**: Session end time (ISO 8601 format).
    - **capacity**: Maximum number of students.
    \nRequires `teacher` role. The session will be associated with the authenticated teacher.
    """
    if session.end_time <= session.start_time:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="End time must be after start time.")
    if session.start_time <= datetime.datetime.utcnow():
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Start time must be in the future.")
    if session.capacity <= 0:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Capacity must be a positive integer.")

    db_session = crud.create_study_session(db=db, session=session, teacher_id=current_user.id)
    # To include reservation_count in the response, we can re-fetch or manually set it
    # For simplicity, we'll just return what create_study_session gives and rely on GET for full details
    # Or, modify create_study_session to return what StudySessionRead expects, including count.
    # Let's use the get_study_session_with_reservations_count for consistency
    return crud.get_study_session_with_reservations_count(db, db_session.id)


@app.get("/api/study-sessions", response_model=List[schemas.StudySessionRead],
         summary="List Upcoming or All Study Sessions with Filters",
         description="Retrieves a list of study sessions. By default, only upcoming sessions are shown. "
                     "Supports filtering by date range, title keywords, and teacher ID.")
async def list_study_sessions(
    skip: int = 0,
    limit: int = Query(default=20, le=100), # Max 100 items per page
    teacher_id: Optional[str] = Query(None, description="Filter by teacher's ID (e.g., 'teacher1')"),
    title_keyword: Optional[str] = Query(None, description="Filter by keyword in session title (case-insensitive)"),
    start_date: Optional[datetime.date] = Query(None, description="Filter sessions starting on or after this date (YYYY-MM-DD)"),
    end_date: Optional[datetime.date] = Query(None, description="Filter sessions ending on or before this date (YYYY-MM-DD)"),
    include_past: bool = Query(False, description="Include past sessions in the results"),
    db: Session = Depends(get_db)
    # current_user: auth.User = Depends(auth.get_current_active_user) # Any active user can view sessions
):
    """
    Available filters:
    - `teacher_id`: Exact match for the teacher's ID.
    - `title_keyword`: Case-insensitive search within the session title.
    - `start_date`: Sessions starting on or after this date.
    - `end_date`: Sessions starting on or before this date. (Note: crud logic checks session end_time)
    - `include_past`: Set to `true` to include sessions whose end_time has passed.
    """
    sessions = crud.get_study_sessions(
        db, skip=skip, limit=limit,
        teacher_id=teacher_id, title_keyword=title_keyword,
        start_date=start_date, end_date=end_date, include_past=include_past
    )
    return sessions

@app.get("/api/study-sessions/{session_id}", response_model=schemas.StudySessionRead,
         summary="Get Specific Study Session Details",
         description="Retrieves details for a single study session, including the count of current reservations.")
async def get_study_session_details(
    session_id: int,
    db: Session = Depends(get_db)
    # current_user: auth.User = Depends(auth.get_current_active_user)
):
    # db_session = crud.get_study_session(db, session_id=session_id)
    db_session_details = crud.get_study_session_with_reservations_count(db, session_id=session_id)
    if db_session_details is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Study session not found")
    return db_session_details

@app.put("/api/study-sessions/{session_id}", response_model=schemas.StudySessionRead,
         summary="Update Study Session (Teacher/Owner Only)",
         description="Allows the teacher who created the session to update its details.")
async def update_study_session(
    session_id: int,
    session_update: schemas.StudySessionUpdate,
    db: Session = Depends(get_db),
    current_user: auth.User = Depends(auth.is_teacher)
):
    db_session = crud.get_study_session(db, session_id) # Fetch first to check ownership before update logic
    if not db_session:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Study session not found")
    if db_session.teacher_id != current_user.id:
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Not authorized to update this session")

    if session_update.start_time and session_update.start_time <= datetime.datetime.utcnow():
         raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Start time must be in the future.")

    # Ensure end_time is after start_time if both are provided or one is updated relative to the other
    current_start_time = db_session.start_time
    current_end_time = db_session.end_time

    new_start_time = session_update.start_time if session_update.start_time else current_start_time
    new_end_time = session_update.end_time if session_update.end_time else current_end_time

    if new_end_time <= new_start_time:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="End time must be after start time.")

    if session_update.capacity is not None and session_update.capacity <= 0:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Capacity must be a positive integer.")

    # If capacity is reduced, check against current reservations
    if session_update.capacity is not None:
        reservations_count = crud.count_reservations_for_session(db, session_id)
        if session_update.capacity < reservations_count:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"New capacity ({session_update.capacity}) is less than current reservations ({reservations_count})."
            )

    updated_session = crud.update_study_session(db, session_id=session_id, session_update=session_update, teacher_id=current_user.id)
    if updated_session is None: # Should be caught by above checks, but good for safety
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Update failed or session not found")

    return crud.get_study_session_with_reservations_count(db, updated_session.id)


@app.delete("/api/study-sessions/{session_id}", status_code=status.HTTP_204_NO_CONTENT,
            summary="Delete Study Session (Teacher/Owner Only)",
            description="Allows the teacher who created the session to delete it. This will also remove all reservations for the session.")
async def delete_study_session(
    session_id: int,
    db: Session = Depends(get_db),
    current_user: auth.User = Depends(auth.is_teacher)
):
    db_session = crud.get_study_session(db, session_id) # Fetch first to check ownership
    if not db_session:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Study session not found")
    if db_session.teacher_id != current_user.id:
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Not authorized to delete this session")

    deleted_session = crud.delete_study_session(db, session_id=session_id, teacher_id=current_user.id)
    if deleted_session is None: # Should be caught by above checks
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Deletion failed or session not found")
    return None # HTTP 204 No Content


# Reservations
@app.post("/api/study-sessions/{session_id}/reserve", response_model=schemas.ReservationRead,
          summary="Reserve a Seat in a Session (Student Only)",
          description="Allows an authenticated student to reserve a seat in an available study session if capacity allows. "
                      "Students cannot reserve a seat in past sessions or if they already have a reservation.")
async def make_reservation(
    session_id: int,
    db: Session = Depends(get_db),
    current_user: auth.User = Depends(auth.is_student)
):
    """
    Reserve a seat for `session_id`.
    \nRequires `student` role.
    \nError Responses:
    - `404 Not Found`: If the session does not exist.
    - `400 Bad Request`:
        - "SESSION_PAST": If the session has already started or passed.
        - "CAPACITY_EXCEEDED": If the session is full.
        - "ALREADY_RESERVED": If the student already has a reservation for this session.
    - `409 Conflict`: Could be used for concurrency issues if not handled by pessimistic locking in CRUD.
    """
    result = crud.create_reservation(db=db, session_id=session_id, student_id=current_user.id)

    if result == "SESSION_PAST":
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Cannot reserve a seat in a past session.")
    if result == "CAPACITY_EXCEEDED":
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Session capacity exceeded.")
    if result == "ALREADY_RESERVED":
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="You have already reserved a seat in this session.")
    if result is None: # Session not found
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Study session not found.")

    return result # This is the db_reservation object

@app.get("/api/my-reservations", response_model=List[schemas.ReservationRead],
         summary="List Student's Own Reservations",
         description="Retrieves all reservations made by the currently authenticated student.")
async def list_my_reservations(
    skip: int = 0,
    limit: int = 10,
    db: Session = Depends(get_db),
    current_user: auth.User = Depends(auth.get_current_active_user) # Any active user can see their own
):
    # Although any active user can call this, it makes most sense for students.
    # Teachers could also have reservations if they act as students in other sessions.
    if not current_user: # Should be caught by get_current_active_user
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication required")

    reservations = crud.get_reservations_by_student(db, student_id=current_user.id, skip=skip, limit=limit)
    return reservations

@app.delete("/api/reservations/{reservation_id}", status_code=status.HTTP_204_NO_CONTENT,
            summary="Cancel an Existing Reservation (Student or Session Teacher)",
            description="Allows a student to cancel their own reservation, or a teacher to cancel a reservation for a session they own. "
                        "Students generally cannot cancel reservations for sessions that have already started.")
async def cancel_reservation(
    reservation_id: int,
    db: Session = Depends(get_db),
    current_user: auth.User = Depends(auth.get_current_active_user)
):
    """
    Cancel reservation with `reservation_id`.
    \nIf the user is a student, they can only cancel their own reservation.
    If the user is a teacher, they can cancel any reservation for a session they own.
    \nError Responses:
    - `404 Not Found`: If the reservation does not exist.
    - `403 Forbidden`: If the user is not authorized to cancel this reservation.
    - `400 Bad Request`:
        - "SESSION_PAST_CANNOT_CANCEL": If a student tries to cancel a reservation for a past or ongoing session.
    """
    reservation_to_delete = crud.get_reservation(db, reservation_id)
    if not reservation_to_delete:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Reservation not found.")

    is_teacher_of_session = False
    session_teacher_id = None
    if "teacher" in current_user.roles:
        # Need to fetch the session to check if the teacher owns it
        session = crud.get_study_session(db, reservation_to_delete.session_id)
        if session and session.teacher_id == current_user.id:
            is_teacher_of_session = True
            session_teacher_id = session.teacher_id


    result = crud.delete_reservation(
        db,
        reservation_id=reservation_id,
        user_id=current_user.id,
        user_is_teacher=is_teacher_of_session # Pass if current user is a teacher AND owns the session
    )

    if result == "PERMISSION_DENIED":
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Not authorized to cancel this reservation.")
    if result == "SESSION_PAST_CANNOT_CANCEL":
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Cannot cancel reservation for a past or ongoing session.")
    if result is None and reservation_to_delete: # Should mean not found after initial check, or other logic error in crud
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Reservation not found or cancellation failed.")

    return None # HTTP 204


# Root endpoint for basic check
@app.get("/", include_in_schema=False)
async def root():
    return {"message": "Study Session Module API is running. See /docs for API documentation."}

# To run the app (for local development):
# uvicorn StudySessionModule.main:app --reload --port 8001 --app-dir .
# (Assuming you are in the directory containing StudySessionModule folder)
# Or, if inside StudySessionModule: uvicorn main:app --reload --port 8001
# Need to adjust PYTHONPATH if running from root: export PYTHONPATH=.
# For testing, we will use TestClient which handles this.

# Add dummy users for header-based auth simulation:
# X-User-ID: teacher1
# X-User-ID: student1
# X-User-ID: student2
# X-User-ID: admin (also a teacher)
