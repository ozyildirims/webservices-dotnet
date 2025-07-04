from tests.base import BaseTestCase
from app.models import User, UserRole

class UserModelTestCase(BaseTestCase):
    def test_password_setter(self):
        u = User(email='test@example.com', role=UserRole.STUDENT)
        u.set_password('cat')
        self.assertIsNotNone(u.password_hash)
        self.assertNotEqual(u.password_hash, 'cat')

    def test_password_verification(self):
        u = User(email='test@example.com', role=UserRole.STUDENT)
        u.set_password('cat')
        self.assertTrue(u.check_password('cat'))
        self.assertFalse(u.check_password('dog'))

    def test_user_roles(self):
        admin = User(email='admin@example.com', role=UserRole.ADMIN)
        teacher = User(email='teacher@example.com', role=UserRole.TEACHER)
        parent = User(email='parent@example.com', role=UserRole.PARENT)
        student = User(email='student@example.com', role=UserRole.STUDENT)
        guest = User(email='guest@example.com', role=UserRole.GUEST) # Default

        self.assertTrue(admin.is_admin)
        self.assertFalse(teacher.is_admin)

        self.assertTrue(teacher.is_teacher)
        self.assertFalse(admin.is_teacher)

        self.assertTrue(parent.is_parent)
        self.assertTrue(student.is_student)

        # Check default role if not specified (though our registration forces a role)
        # default_user = User(email='default@example.com')
        # self.assertEqual(default_user.role, UserRole.GUEST) # Or whatever your default is

    def test_user_representation(self):
        u = User(email='repr@example.com', role=UserRole.ADMIN)
        self.assertEqual(repr(u), '<User repr@example.com (admin)>')

if __name__ == '__main__':
    unittest.main()
