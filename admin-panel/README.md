# Admin Panel

A modern, responsive admin panel built with Next.js, Tailwind CSS, and Hero UI for managing your .NET backend services.

## Features

- 🎨 **Modern UI**: Clean, responsive design using Tailwind CSS and Hero UI
- 📊 **Dashboard**: Overview with statistics and recent activity
- 👥 **Employee Management**: Full CRUD operations for employees
- 📝 **Exam Management**: Create, edit, and publish exams
- 📢 **Announcements**: Manage system announcements and notifications
- 📚 **Books Management**: Simple book catalog management
- 🚗 **Cars Management**: Vehicle fleet information
- 📈 **Analytics**: Data visualization and statistics
- ⚙️ **Settings**: System configuration and user preferences
- 🔐 **Authentication Ready**: JWT token support
- 📱 **Mobile Responsive**: Works on all device sizes

## Pages

### Dashboard (`/`)
- System overview with key metrics
- Recent employees, exams, and announcements
- Quick access to important data

### Employees (`/employees`)
- View all employees in a table format
- Add, edit, and delete employees
- Search functionality
- Employee details with department information

### Exams (`/exams`)
- Manage exam definitions
- Create and edit exams with sections
- Publish/unpublish exams
- Exam type categorization (Quiz, Midterm, Final, Mock Exam)

### Announcements (`/announcements`)
- Create and manage system announcements
- Priority levels (Low, Normal, Medium, High, Critical)
- Active/inactive status
- Target audience support

### Books (`/books`)
- Simple book catalog management
- Add and remove books
- Search functionality

### Cars (`/cars`)
- View vehicle fleet information
- Special Santa car feature (feature flag protected)
- Search by plate or model

### Analytics (`/analytics`)
- Employee gender distribution
- Exam type distribution
- Announcement priority distribution
- Recent activity overview

### Settings (`/settings`)
- General settings (language, timezone, theme)
- Notification preferences
- Security settings
- Profile management

## Technology Stack

- **Frontend**: Next.js 14, React 18
- **Styling**: Tailwind CSS
- **Icons**: Heroicons
- **HTTP Client**: Axios
- **Notifications**: React Hot Toast
- **Date Handling**: date-fns
- **Utilities**: clsx

## Getting Started

### Prerequisites

- Node.js 18+ 
- npm or yarn
- Your .NET backend running on `http://localhost:5000` (or set `NEXT_PUBLIC_API_URL`)

### Installation

1. Install dependencies:
```bash
npm install
```

2. Set up environment variables (optional):
```bash
# Create .env.local file
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

3. Run the development server:
```bash
npm run dev
```

4. Open [http://localhost:3000](http://localhost:3000) in your browser.

### Building for Production

```bash
npm run build
npm start
```

## API Integration

The admin panel is designed to work with your .NET backend API. It includes endpoints for:

- **Authentication**: Login, register, profile management
- **Employees**: CRUD operations
- **Exams**: Exam definitions and results management
- **Announcements**: System announcements and notifications
- **Books**: Simple book management
- **Cars**: Vehicle fleet information

### API Configuration

The API base URL can be configured via the `NEXT_PUBLIC_API_URL` environment variable. Default is `http://localhost:5000/api`.

### Authentication

The admin panel supports JWT token authentication. Tokens are stored in localStorage and automatically included in API requests.

## Customization

### Styling

The admin panel uses Tailwind CSS with custom components defined in `styles/globals.css`. You can customize:

- Color scheme in `tailwind.config.js`
- Component styles in `styles/globals.css`
- Layout components in `components/Layout.js`

### Adding New Pages

1. Create a new page in `pages/`
2. Add navigation item in `components/Layout.js`
3. Add API endpoints in `services/api.js`

### API Endpoints

All API calls are centralized in `services/api.js`. Add new endpoints by extending the existing API objects.

## Deployment

### Docker

The admin panel includes a Dockerfile for containerized deployment:

```bash
docker build -t admin-panel .
docker run -p 3000:3000 admin-panel
```

### Vercel

Deploy to Vercel with zero configuration:

```bash
npm install -g vercel
vercel
```

### Other Platforms

The admin panel can be deployed to any platform that supports Next.js applications.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For support and questions, please refer to the project documentation or create an issue in the repository. 