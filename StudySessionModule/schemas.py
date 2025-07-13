from pydantic import BaseModel, EmailStr
from typing import Optional, List
import datetime

# User Schemas (basic for identification)
class UserBase(BaseModel):
    id: str # Could be username or a unique ID string
    email: Optional[EmailStr] = None

class User(UserBase):
    class Config:
        orm_mode = True

# Reservation Schemas
class ReservationBase(BaseModel):
    student_id: str

class ReservationCreate(ReservationBase):
    session_id: int

class ReservationRead(ReservationBase):
    id: int
    session_id: int
    reserved_at: datetime.datetime

    class Config:
        orm_mode = True

# Study Session Schemas
class StudySessionBase(BaseModel):
    title: str
    description: Optional[str] = None
    start_time: datetime.datetime
    end_time: datetime.datetime
    capacity: int

class StudySessionCreate(StudySessionBase):
    pass

class StudySessionRead(StudySessionBase):
    id: int
    teacher_id: str
    created_at: datetime.datetime
    updated_at: datetime.datetime
    reservations_count: Optional[int] = 0 # To show current number of reservations

    class Config:
        orm_mode = True

class StudySessionWithReservations(StudySessionRead):
    reservations: List[ReservationRead] = []

    class Config:
        orm_mode = True

class StudySessionUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    start_time: Optional[datetime.datetime] = None
    end_time: Optional[datetime.datetime] = None
    capacity: Optional[int] = None
