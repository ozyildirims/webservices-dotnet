from flask import Flask, jsonify
from flasgger import Swagger # Import Swagger
from app.config import Config
from app.extensions import db, migrate, bcrypt, jwt
# We will import and register blueprints later

def create_app(config_class=Config):
    app = Flask(__name__)
    app.config.from_object(config_class)

    # Initialize Flasgger
    swagger_config = Swagger.DEFAULT_CONFIG.copy()
    # Update with any custom configs from app.config['SWAGGER']
    custom_swagger_config = app.config.get('SWAGGER', {})

    # Define security definitions for JWT Bearer token
    custom_swagger_config.setdefault('components', {}).setdefault('securitySchemes', {
        'bearerAuth': {
            'type': 'http',
            'scheme': 'bearer',
            'bearerFormat': 'JWT'
        }
    })
    # Optional: define a global security requirement if most/all endpoints need it
    # custom_swagger_config.setdefault('security', [{'bearerAuth': []}])

    swagger_config.update(custom_swagger_config)
    Swagger(app, config=swagger_config)
    # app = Flask(__name__) # Duplicate: app is already created
    # app.config.from_object(config_class) # Duplicate: config already loaded

    # Initialize Flask extensions
    db.init_app(app)
    migrate.init_app(app, db)
    bcrypt.init_app(app)
    jwt.init_app(app)

    # Register blueprints
    from app.routes import auth_bp, parents_bp
    app.register_blueprint(auth_bp, url_prefix='/api/auth')
    app.register_blueprint(parents_bp, url_prefix='/api/parents')

    # from app.routes.users import bp as users_bp
    # app.register_blueprint(users_bp, url_prefix='/api/users')


    # Configure JWT error handlers
    # Note: These handlers are examples. You might want to customize them further.
    @jwt.unauthorized_loader
    def unauthorized_response(callback):
        return jsonify({
            'msg': 'Missing Authorization Header'
        }), 401

    @jwt.invalid_token_loader
    def invalid_token_response(callback):
        # callback is the error string
        return jsonify({
            'msg': 'Invalid token',
            'error': callback
        }), 422 # Unprocessable Entity is often used for invalid tokens

    @jwt.expired_token_loader
    def expired_token_response(jwt_header, jwt_payload):
        return jsonify({
            'msg': 'Token has expired'
        }), 401

    @app.route('/health')
    def health_check():
        return "OK", 200

    return app
