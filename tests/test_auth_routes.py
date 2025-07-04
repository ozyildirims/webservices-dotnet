import json
from tests.base import BaseTestCase
from app.models import User, UserRole
from app.extensions import db

class AuthRoutesTestCase(BaseTestCase):

    def test_register_user_success(self):
        response = self.client.post('/api/auth/register', json={
            "email": "newuser@example.com",
            "password": "newpassword123",
            "role": "student",
            "first_name": "New",
            "last_name": "User"
        })
        self.assertEqual(response.status_code, 201)
        data = response.get_json()
        self.assertEqual(data['user']['email'], "newuser@example.com")
        self.assertEqual(data['user']['role'], "student")

        # Verify user in DB
        user = User.query.filter_by(email="newuser@example.com").first()
        self.assertIsNotNone(user)
        self.assertEqual(user.role, UserRole.STUDENT)

    def test_register_user_duplicate_email(self):
        # First registration
        self.client.post('/api/auth/register', json={
            "email": "test@example.com", "password": "pw1", "role": "student"
        })
        # Second registration with same email
        response = self.client.post('/api/auth/register', json={
            "email": "test@example.com", "password": "pw2", "role": "teacher"
        })
        self.assertEqual(response.status_code, 400)
        data = response.get_json()
        self.assertIn("Email address already registered", data['msg'])

    def test_register_user_invalid_role(self):
        response = self.client.post('/api/auth/register', json={
            "email": "invalidrole@example.com",
            "password": "password123",
            "role": "nonexistentrole"
        })
        self.assertEqual(response.status_code, 400)
        data = response.get_json()
        self.assertIn("Invalid role", data['msg'])

    def test_register_user_missing_fields(self):
        response = self.client.post('/api/auth/register', json={"email": "missing@example.com"})
        self.assertEqual(response.status_code, 400)
        data = response.get_json()
        self.assertIn("Missing email or password", data['msg']) # Based on current error message

    def test_login_success(self):
        # First, register a user
        self.client.post('/api/auth/register', json={
            "email": "loginuser@example.com", "password": "loginpass", "role": "teacher"
        })
        # Attempt login
        response = self.client.post('/api/auth/login', json={
            "email": "loginuser@example.com", "password": "loginpass"
        })
        self.assertEqual(response.status_code, 200)
        data = response.get_json()
        self.assertIn('access_token', data)

    def test_login_wrong_password(self):
        self.client.post('/api/auth/register', json={
            "email": "loginuser2@example.com", "password": "correctpass", "role": "student"
        })
        response = self.client.post('/api/auth/login', json={
            "email": "loginuser2@example.com", "password": "wrongpass"
        })
        self.assertEqual(response.status_code, 401) # Unauthorized
        data = response.get_json()
        self.assertIn("Bad email or password", data['msg'])

    def test_login_nonexistent_user(self):
        response = self.client.post('/api/auth/login', json={
            "email": "nosuchuser@example.com", "password": "anypass"
        })
        self.assertEqual(response.status_code, 401)
        data = response.get_json()
        self.assertIn("Bad email or password", data['msg'])

    def test_get_me_success(self):
        # Register and login
        self.client.post('/api/auth/register', json={
            "email": "me_user@example.com", "password": "mepass", "role": "parent",
            "first_name": "Test", "last_name": "Parent"
        })
        login_response = self.client.post('/api/auth/login', json={
            "email": "me_user@example.com", "password": "mepass"
        })
        access_token = login_response.get_json()['access_token']

        # Request /me
        response = self.client.get('/api/auth/me', headers={
            'Authorization': f'Bearer {access_token}'
        })
        self.assertEqual(response.status_code, 200)
        data = response.get_json()
        self.assertEqual(data['email'], "me_user@example.com")
        self.assertEqual(data['role'], "parent")
        self.assertEqual(data['first_name'], "Test")

    def test_get_me_no_token(self):
        response = self.client.get('/api/auth/me')
        self.assertEqual(response.status_code, 401) # Unauthorized - missing token
        data = response.get_json()
        self.assertIn("Missing Authorization Header", data['msg'])

    def test_get_me_invalid_token(self):
        response = self.client.get('/api/auth/me', headers={
            'Authorization': 'Bearer invalidtoken123'
        })
        self.assertEqual(response.status_code, 422) # Unprocessable Entity for invalid token
        data = response.get_json()
        self.assertIn("Invalid token", data['msg'])

if __name__ == '__main__':
    unittest.main()
