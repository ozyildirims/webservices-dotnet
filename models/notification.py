from sqlalchemy import Column, Integer, String, DateTime, ForeignKey, JSON
from sqlalchemy.orm import relationship
from sqlalchemy.ext.declarative import declarative_base
import datetime

Base = declarative_base()

class Notification(Base):
    __tablename__ = "notifications"

    id = Column(Integer, primary_key=True, index=True)
    title = Column(String, index=True)
    content = Column(String)
    creator_id = Column(Integer, ForeignKey("users.id")) # Assuming a User model exists
    creation_date = Column(DateTime, default=datetime.datetime.utcnow)

    # Targeting information
    target_type = Column(String) # 'all', 'role', 'user'
    target_roles = Column(JSON, nullable=True) # List of roles if target_type is 'role'
    target_user_ids = Column(JSON, nullable=True) # List of user IDs if target_type is 'user'

    schedule_time = Column(DateTime, nullable=True) # For scheduled notifications
    delivery_status = Column(String, default="pending") # pending, sent, failed
    status = Column(String, default="active") # active, archived

    creator = relationship("User") # Assuming a User model exists


class UserNotification(Base):
    __tablename__ = "user_notifications"

    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, ForeignKey("users.id"), index=True)
    notification_id = Column(Integer, ForeignKey("notifications.id"), index=True)
    read_status = Column(String, default="unread") # unread, read
    received_at = Column(DateTime, default=datetime.datetime.utcnow)

    user = relationship("User")
    notification = relationship("Notification")
