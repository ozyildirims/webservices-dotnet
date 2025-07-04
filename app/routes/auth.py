from flask import Blueprint, request, jsonify
from app.models import User, UserRole
from app.extensions import db, jwt
from flask_jwt_extended import create_access_token, jwt_required, get_jwt_identity, get_jwt # Added get_jwt
from app.utils import admin_required # Import the decorator
from sqlalchemy.exc import IntegrityError

bp = Blueprint('auth', __name__)

@bp.route('/register', methods=['POST'])
def register():
    """
    Register a new user.
    ---
    tags:
      - Auth
    summary: Creates a new user with a specified role.
    description: Allows registration for students, teachers, parents. Admin creation is typically handled by seeding.
    requestBody:
      required: true
      content:
        application/json:
          schema:
            type: object
            required:
              - email
              - password
              - role
            properties:
              email:
                type: string
                format: email
                description: User's email address.
                example: user@example.com
              password:
                type: string
                format: password
                description: User's password (min length can be enforced by frontend/backend).
                example: "Str0ngP@ssw0rd!"
              role:
                type: string
                enum: ["student", "teacher", "parent", "guest"]
                description: Role of the user.
                example: "student"
              first_name:
                type: string
                description: User's first name.
                example: "John"
              last_name:
                type: string
                description: User's last name.
                example: "Doe"
    responses:
      201:
        description: User registered successfully.
        content:
          application/json:
            schema:
              type: object
              properties:
                msg:
                  type: string
                  example: User registered successfully
                user:
                  type: object
                  properties:
                    id:
                      type: integer
                      example: 1
                    email:
                      type: string
                      example: user@example.com
                    role:
                      type: string
                      example: student
                    first_name:
                      type: string
                      example: John
                    last_name:
                      type: string
                      example: Doe
      400:
        description: Bad request (e.g., missing fields, invalid role, email already exists).
        content:
          application/json:
            schema:
              type: object
              properties:
                msg:
                  type: string
                  example: "Missing email or password"
    """
    data = request.get_json()
    if not data:
        return jsonify({"msg": "Missing JSON in request"}), 400

    email = data.get('email')
    password = data.get('password')
    role_str = data.get('role', 'guest') # Default role to guest if not provided
    first_name = data.get('first_name')
    last_name = data.get('last_name')

    if not email or not password:
        return jsonify({"msg": "Missing email or password"}), 400

    try:
        role = UserRole[role_str.upper()]
    except KeyError:
        return jsonify({"msg": f"Invalid role: {role_str}. Valid roles are: {[r.value for r in UserRole]}"}), 400

    if User.query.filter_by(email=email).first():
        return jsonify({"msg": "Email address already registered"}), 400

    user = User(email=email, role=role, first_name=first_name, last_name=last_name)
    user.set_password(password)

    try:
        db.session.add(user)
        db.session.commit()
    except IntegrityError: # Catch race conditions or other DB errors
        db.session.rollback()
        return jsonify({"msg": "Error creating user. Email might already exist or other database error."}), 400
    except Exception as e:
        db.session.rollback()
        # Log the exception e
        return jsonify({"msg": "An unexpected error occurred during registration."}), 500


    return jsonify({
        "msg": "User registered successfully",
        "user": {
            "id": user.id,
            "email": user.email,
            "role": user.role.value,
            "first_name": user.first_name,
            "last_name": user.last_name
        }
    }), 201


@bp.route('/login', methods=['POST'])
def login():
    """
    Login an existing user.
    ---
    tags:
      - Auth
    summary: Authenticates a user and returns a JWT.
    requestBody:
      required: true
      content:
        application/json:
          schema:
            type: object
            required:
              - email
              - password
            properties:
              email:
                type: string
                format: email
                description: User's email address.
                example: user@example.com
              password:
                type: string
                format: password
                description: User's password.
                example: "Str0ngP@ssw0rd!"
    responses:
      200:
        description: Login successful, JWT returned.
        content:
          application/json:
            schema:
              type: object
              properties:
                access_token:
                  type: string
                  description: JWT access token.
                  example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
      400:
        description: Bad request (e.g., missing email or password).
        content:
          application/json:
            schema:
              type: object
              properties:
                msg:
                  type: string
                  example: "Missing email or password"
      401:
        description: Unauthorized (e.g., bad email or password).
        content:
          application/json:
            schema:
              type: object
              properties:
                msg:
                  type: string
                  example: "Bad email or password"
    """
    data = request.get_json()
    if not data:
        return jsonify({"msg": "Missing JSON in request"}), 400

    email = data.get('email')
    password = data.get('password')

    if not email or not password:
        return jsonify({"msg": "Missing email or password"}), 400

    user = User.query.filter_by(email=email).first()

    if user and user.check_password(password):
        # Create a dictionary with the claims for the JWT
        # We can add more user details to the token if needed, but keep it minimal
        identity_claims = {
            "id": user.id,
            "email": user.email,
            "role": user.role.value
        }
        access_token = create_access_token(identity=user.email, additional_claims=identity_claims)
        return jsonify(access_token=access_token), 200
    else:
        return jsonify({"msg": "Bad email or password"}), 401

@bp.route('/admin_test', methods=['GET'])
@admin_required
def admin_test_route():
    current_user_email = get_jwt_identity() # Get user email from token
    claims = get_jwt() # If you need full claims
    user_role = claims.get('role')
    return jsonify(logged_in_as=current_user_email, role=user_role, msg="Admin test route successful!"), 200


@bp.route('/me', methods=['GET'])
@jwt_required() # Requires a valid JWT
def get_current_user_profile():
    """
    Get current logged-in user's profile.
    ---
    tags:
      - Auth
    summary: Fetches the profile information of the currently authenticated user.
    security:
      - bearerAuth: []
    responses:
      200:
        description: User profile data.
        content:
          application/json:
            schema:
              type: object
              properties:
                id:
                  type: integer
                  example: 1
                email:
                  type: string
                  format: email
                  example: user@example.com
                first_name:
                  type: string
                  example: "John"
                last_name:
                  type: string
                  example: "Doe"
                role:
                  type: string
                  enum: ["student", "teacher", "parent", "admin", "guest"]
                  example: "student"
      401:
        description: Unauthorized (e.g., token missing or expired).
        content:
          application/json:
            schema:
              type: object
              properties:
                msg:
                  type: string
                  example: "Missing Authorization Header"
      422:
        description: Unprocessable Entity (e.g., invalid token format).
        content:
          application/json:
            schema:
              type: object
              properties:
                msg:
                  type: string
                  example: "Invalid token"
    """
    current_user_email = get_jwt_identity() # This is the 'identity' we set during login
    user = User.query.filter_by(email=current_user_email).first_or_404()

    # We can decide what information to return.
    # For security/privacy, avoid returning sensitive info like password_hash.
    user_data = {
        "id": user.id,
        "email": user.email,
        "first_name": user.first_name,
        "last_name": user.last_name,
        "role": user.role.value
    }

    # If the user is a parent, we could optionally include linked children's basic info
    # For now, keeping it simple. This could be a separate endpoint or an expansion parameter.

    return jsonify(user_data), 200
