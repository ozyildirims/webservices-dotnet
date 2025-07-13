from fastapi import Depends, HTTPException, status
from pydantic import BaseModel
from typing import Optional

# --- User Simulation ---
# In a real application, this would come from a database and a proper auth system (e.g., OAuth2)
class User(BaseModel):
    id: str
    email: Optional[str] = None
    roles: list[str] = [] # e.g., ["student", "teacher"]

# Dummy user data
fake_users_db = {
    "teacher1": {"id": "teacher1", "email": "teacher1@example.com", "roles": ["teacher"]},
    "student1": {"id": "student1", "email": "student1@example.com", "roles": ["student"]},
    "student2": {"id": "student2", "email": "student2@example.com", "roles": ["student"]},
    "admin": {"id": "admin", "email": "admin@example.com", "roles": ["teacher", "admin"]} # Admin can also be a teacher
}

# --- Authentication Simulation ---

# This function simulates getting the current user based on a token or session.
# For simplicity, we'll use a header "X-User-ID" to identify the user.
async def get_current_user(x_user_id: Optional[str] = None) -> Optional[User]:
    if x_user_id and x_user_id in fake_users_db:
        user_data = fake_users_db[x_user_id]
        return User(**user_data)
    return None

async def get_current_active_user(current_user: User = Depends(get_current_user)) -> User:
    if not current_user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Not authenticated",
            headers={"WWW-Authenticate": "Bearer"}, # Though not using Bearer, it's a common header
        )
    return current_user

# --- Authorization Dependencies ---

async def is_teacher(current_user: User = Depends(get_current_active_user)) -> User:
    if "teacher" not in current_user.roles:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Operation not permitted: Requires teacher role."
        )
    return current_user

async def is_student(current_user: User = Depends(get_current_active_user)) -> User:
    if "student" not in current_user.roles:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Operation not permitted: Requires student role."
        )
    return current_user

# Example of a more specific check: is the user the owner of a resource?
# This would typically be used within an endpoint that has access to the resource.
# For now, this is a placeholder concept.
# async def is_owner(resource_owner_id: str, current_user: User = Depends(get_current_active_user)):
#     if resource_owner_id != current_user.id:
#         raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Not the owner")
#     return current_user

# To make it easier to test without setting headers in every test,
# we can create a dependency that can be overridden.
# This is more for testing convenience.
DUMMY_TEACHER = User(id="test_teacher", roles=["teacher"])
DUMMY_STUDENT = User(id="test_student", roles=["student"])

async def get_current_user_for_testing(user_id: Optional[str] = "teacher1"): # Default to a teacher for some tests
    if user_id and user_id in fake_users_db:
        return User(**fake_users_db[user_id])
    elif user_id == "test_teacher":
        return DUMMY_TEACHER
    elif user_id == "test_student":
        return DUMMY_STUDENT
    return None

async def get_current_active_user_for_testing(current_user: User = Depends(get_current_user_for_testing)):
    if not current_user:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Not authenticated for testing")
    return current_user

async def is_teacher_for_testing(current_user: User = Depends(get_current_active_user_for_testing)):
    if "teacher" not in current_user.roles:
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Requires teacher role for testing")
    return current_user

async def is_student_for_testing(current_user: User = Depends(get_current_active_user_for_testing)):
    if "student" not in current_user.roles:
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Requires student role for testing")
    return current_user
