import json
from tests.base import BaseTestCase
from app.models import User, UserRole, parents_students_table
from app.extensions import db

class ParentStudentLinkingTestCase(BaseTestCase):

    def setUp(self):
        super().setUp()
        # Create users: admin, parent1, parent2, student1, student2, teacher
        self.admin = User(email='admin_ps@example.com', role=UserRole.ADMIN)
        self.admin.set_password('adminpass')

        self.parent1 = User(email='parent1_ps@example.com', role=UserRole.PARENT, first_name="Parent", last_name="One")
        self.parent1.set_password('parent1pass')

        self.parent2 = User(email='parent2_ps@example.com', role=UserRole.PARENT, first_name="Parent", last_name="Two")
        self.parent2.set_password('parent2pass')

        self.student1 = User(email='student1_ps@example.com', role=UserRole.STUDENT, first_name="Student", last_name="Alpha")
        self.student1.set_password('student1pass')

        self.student2 = User(email='student2_ps@example.com', role=UserRole.STUDENT, first_name="Student", last_name="Beta")
        self.student2.set_password('student2pass')

        self.teacher = User(email='teacher_ps@example.com', role=UserRole.TEACHER)
        self.teacher.set_password('teacherpass')

        db.session.add_all([self.admin, self.parent1, self.parent2, self.student1, self.student2, self.teacher])
        db.session.commit()

        # Get tokens
        self.admin_token = self._get_token('admin_ps@example.com', 'adminpass')
        self.parent1_token = self._get_token('parent1_ps@example.com', 'parent1pass')
        self.parent2_token = self._get_token('parent2_ps@example.com', 'parent2pass')
        self.student1_token = self._get_token('student1_ps@example.com', 'student1pass') # Not used for linking directly but good for other tests
        self.teacher_token = self._get_token('teacher_ps@example.com', 'teacherpass')


    def _get_token(self, email, password):
        response = self.client.post('/api/auth/login', json={'email': email, 'password': password})
        self.assertEqual(response.status_code, 200, f"Login failed for {email}")
        return response.get_json()['access_token']

    def test_admin_link_student_to_parent(self):
        response = self.client.post(
            f'/api/parents/{self.parent1.id}/link_student',
            headers={'Authorization': f'Bearer {self.admin_token}'},
            json={'student_id': self.student1.id}
        )
        self.assertEqual(response.status_code, 201)
        data = response.get_json()
        self.assertIn("successfully linked", data['msg'])
        self.assertTrue(self.student1 in self.parent1.children)

    def test_parent_link_student_to_own_profile(self):
        response = self.client.post(
            f'/api/parents/{self.parent1.id}/link_student',
            headers={'Authorization': f'Bearer {self.parent1_token}'},
            json={'student_id': self.student2.id}
        )
        self.assertEqual(response.status_code, 201)
        self.assertTrue(self.student2 in self.parent1.children)

    def test_parent_cannot_link_student_to_another_parent_profile(self):
        response = self.client.post(
            f'/api/parents/{self.parent2.id}/link_student', # Target parent2
            headers={'Authorization': f'Bearer {self.parent1_token}'}, # Action by parent1
            json={'student_id': self.student1.id}
        )
        self.assertEqual(response.status_code, 403) # Forbidden

    def test_link_non_student_fails(self):
        response = self.client.post(
            f'/api/parents/{self.parent1.id}/link_student',
            headers={'Authorization': f'Bearer {self.admin_token}'},
            json={'student_id': self.teacher.id} # Trying to link a teacher as a student
        )
        self.assertEqual(response.status_code, 400)
        data = response.get_json()
        self.assertIn("is not a student", data['msg'])

    def test_link_to_non_parent_fails(self):
        response = self.client.post(
            f'/api/parents/{self.teacher.id}/link_student', # Target is a teacher
            headers={'Authorization': f'Bearer {self.admin_token}'},
            json={'student_id': self.student1.id}
        )
        self.assertEqual(response.status_code, 400)
        data = response.get_json()
        self.assertIn("is not a parent", data['msg'])

    def test_link_student_already_linked(self):
        # First link
        self.client.post(
            f'/api/parents/{self.parent1.id}/link_student',
            headers={'Authorization': f'Bearer {self.admin_token}'},
            json={'student_id': self.student1.id}
        )
        # Attempt to link again
        response = self.client.post(
            f'/api/parents/{self.parent1.id}/link_student',
            headers={'Authorization': f'Bearer {self.admin_token}'},
            json={'student_id': self.student1.id}
        )
        self.assertEqual(response.status_code, 409) # Conflict
        data = response.get_json()
        self.assertIn("already linked", data['msg'])

    def test_get_linked_students_by_parent_self(self):
        # Link student1 and student2 to parent1
        self.parent1.children.append(self.student1)
        self.parent1.children.append(self.student2)
        db.session.commit()

        response = self.client.get(
            f'/api/parents/{self.parent1.id}/students',
            headers={'Authorization': f'Bearer {self.parent1_token}'}
        )
        self.assertEqual(response.status_code, 200)
        data = response.get_json()
        self.assertEqual(len(data['students']), 2)
        student_ids = {s['id'] for s in data['students']}
        self.assertIn(self.student1.id, student_ids)
        self.assertIn(self.student2.id, student_ids)

    def test_get_linked_students_by_admin(self):
        self.parent1.children.append(self.student1)
        db.session.commit()
        response = self.client.get(
            f'/api/parents/{self.parent1.id}/students',
            headers={'Authorization': f'Bearer {self.admin_token}'}
        )
        self.assertEqual(response.status_code, 200)
        data = response.get_json()
        self.assertEqual(len(data['students']), 1)
        self.assertEqual(data['students'][0]['id'], self.student1.id)

    def test_get_linked_students_by_teacher(self):
        self.parent1.children.append(self.student1)
        db.session.commit()
        response = self.client.get(
            f'/api/parents/{self.parent1.id}/students',
            headers={'Authorization': f'Bearer {self.teacher_token}'}
        )
        self.assertEqual(response.status_code, 200)
        data = response.get_json()
        self.assertEqual(len(data['students']), 1)

    def test_parent_cannot_get_another_parents_students(self):
        self.parent2.children.append(self.student2) # parent2 has student2
        db.session.commit()

        response = self.client.get(
            f'/api/parents/{self.parent2.id}/students', # Target parent2's students
            headers={'Authorization': f'Bearer {self.parent1_token}'} # Action by parent1
        )
        self.assertEqual(response.status_code, 403) # Forbidden

    def test_student_cannot_get_linked_students(self):
        # Student is not in [ADMIN, PARENT, TEACHER] for this endpoint
        self.parent1.children.append(self.student1)
        db.session.commit()
        response = self.client.get(
            f'/api/parents/{self.parent1.id}/students',
            headers={'Authorization': f'Bearer {self.student1_token}'}
        )
        self.assertEqual(response.status_code, 403)

if __name__ == '__main__':
    unittest.main()
