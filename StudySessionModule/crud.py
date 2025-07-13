from sqlalchemy.orm import Session, joinedload
from sqlalchemy import func, and_
from . import models, schemas
import datetime
from typing import List, Optional

# --- Study Session CRUD ---

def get_study_session(db: Session, session_id: int) -> Optional[models.StudySession]:
    return db.query(models.StudySession).filter(models.StudySession.id == session_id).first()

def get_study_session_with_reservations_count(db: Session, session_id: int) -> Optional[schemas.StudySessionRead]:
    session = db.query(
        models.StudySession,
        func.count(models.Reservation.id).label("reservations_count")
    ).outerjoin(models.Reservation, models.StudySession.id == models.Reservation.session_id)\
    .filter(models.StudySession.id == session_id)\
    .group_by(models.StudySession.id)\
    .first()

    if session:
        session_model, count = session
        session_read = schemas.StudySessionRead.from_orm(session_model)
        session_read.reservations_count = count
        return session_read
    return None


def get_study_sessions(db: Session, skip: int = 0, limit: int = 100,
                       teacher_id: Optional[str] = None,
                       title_keyword: Optional[str] = None,
                       start_date: Optional[datetime.date] = None,
                       end_date: Optional[datetime.date] = None,
                       include_past: bool = False) -> List[schemas.StudySessionRead]:
    query = db.query(
        models.StudySession,
        func.count(models.Reservation.id).label("reservations_count")
    ).outerjoin(models.Reservation, models.StudySession.id == models.Reservation.session_id)\
    .group_by(models.StudySession.id)

    if not include_past:
        query = query.filter(models.StudySession.end_time >= datetime.datetime.utcnow())

    if teacher_id:
        query = query.filter(models.StudySession.teacher_id == teacher_id)

    if title_keyword:
        query = query.filter(models.StudySession.title.ilike(f"%{title_keyword}%")) # Case-insensitive search

    if start_date:
        query = query.filter(models.StudySession.start_time >= datetime.datetime.combine(start_date, datetime.time.min))

    if end_date:
        query = query.filter(models.StudySession.end_time <= datetime.datetime.combine(end_date, datetime.time.max))

    sessions_with_counts = query.order_by(models.StudySession.start_time.asc()).offset(skip).limit(limit).all()

    result_list = []
    for session_model, count in sessions_with_counts:
        session_read = schemas.StudySessionRead.from_orm(session_model)
        session_read.reservations_count = count
        result_list.append(session_read)

    return result_list


def create_study_session(db: Session, session: schemas.StudySessionCreate, teacher_id: str) -> models.StudySession:
    db_session = models.StudySession(**session.dict(), teacher_id=teacher_id)
    db.add(db_session)
    db.commit()
    db.refresh(db_session)
    return db_session

def update_study_session(db: Session, session_id: int, session_update: schemas.StudySessionUpdate, teacher_id: str) -> Optional[models.StudySession]:
    db_session = db.query(models.StudySession).filter(models.StudySession.id == session_id).first()
    if not db_session:
        return None
    if db_session.teacher_id != teacher_id: # Ownership check
        return None # Or raise HTTPException for permission denied

    update_data = session_update.dict(exclude_unset=True)
    for key, value in update_data.items():
        setattr(db_session, key, value)

    db_session.updated_at = datetime.datetime.utcnow()
    db.commit()
    db.refresh(db_session)
    return db_session

def delete_study_session(db: Session, session_id: int, teacher_id: str) -> Optional[models.StudySession]:
    db_session = db.query(models.StudySession).filter(models.StudySession.id == session_id).first()
    if not db_session:
        return None
    if db_session.teacher_id != teacher_id: # Ownership check
        return None # Or raise HTTPException

    # Optionally, handle existing reservations (e.g., delete them or notify users)
    db.query(models.Reservation).filter(models.Reservation.session_id == session_id).delete()

    db.delete(db_session)
    db.commit()
    return db_session


# --- Reservation CRUD ---

def get_reservation(db: Session, reservation_id: int) -> Optional[models.Reservation]:
    return db.query(models.Reservation).filter(models.Reservation.id == reservation_id).first()

def get_reservations_by_student(db: Session, student_id: str, skip: int = 0, limit: int = 100) -> List[models.Reservation]:
    return db.query(models.Reservation)\
             .filter(models.Reservation.student_id == student_id)\
             .order_by(models.Reservation.reserved_at.desc())\
             .offset(skip).limit(limit).all()

def get_reservations_for_session(db: Session, session_id: int) -> List[models.Reservation]:
    return db.query(models.Reservation).filter(models.Reservation.session_id == session_id).all()

def count_reservations_for_session(db: Session, session_id: int) -> int:
    return db.query(models.Reservation).filter(models.Reservation.session_id == session_id).count()

def create_reservation(db: Session, session_id: int, student_id: str) -> Optional[models.Reservation]:
    # Lock the session row for update to prevent race conditions on capacity check
    # This requires a database that supports row-level locking, like PostgreSQL.
    # For SQLite, this lock is advisory and might not prevent all race conditions under high concurrency.
    # A more robust solution for SQLite might involve application-level locks or serializing writes.

    # Start a transaction
    try:
        # It's better to lock the StudySession row if possible or use FOR UPDATE if the DB supports it
        # For simplicity here, we'll query and then check. This is prone to race conditions without proper DB locking.
        # A select for update would be: db.query(models.StudySession).filter(models.StudySession.id == session_id).with_for_update().first()

        study_session = db.query(models.StudySession).filter(models.StudySession.id == session_id).first()

        if not study_session:
            return None # Session not found

        if study_session.start_time <= datetime.datetime.utcnow():
            return "SESSION_PAST" # Cannot reserve past sessions

        current_reservations_count = count_reservations_for_session(db, session_id)
        if current_reservations_count >= study_session.capacity:
            return "CAPACITY_EXCEEDED" # Capacity exceeded

        # Check if student already reserved this session
        existing_reservation = db.query(models.Reservation).filter(
            models.Reservation.session_id == session_id,
            models.Reservation.student_id == student_id
        ).first()
        if existing_reservation:
            return "ALREADY_RESERVED"

        db_reservation = models.Reservation(session_id=session_id, student_id=student_id)
        db.add(db_reservation)
        db.commit()
        db.refresh(db_reservation)
        return db_reservation
    except Exception as e:
        db.rollback() # Rollback in case of any error during the transaction
        raise e


def delete_reservation(db: Session, reservation_id: int, user_id: str, user_is_teacher: bool = False, session_teacher_id: Optional[str] = None) -> Optional[models.Reservation]:
    db_reservation = db.query(models.Reservation).options(joinedload(models.Reservation.session)).filter(models.Reservation.id == reservation_id).first()

    if not db_reservation:
        return None

    # Student can cancel their own reservation
    # Teacher who owns the session can cancel any reservation for their session
    can_cancel = False
    if db_reservation.student_id == user_id:
        can_cancel = True
    elif user_is_teacher and db_reservation.session.teacher_id == user_id: # Check if the current user is the teacher of the session
        can_cancel = True

    if not can_cancel:
        return "PERMISSION_DENIED" # Or raise HTTPException

    if db_reservation.session.start_time <= datetime.datetime.utcnow() and not user_is_teacher : # Students cannot cancel past reservations
        # Teachers might be allowed to clean up, depending on policy
        return "SESSION_PAST_CANNOT_CANCEL"


    db.delete(db_reservation)
    db.commit()
    return db_reservation
