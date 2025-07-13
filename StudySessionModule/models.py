from sqlalchemy import Column, Integer, String, DateTime, ForeignKey, func
from sqlalchemy.orm import relationship
from sqlalchemy.ext.declarative import declarative_base
import datetime

Base = declarative_base()

class StudySession(Base):
    __tablename__ = "study_sessions"

    id = Column(Integer, primary_key=True, index=True)
    title = Column(String, index=True, nullable=False)
    description = Column(String)
    start_time = Column(DateTime, nullable=False)
    end_time = Column(DateTime, nullable=False)
    capacity = Column(Integer, nullable=False)
    teacher_id = Column(String, nullable=False) # Assuming teacher_id is a string, e.g., username or email

    created_at = Column(DateTime, default=datetime.datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.datetime.utcnow, onupdate=datetime.datetime.utcnow)

    reservations = relationship("Reservation", back_populates="session")

    def __repr__(self):
        return f"<StudySession(title='{self.title}', teacher_id='{self.teacher_id}')>"

class Reservation(Base):
    __tablename__ = "reservations"

    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("study_sessions.id"), nullable=False)
    student_id = Column(String, nullable=False) # Assuming student_id is a string
    reserved_at = Column(DateTime, default=datetime.datetime.utcnow)

    session = relationship("StudySession", back_populates="reservations")

    def __repr__(self):
        return f"<Reservation(session_id='{self.session_id}', student_id='{self.student_id}')>"
