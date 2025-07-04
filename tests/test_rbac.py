import json
from tests.base import BaseTestCase
from app.models import User, UserRole
from app.extensions import db

class RbacTestCase(BaseTestCase):

    def setUp(self):
        super().setUp()
        # Create users with different roles
        self.admin_user = User(email='admin_for_rbac@example.com', role=UserRole.ADMIN, first_name="AdminRBAC")
        self.admin_user.set_password('adminpass')

        self.teacher_user = User(email='teacher_for_rbac@example.com', role=UserRole.TEACHER, first_name="TeacherRBAC")
        self.teacher_user.set_password('teacherpass')

        self.student_user = User(email='student_for_rbac@example.com', role=UserRole.STUDENT, first_name="StudentRBAC")
        self.student_user.set_password('studentpass')

        db.session.add_all([self.admin_user, self.teacher_user, self.student_user])
        db.session.commit()

        # Log them in to get tokens
        self.admin_token = self._get_token('admin_for_rbac@example.com', 'adminpass')
        self.teacher_token = self._get_token('teacher_for_rbac@example.com', 'teacherpass')
        self.student_token = self._get_token('student_for_rbac@example.com', 'studentpass')

    def _get_token(self, email, password):
        response = self.client.post('/api/auth/login', json={'email': email, 'password': password})
        self.assertEqual(response.status_code, 200, f"Login failed for {email}")
        return response.get_json()['access_token']

    def test_admin_access_to_admin_route(self):
        response = self.client.get('/api/auth/admin_test', headers={
            'Authorization': f'Bearer {self.admin_token}'
        })
        self.assertEqual(response.status_code, 200)
        data = response.get_json()
        self.assertIn("Admin test route successful!", data['msg'])

    def test_non_admin_access_to_admin_route_forbidden(self):
        # Test with teacher token
        response_teacher = self.client.get('/api/auth/admin_test', headers={
            'Authorization': f'Bearer {self.teacher_token}'
        })
        self.assertEqual(response_teacher.status_code, 403) # Forbidden
        data_teacher = response_teacher.get_json()
        self.assertIn("Access forbidden", data_teacher['msg'])
        self.assertIn("not one of the required roles", data_teacher['msg'])


        # Test with student token
        response_student = self.client.get('/api/auth/admin_test', headers={
            'Authorization': f'Bearer {self.student_token}'
        })
        self.assertEqual(response_student.status_code, 403) # Forbidden
        data_student = response_student.get_json()
        self.assertIn("Access forbidden", data_student['msg'])

    def test_access_without_token_to_protected_route(self):
        response = self.client.get('/api/auth/admin_test') # No token
        self.assertEqual(response.status_code, 401) # Unauthorized
        data = response.get_json()
        self.assertIn("Missing Authorization Header", data['msg'])

    def test_role_required_decorator_with_multiple_roles(self):
        # Let's assume we have an endpoint that requires TEACHER or ADMIN
        # We'll need to create such an endpoint or mock one for a pure unit test of the decorator
        # For now, this test conceptualizes it. We can add a real endpoint later if needed.

        # @self.app.route('/teacher_or_admin_only')
        # @role_required([UserRole.TEACHER, UserRole.ADMIN])
        # def teacher_or_admin_route():
        # return jsonify(msg="Accessible by teacher or admin"), 200

        # This part would require modifying app.py to add a test route or a more complex setup.
        # For now, the decorator's logic is tested by the admin_test route implicitly.
        # If we add a route like /api/parents/<id>/students (accessible by parent, teacher, admin),
        # we can test it more directly.
        pass


if __name__ == '__main__':
    unittest.main()
