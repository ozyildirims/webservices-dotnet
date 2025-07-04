from .auth import bp as auth_bp
from .parents import bp as parents_bp
# We will add other blueprints here as we create them

__all__ = ["auth_bp", "parents_bp"]
