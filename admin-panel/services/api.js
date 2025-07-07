import axios from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://netcore-boilerplate:8080/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
    'Authorization': 'ApiKey ABC-xyz',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor to handle errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth endpoints
export const authAPI = {
  login: (credentials) => api.post('/auth/login', credentials),
  register: (userData) => api.post('/auth/register', userData),
  getProfile: () => api.get('/auth/profile'),
  updateProfile: (userData) => api.put('/auth/profile', userData),
  changePassword: (passwordData) => api.post('/auth/change-password', passwordData),
  refreshToken: (refreshData) => api.post('/auth/refresh', refreshData),
};

// Employees endpoints
export const employeesAPI = {
  getAll: () => api.get('/employees'),
  getById: (id) => api.get(`/employees/${id}`),
  getDetails: (id) => api.get(`/employees/${id}/details`),
  create: (employeeData) => api.post('/employees', employeeData),
  update: (id, employeeData) => api.put(`/employees/${id}`, employeeData),
  delete: (id) => api.delete(`/employees/${id}`),
  getOldest: () => api.get('/employees/oldest'),
};

// Cars endpoints
export const carsAPI = {
  getAll: () => api.get('/cars'),
  getSanta: () => api.get('/cars/santa'),
};

// Books endpoints
export const booksAPI = {
  getAll: () => api.get('/books'),
  getById: (id) => api.get(`/books/${id}`),
  create: (bookData) => api.post('/books', bookData),
  delete: (id) => api.delete(`/books/${id}`),
};

// Exams endpoints
export const examsAPI = {
  getAll: () => api.get('/exams'),
  getActive: () => api.get('/exams/active'),
  getByType: (type) => api.get(`/exams/type/${type}`),
  getById: (id) => api.get(`/exams/${id}`),
  create: (examData) => api.post('/exams', examData),
  update: (id, examData) => api.put(`/exams/${id}`, examData),
  delete: (id) => api.delete(`/exams/${id}`),
  publish: (id) => api.post(`/exams/${id}/publish`),
  unpublish: (id) => api.post(`/exams/${id}/unpublish`),
};

// Exam Results endpoints
export const examResultsAPI = {
  getById: (id) => api.get(`/exam-results/${id}`),
  getByStudent: (studentId) => api.get(`/exam-results/student/${studentId}`),
  getByExam: (examId) => api.get(`/exam-results/exam/${examId}`),
  getByStudentAndExam: (studentId, examId) => api.get(`/exam-results/student/${studentId}/exam/${examId}`),
  start: (examData) => api.post('/exam-results/start', examData),
  update: (id, resultData) => api.put(`/exam-results/${id}`, resultData),
  export: (examId) => api.get(`/exam-results/exam/${examId}/export`),
  import: (examId, file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post(`/exam-results/exam/${examId}/import`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
};

// Announcements endpoints
export const announcementsAPI = {
  getAll: (params) => api.get('/announcements', { params }),
  getById: (id) => api.get(`/announcements/${id}`),
  create: (announcementData) => api.post('/announcements', announcementData),
  update: (id, announcementData) => api.put(`/announcements/${id}`, announcementData),
  delete: (id) => api.delete(`/announcements/${id}`),
  getNotifications: (params) => api.get('/announcements/notifications', { params }),
  markAsRead: (id) => api.post(`/announcements/notifications/${id}/read`),
  getSettings: () => api.get('/announcements/settings'),
  updateSettings: (settingsData) => api.put('/announcements/settings', settingsData),
  registerDevice: (deviceData) => api.post('/announcements/devices', deviceData),
};

// Pings endpoints
export const pingsAPI = {
  getWebsiteStatus: () => api.get('/pings/website'),
  getRandomStatus: () => api.get('/pings/random'),
};

export default api; 