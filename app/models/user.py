import enum
from sqlalchemy import Column, Integer, String, Enum, Table, ForeignKey
from sqlalchemy.orm import relationship
from app.extensions import db, bcrypt

class UserRole(enum.Enum):
    STUDENT = "student"
    TEACHER = "teacher"
    PARENT = "parent"
    ADMIN = "admin"
    GUEST = "guest"

# Association table for the many-to-many relationship between parents and students
parents_students_table = Table('parents_students', db.Model.metadata,
    Column('parent_id', Integer, ForeignKey('users.id'), primary_key=True),
    Column('student_id', Integer, ForeignKey('users.id'), primary_key=True)
)

class User(db.Model):
    __tablename__ = "users"

    id = Column(Integer, primary_key=True)
    email = Column(String(120), unique=True, nullable=False)
    password_hash = Column(String(128), nullable=False)
    first_name = Column(String(50), nullable=True)
    last_name = Column(String(50), nullable=True)
    role = Column(Enum(UserRole), nullable=False, default=UserRole.GUEST)

    # For parents, this relationship lists their children
    children = relationship(
        "User",
        secondary=parents_students_table,
        primaryjoin=(parents_students_table.c.parent_id == id),
        secondaryjoin=(parents_students_table.c.student_id == id),
        backref="parents"
    )

    def __repr__(self):
        return f"<User {self.email} ({self.role.value})>"

    def set_password(self, password):
        self.password_hash = bcrypt.generate_password_hash(password).decode('utf-8')

    def check_password(self, password):
        return bcrypt.check_password_hash(self.password_hash, password)

    @property
    def is_admin(self):
        return self.role == UserRole.ADMIN

    @property
    def is_parent(self):
        return self.role == UserRole.PARENT

    @property
    def is_student(self):
        return self.role == UserRole.STUDENT

    @property
    def is_teacher(self):
        return self.role == UserRole.TEACHER
