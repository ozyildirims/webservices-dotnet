import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import {
  ChartBarIcon,
  UserGroupIcon,
  AcademicCapIcon,
  BellIcon,
  DocumentTextIcon,
  TruckIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
} from '@heroicons/react/24/outline';
import { employeesAPI, examsAPI, announcementsAPI, booksAPI, carsAPI } from '../services/api';
import toast from 'react-hot-toast';

export default function Analytics() {
  const [data, setData] = useState({
    employees: [],
    exams: [],
    announcements: [],
    books: [],
    cars: [],
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchData();
  }, []);

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
      console.error('Error fetching analytics data:', error);
      toast.error('Failed to load analytics data');
    } finally {
      setLoading(false);
    }
  };

  const stats = [
    {
      name: 'Total Employees',
      value: data.employees.length,
      icon: UserGroupIcon,
      color: 'bg-blue-500',
      change: '+12%',
      changeType: 'increase',
    },
    {
      name: 'Active Exams',
      value: data.exams.filter(exam => exam.IsPublished).length,
      icon: AcademicCapIcon,
      color: 'bg-green-500',
      change: '+5%',
      changeType: 'increase',
    },
    {
      name: 'Announcements',
      value: data.announcements.length,
      icon: BellIcon,
      color: 'bg-yellow-500',
      change: '+8%',
      changeType: 'increase',
    },
    {
      name: 'Books',
      value: data.books.length,
      icon: DocumentTextIcon,
      color: 'bg-purple-500',
      change: '+3%',
      changeType: 'increase',
    },
    {
      name: 'Cars',
      value: data.cars.length,
      icon: TruckIcon,
      color: 'bg-red-500',
      change: '+2%',
      changeType: 'increase',
    },
  ];

  const getGenderDistribution = () => {
    const distribution = data.employees.reduce((acc, employee) => {
      acc[employee.Gender] = (acc[employee.Gender] || 0) + 1;
      return acc;
    }, {});
    return distribution;
  };

  const getExamTypeDistribution = () => {
    const distribution = data.exams.reduce((acc, exam) => {
      const type = exam.Type === 0 ? 'Quiz' : 
                   exam.Type === 1 ? 'Midterm' : 
                   exam.Type === 2 ? 'Final' : 'Mock Exam';
      acc[type] = (acc[type] || 0) + 1;
      return acc;
    }, {});
    return distribution;
  };

  const getAnnouncementPriorityDistribution = () => {
    const distribution = data.announcements.reduce((acc, announcement) => {
      acc[announcement.Priority] = (acc[announcement.Priority] || 0) + 1;
      return acc;
    }, {});
    return distribution;
  };

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
          <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
          <p className="mt-1 text-sm text-gray-500">
            Overview of your system's performance and statistics
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
                <div className="ml-4 flex-1">
                  <p className="text-sm font-medium text-gray-500">{stat.name}</p>
                  <p className="text-2xl font-semibold text-gray-900">{stat.value}</p>
                  <div className="flex items-center mt-1">
                    {stat.changeType === 'increase' ? (
                      <ArrowTrendingUpIcon className="h-4 w-4 text-green-500" />
                    ) : (
                      <ArrowTrendingDownIcon className="h-4 w-4 text-red-500" />
                    )}
                    <span className={`text-xs font-medium ml-1 ${
                      stat.changeType === 'increase' ? 'text-green-600' : 'text-red-600'
                    }`}>
                      {stat.change}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Charts Grid */}
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          {/* Gender Distribution */}
          <div className="card">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Employee Gender Distribution</h3>
            <div className="space-y-3">
              {Object.entries(getGenderDistribution()).map(([gender, count]) => (
                <div key={gender} className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className={`w-3 h-3 rounded-full mr-3 ${
                      gender === 'M' ? 'bg-blue-500' : 'bg-pink-500'
                    }`}></div>
                    <span className="text-sm font-medium text-gray-900">
                      {gender === 'M' ? 'Male' : 'Female'}
                    </span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <div className="w-32 bg-gray-200 rounded-full h-2">
                      <div
                        className={`h-2 rounded-full ${
                          gender === 'M' ? 'bg-blue-500' : 'bg-pink-500'
                        }`}
                        style={{
                          width: `${(count / data.employees.length) * 100}%`
                        }}
                      ></div>
                    </div>
                    <span className="text-sm text-gray-500 w-8 text-right">{count}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Exam Type Distribution */}
          <div className="card">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Exam Type Distribution</h3>
            <div className="space-y-3">
              {Object.entries(getExamTypeDistribution()).map(([type, count]) => (
                <div key={type} className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-900">{type}</span>
                  <div className="flex items-center space-x-2">
                    <div className="w-32 bg-gray-200 rounded-full h-2">
                      <div
                        className="h-2 rounded-full bg-green-500"
                        style={{
                          width: `${(count / data.exams.length) * 100}%`
                        }}
                      ></div>
                    </div>
                    <span className="text-sm text-gray-500 w-8 text-right">{count}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Announcement Priority Distribution */}
        <div className="card">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Announcement Priority Distribution</h3>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5">
            {Object.entries(getAnnouncementPriorityDistribution()).map(([priority, count]) => {
              const priorityLabels = {
                1: 'Low',
                2: 'Normal',
                3: 'Medium',
                4: 'High',
                5: 'Critical',
              };
              const priorityColors = {
                1: 'bg-gray-100 text-gray-800',
                2: 'bg-blue-100 text-blue-800',
                3: 'bg-yellow-100 text-yellow-800',
                4: 'bg-orange-100 text-orange-800',
                5: 'bg-red-100 text-red-800',
              };
              
              return (
                <div key={priority} className="text-center">
                  <div className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium mb-2 ${
                    priorityColors[priority] || priorityColors[1]
                  }`}>
                    {priorityLabels[priority] || 'Unknown'}
                  </div>
                  <p className="text-2xl font-bold text-gray-900">{count}</p>
                  <p className="text-sm text-gray-500">
                    {((count / data.announcements.length) * 100).toFixed(1)}%
                  </p>
                </div>
              );
            })}
          </div>
        </div>

        {/* Recent Activity */}
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          {/* Recent Employees */}
          <div className="card">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Recent Employees</h3>
            <div className="space-y-3">
              {data.employees.slice(0, 5).map((employee) => (
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
              {data.exams.slice(0, 5).map((exam) => (
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
                      {exam.Type === 0 ? 'Quiz' : 
                       exam.Type === 1 ? 'Midterm' : 
                       exam.Type === 2 ? 'Final' : 'Mock Exam'} • {exam.TotalPoints} points
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
      </div>
    </Layout>
  );
} 