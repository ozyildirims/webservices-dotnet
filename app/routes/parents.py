from flask import Blueprint, request, jsonify
from app.models import User, UserRole, parents_students_table
from app.extensions import db
from app.utils import role_required, admin_required, parent_required # For protecting routes
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from sqlalchemy.exc import IntegrityError

bp = Blueprint('parents', __name__)

@bp.route('/<int:parent_id>/link_student', methods=['POST'])
@role_required([UserRole.ADMIN, UserRole.PARENT])
def link_student_to_parent(parent_id):
    """
    Link a student to a parent.
    ---
    tags:
      - Parents
    summary: Associates a student with a parent's profile.
    description: This can be done by an Admin or by the Parent themselves for their own profile.
    security:
      - bearerAuth: []
    parameters:
      - name: parent_id
        in: path
        required: true
        description: The ID of the parent to link the student to.
        schema:
          type: integer
    requestBody:
      required: true
      content:
        application/json:
          schema:
            type: object
            required:
              - student_id
            properties:
              student_id:
                type: integer
                description: The ID of the student to be linked.
                example: 102
    responses:
      201:
        description: Student successfully linked to parent.
        content:
          application/json:
            schema:
              type: object
              properties:
                msg:
                  type: string
                  example: "Student student_email@example.com (ID: 102) successfully linked to parent parent_email@example.com (ID: 5)."
      400:
        description: Bad request (e.g., missing student_id, user is not a student/parent).
      401:
        description: Unauthorized (token missing or invalid).
      403:
        description: Forbidden (e.g., parent trying to link to another parent's profile).
      404:
        description: Not Found (e.g., parent_id or student_id does not exist).
      409:
        description: Conflict (e.g., student already linked to this parent).
    """
    claims = get_jwt()
    current_user_id = claims.get('id')
    current_user_role = UserRole(claims.get('role'))

    parent = User.query.get_or_404(parent_id)
    if not parent.is_parent:
        return jsonify(msg=f"User ID {parent_id} is not a parent."), 400

    # Ensure only admins or the parent themselves can perform this action
    if current_user_role != UserRole.ADMIN and current_user_id != parent.id:
        return jsonify(msg="Forbidden: You can only link students to your own parent profile or you must be an admin."), 403

    data = request.get_json()
    if not data or 'student_id' not in data:
        return jsonify(msg="Missing student_id in request body"), 400

    student_id = data.get('student_id')
    student = User.query.get_or_404(student_id)

    if not student.is_student:
        return jsonify(msg=f"User ID {student_id} is not a student."), 400

    # Check if already linked
    if student in parent.children:
        return jsonify(msg=f"Student {student_id} is already linked to parent {parent_id}."), 409 # Conflict

    parent.children.append(student)
    try:
        db.session.commit()
    except IntegrityError: # Should not happen if checks are correct, but good for safety
        db.session.rollback()
        return jsonify(msg="Database error occurred while linking."), 500

    return jsonify(msg=f"Student {student.email} (ID: {student_id}) successfully linked to parent {parent.email} (ID: {parent_id})."), 201


@bp.route('/<int:parent_id>/students', methods=['GET'])
@role_required([UserRole.ADMIN, UserRole.PARENT, UserRole.TEACHER])
def get_linked_students(parent_id):
    """
    Get all students linked to a specific parent.
    ---
    tags:
      - Parents
    summary: Retrieves a list of students associated with a given parent.
    description: Accessible by Admins, the Parent themselves, or Teachers.
    security:
      - bearerAuth: []
    parameters:
      - name: parent_id
        in: path
        required: true
        description: The ID of the parent whose linked students are to be retrieved.
        schema:
          type: integer
    responses:
      200:
        description: A list of students linked to the parent.
        content:
          application/json:
            schema:
              type: object
              properties:
                students:
                  type: array
                  items:
                    type: object
                    properties:
                      id:
                        type: integer
                        example: 102
                      email:
                        type: string
                        format: email
                        example: "student@example.com"
                      first_name:
                        type: string
                        example: "Student"
                      last_name:
                        type: string
                        example: "Alpha"
      400:
        description: Bad request (e.g., user_id does not correspond to a parent).
      401:
        description: Unauthorized (token missing or invalid).
      403:
        description: Forbidden (e.g., parent trying to access another parent's students).
      404:
        description: Not Found (e.g., parent_id does not exist).
    """
    claims = get_jwt()
    current_user_id = claims.get('id')
    current_user_role = UserRole(claims.get('role'))

    parent = User.query.get_or_404(parent_id)
    if not parent.is_parent:
        return jsonify(msg=f"User ID {parent_id} is not a parent."), 400

    # Authorization: Admin can see any. Parent can only see their own. Teacher can see any (for now, could be refined).
    if current_user_role == UserRole.PARENT and current_user_id != parent.id:
        return jsonify(msg="Forbidden: You can only view students linked to your own parent profile."), 403

    students_data = [{
        "id": student.id,
        "email": student.email,
        "first_name": student.first_name,
        "last_name": student.last_name
    } for student in parent.children]

    return jsonify(students=students_data), 200
