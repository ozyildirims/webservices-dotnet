from dotenv import load_dotenv
load_dotenv() # Load environment variables from .env file

import os
import click # For creating CLI commands
from flask.cli import with_appcontext
from app.app import create_app, db
from app.models import User, UserRole # Make sure models are imported for migrations

app = create_app()

# Environment variables for the default admin
DEFAULT_ADMIN_EMAIL = os.environ.get('DEFAULT_ADMIN_EMAIL', 'admin@example.com')
DEFAULT_ADMIN_PASSWORD = os.environ.get('DEFAULT_ADMIN_PASSWORD', 'AdminPassword123!')

@app.shell_context_processor
def make_shell_context():
    return {'db': db, 'User': User, 'UserRole': UserRole}

@click.command(name='seed_admin')
@with_appcontext
def seed_admin_command():
    """Creates an admin user if one doesn't exist or updates the default one."""
    admin_email = DEFAULT_ADMIN_EMAIL
    admin_password = DEFAULT_ADMIN_PASSWORD

    admin_user = User.query.filter_by(email=admin_email).first()
    if admin_user:
        if admin_user.role != UserRole.ADMIN:
            admin_user.role = UserRole.ADMIN
            click.echo(f"User {admin_email} already existed, updated role to ADMIN.")
        else:
            click.echo(f"Admin user {admin_email} already exists.")
        # Optionally update password if it's different, or add a flag to force update
        # admin_user.set_password(admin_password)
        # db.session.commit()
        # click.echo(f"Admin user {admin_email} password updated (if changed).")
    else:
        admin_user = User(
            email=admin_email,
            role=UserRole.ADMIN,
            first_name="Default",
            last_name="Admin"
        )
        admin_user.set_password(admin_password)
        db.session.add(admin_user)
        click.echo(f"Admin user {admin_email} created.")

    try:
        db.session.commit()
    except Exception as e:
        db.session.rollback()
        click.echo(f"Error during admin seeding: {e}", err=True)

app.cli.add_command(seed_admin_command)

if __name__ == '__main__':
    app.run(debug=True)
