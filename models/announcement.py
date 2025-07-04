from sqlalchemy import Column, Integer, String, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from sqlalchemy.ext.declarative import declarative_base
import datetime

Base = declarative_base()

class Announcement(Base):
    __tablename__ = "announcements"

    id = Column(Integer, primary_key=True, index=True)
    title = Column(String, index=True)
    content = Column(String)
    creator_id = Column(Integer, ForeignKey("users.id")) # Assuming a User model exists
    creation_date = Column(DateTime, default=datetime.datetime.utcnow)
    status = Column(String, default="active") # active, archived

    creator = relationship("User") # Assuming a User model exists
