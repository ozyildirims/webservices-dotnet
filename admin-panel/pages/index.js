import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import {
  UserGroupIcon,
  AcademicCapIcon,
  BellIcon,
  DocumentTextIcon,
  TruckIcon,
  ChartBarIcon,
  ArrowUpIcon,
  ArrowDownIcon,
} from '@heroicons/react/24/outline';
import { employeesAPI, examsAPI, announcementsAPI, booksAPI, carsAPI } from '../services/api';
import toast from 'react-hot-toast';

const stats = [
  { name: 'Total Employees', icon: UserGroupIcon, color: 'bg-blue-500' },
  { name: 'Active Exams', icon: AcademicCapIcon, color: 'bg-green-500' },
  { name: 'Announcements', icon: BellIcon, color: 'bg-yellow-500' },
  { name: 'Books', icon: DocumentTextIcon, color: 'bg-purple-500' },
  { name: 'Cars', icon: TruckIcon, color: 'bg-red-500' },
];

export default function Dashboard() {
  const [data, setData] = useState({
    employees: [],
    exams: [],
    announcements: [],
    books: [],
    cars: [],
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [employeesRes, examsRes, announcementsRes, booksRes, carsRes] = await Promise.all([
          employeesAPI.getAll(),
          examsAPI.getAll(),
          announcementsAPI.getAll(),
          booksAPI.getAll(),
          carsAPI.getAll(),
        ]);

        setData({
          employees: employeesRes.data || [],
          exams: examsRes.data || [],
          announcements: announcementsRes.data || [],
          books: booksRes.data || [],
          cars: carsRes.data || [],
        });
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
        toast.error('Failed to load dashboard data');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const getStatValue = (key) => {
    return data[key]?.length || 0;
  };

  const recentEmployees = data.employees.slice(0, 5);
  const recentExams = data.exams.slice(0, 5);
  const recentAnnouncements = data.announcements.slice(0, 5);

  if (loading) {
    return (
      <Layout>
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-1 text-sm text-gray-500">
            Welcome to your admin panel. Here's an overview of your system.
          </p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
          {stats.map((stat) => (
            <div key={stat.name} className="card">
              <div className="flex items-center">
                <div className={`flex-shrink-0 p-3 rounded-lg ${stat.color}`}>
                  <stat.icon className="h-6 w-6 text-white" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-500">{stat.name}</p>
                  <p className="text-2xl font-semibold text-gray-900">
                    {getStatValue(stat.name.toLowerCase().replace(' ', ''))}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Recent Activity */}
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          {/* Recent Employees */}
          <div className="card">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Recent Employees</h3>
            <div className="space-y-3">
              {recentEmployees.map((employee) => (
                <div key={employee.Id} className="flex items-center space-x-3">
                  <div className="flex-shrink-0">
                    <div className="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center">
                      <span className="text-sm font-medium text-gray-600">
                        {employee.FirstName?.[0]}{employee.LastName?.[0]}
                      </span>
                    </div>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      {employee.FirstName} {employee.LastName}
                    </p>
                    <p className="text-sm text-gray-500">
                      {employee.Gender} • {new Date(employee.BirthDate).getFullYear()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Recent Exams */}
          <div className="card">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Recent Exams</h3>
            <div className="space-y-3">
              {recentExams.map((exam) => (
                <div key={exam.Id} className="flex items-center space-x-3">
                  <div className="flex-shrink-0">
                    <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
                      <AcademicCapIcon className="h-4 w-4 text-green-600" />
                    </div>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      {exam.Title}
                    </p>
                    <p className="text-sm text-gray-500">
                      {exam.Type} • {exam.TotalPoints} points
                    </p>
                  </div>
                  <div className="flex-shrink-0">
                    <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                      exam.IsPublished 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-yellow-100 text-yellow-800'
                    }`}>
                      {exam.IsPublished ? 'Published' : 'Draft'}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Recent Announcements */}
        <div className="card">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Recent Announcements</h3>
          <div className="space-y-4">
            {recentAnnouncements.map((announcement) => (
              <div key={announcement.Id} className="border-l-4 border-blue-500 pl-4">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <h4 className="text-sm font-medium text-gray-900">{announcement.Title}</h4>
                    <p className="text-sm text-gray-500 mt-1 line-clamp-2">
                      {announcement.Content}
                    </p>
                    <div className="flex items-center space-x-4 mt-2 text-xs text-gray-400">
                      <span>Priority: {announcement.Priority}</span>
                      <span>Active: {announcement.IsActive ? 'Yes' : 'No'}</span>
                      <span>{new Date(announcement.CreatedAt).toLocaleDateString()}</span>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </Layout>
  );
} 