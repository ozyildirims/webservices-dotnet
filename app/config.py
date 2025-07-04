import os
from datetime import timedelta

class Config:
    SECRET_KEY = os.environ.get('SECRET_KEY') or 'you-will-never-guess'
    SQLALCHEMY_DATABASE_URI = os.environ.get('DATABASE_URL') or \
        'sqlite:///' + os.path.join(os.path.abspath(os.path.dirname(__file__)), 'app.db')
    SQLALCHEMY_TRACK_MODIFICATIONS = False

    JWT_SECRET_KEY = os.environ.get('JWT_SECRET_KEY') or 'super-secret-jwt'
    JWT_ACCESS_TOKEN_EXPIRES = timedelta(days=7)
    # For refresh tokens, if we add them later
    # JWT_REFRESH_TOKEN_EXPIRES = timedelta(days=30)

    # Optional: for Flasgger (OpenAPI documentation)
    SWAGGER = {
        'title': 'Education Platform API',
        'uiversion': 3,
        'openapi': '3.0.2',
        'doc_expansion': 'list', # or 'full' or 'none'
        'specs_route': '/apidocs/' # URL for Swagger UI
    }
