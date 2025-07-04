from functools import wraps
from flask import jsonify
from flask_jwt_extended import verify_jwt_in_request, get_jwt
from app.models import UserRole

def role_required(required_roles):
    """
    Decorator to ensure the user has at least one of the required roles.
    `required_roles` can be a single UserRole enum or a list/tuple of UserRole enums.
    """
    if not isinstance(required_roles, (list, tuple)):
        required_roles = [required_roles]

    # Convert role names to UserRole enum members if they are strings
    processed_roles = []
    for role in required_roles:
        if isinstance(role, str):
            try:
                processed_roles.append(UserRole[role.upper()])
            except KeyError:
                # This would be a developer error, raise an exception or log it
                raise ValueError(f"Invalid role string '{role}' used in role_required decorator.")
        elif isinstance(role, UserRole):
            processed_roles.append(role)
        else:
            raise TypeError("Roles must be UserRole enums or valid role strings.")

    required_roles_set = set(processed_roles)

    def decorator(fn):
        @wraps(fn)
        def wrapper(*args, **kwargs):
            verify_jwt_in_request()
            claims = get_jwt()
            user_role_str = claims.get('role')

            if not user_role_str:
                return jsonify(msg="Missing role in token"), 403

            try:
                user_role = UserRole(user_role_str) # Convert string from token to Enum
            except ValueError:
                return jsonify(msg=f"Invalid role '{user_role_str}' in token"), 403

            if user_role not in required_roles_set:
                return jsonify(msg=f"Access forbidden: User role '{user_role.value}' is not one of the required roles."), 403

            return fn(*args, **kwargs)
        return wrapper
    return decorator

# Convenience decorators for specific roles
def admin_required(fn):
    return role_required(UserRole.ADMIN)(fn)

def teacher_required(fn):
    return role_required(UserRole.TEACHER)(fn)

def parent_required(fn):
    return role_required(UserRole.PARENT)(fn)

def student_required(fn):
    return role_required(UserRole.STUDENT)(fn)

# Example for a route accessible by multiple specific roles
def teacher_or_admin_required(fn):
    return role_required([UserRole.TEACHER, UserRole.ADMIN])(fn)
