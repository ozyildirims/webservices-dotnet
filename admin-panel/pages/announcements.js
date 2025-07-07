import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import {
  PlusIcon,
  PencilIcon,
  TrashIcon,
  EyeIcon,
  MagnifyingGlassIcon,
  BellIcon,
  ExclamationTriangleIcon,
} from '@heroicons/react/24/outline';
import { announcementsAPI } from '../services/api';
import toast from 'react-hot-toast';

const priorityColors = {
  1: 'bg-gray-100 text-gray-800',
  2: 'bg-blue-100 text-blue-800',
  3: 'bg-yellow-100 text-yellow-800',
  4: 'bg-orange-100 text-orange-800',
  5: 'bg-red-100 text-red-800',
};

export default function Announcements() {
  const [announcements, setAnnouncements] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingAnnouncement, setEditingAnnouncement] = useState(null);
  const [formData, setFormData] = useState({
    Title: '',
    Content: '',
    StartDate: '',
    EndDate: '',
    Priority: 1,
    IsActive: true,
    Targets: [],
  });

  useEffect(() => {
    fetchAnnouncements();
  }, []);

  const fetchAnnouncements = async () => {
    try {
      const response = await announcementsAPI.getAll();
      setAnnouncements(response.data || []);
    } catch (error) {
      console.error('Error fetching announcements:', error);
      toast.error('Failed to load announcements');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (editingAnnouncement) {
        await announcementsAPI.update(editingAnnouncement.Id, formData);
        toast.success('Announcement updated successfully');
      } else {
        await announcementsAPI.create(formData);
        toast.success('Announcement created successfully');
      }
      setShowModal(false);
      setEditingAnnouncement(null);
      resetForm();
      fetchAnnouncements();
    } catch (error) {
      console.error('Error saving announcement:', error);
      toast.error('Failed to save announcement');
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Are you sure you want to delete this announcement?')) return;
    
    try {
      await announcementsAPI.delete(id);
      toast.success('Announcement deleted successfully');
      fetchAnnouncements();
    } catch (error) {
      console.error('Error deleting announcement:', error);
      toast.error('Failed to delete announcement');
    }
  };

  const handleEdit = (announcement) => {
    setEditingAnnouncement(announcement);
    setFormData({
      Title: announcement.Title || '',
      Content: announcement.Content || '',
      StartDate: announcement.StartDate ? new Date(announcement.StartDate).toISOString().split('T')[0] : '',
      EndDate: announcement.EndDate ? new Date(announcement.EndDate).toISOString().split('T')[0] : '',
      Priority: announcement.Priority || 1,
      IsActive: announcement.IsActive !== undefined ? announcement.IsActive : true,
      Targets: announcement.Targets || [],
    });
    setShowModal(true);
  };

  const resetForm = () => {
    setFormData({
      Title: '',
      Content: '',
      StartDate: '',
      EndDate: '',
      Priority: 1,
      IsActive: true,
      Targets: [],
    });
  };

  const filteredAnnouncements = announcements.filter(announcement =>
    announcement.Title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    announcement.Content?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getPriorityLabel = (priority) => {
    const labels = {
      1: 'Low',
      2: 'Normal',
      3: 'Medium',
      4: 'High',
      5: 'Critical',
    };
    return labels[priority] || 'Unknown';
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
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Announcements</h1>
            <p className="mt-1 text-sm text-gray-500">
              Manage system announcements and notifications
            </p>
          </div>
          <button
            onClick={() => {
              setEditingAnnouncement(null);
              resetForm();
              setShowModal(true);
            }}
            className="btn-primary flex items-center"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Announcement
          </button>
        </div>

        {/* Search */}
        <div className="relative">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search announcements..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input-field pl-10"
          />
        </div>

        {/* Announcements Grid */}
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {filteredAnnouncements.map((announcement) => (
            <div key={announcement.Id} className="card hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div className="flex-1">
                  <h3 className="text-lg font-medium text-gray-900 truncate">{announcement.Title}</h3>
                  <p className="text-sm text-gray-500 mt-1 line-clamp-2">{announcement.Content}</p>
                </div>
                <div className="flex flex-col items-end space-y-2">
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    priorityColors[announcement.Priority] || priorityColors[1]
                  }`}>
                    {getPriorityLabel(announcement.Priority)}
                  </span>
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    announcement.IsActive 
                      ? 'bg-green-100 text-green-800' 
                      : 'bg-gray-100 text-gray-800'
                  }`}>
                    {announcement.IsActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
              </div>
              
              <div className="space-y-2 text-sm text-gray-600">
                <div className="flex justify-between">
                  <span>Start Date:</span>
                  <span className="font-medium">{new Date(announcement.StartDate).toLocaleDateString()}</span>
                </div>
                {announcement.EndDate && (
                  <div className="flex justify-between">
                    <span>End Date:</span>
                    <span className="font-medium">{new Date(announcement.EndDate).toLocaleDateString()}</span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span>Created:</span>
                  <span className="font-medium">{new Date(announcement.CreatedAt).toLocaleDateString()}</span>
                </div>
                {announcement.Targets && announcement.Targets.length > 0 && (
                  <div className="flex justify-between">
                    <span>Targets:</span>
                    <span className="font-medium">{announcement.Targets.length}</span>
                  </div>
                )}
              </div>

              <div className="flex justify-between items-center mt-4 pt-4 border-t border-gray-200">
                <div className="flex space-x-2">
                  <button
                    onClick={() => handleEdit(announcement)}
                    className="text-blue-600 hover:text-blue-900 p-1"
                    title="Edit"
                  >
                    <PencilIcon className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => handleDelete(announcement.Id)}
                    className="text-red-600 hover:text-red-900 p-1"
                    title="Delete"
                  >
                    <TrashIcon className="h-4 w-4" />
                  </button>
                </div>
                <div className="flex items-center space-x-1 text-xs text-gray-500">
                  <BellIcon className="h-3 w-3" />
                  <span>ID: {announcement.Id.slice(0, 8)}...</span>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Modal */}
        {showModal && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-full max-w-md shadow-lg rounded-md bg-white">
              <div className="mt-3">
                <h3 className="text-lg font-medium text-gray-900 mb-4">
                  {editingAnnouncement ? 'Edit Announcement' : 'Create Announcement'}
                </h3>
                <form onSubmit={handleSubmit} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Title</label>
                    <input
                      type="text"
                      required
                      value={formData.Title}
                      onChange={(e) => setFormData({ ...formData, Title: e.target.value })}
                      className="input-field"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Content</label>
                    <textarea
                      required
                      value={formData.Content}
                      onChange={(e) => setFormData({ ...formData, Content: e.target.value })}
                      className="input-field"
                      rows={4}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Priority</label>
                    <select
                      required
                      value={formData.Priority}
                      onChange={(e) => setFormData({ ...formData, Priority: parseInt(e.target.value) })}
                      className="input-field"
                    >
                      <option value={1}>Low</option>
                      <option value={2}>Normal</option>
                      <option value={3}>Medium</option>
                      <option value={4}>High</option>
                      <option value={5}>Critical</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Start Date</label>
                    <input
                      type="datetime-local"
                      required
                      value={formData.StartDate}
                      onChange={(e) => setFormData({ ...formData, StartDate: e.target.value })}
                      className="input-field"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">End Date (Optional)</label>
                    <input
                      type="datetime-local"
                      value={formData.EndDate}
                      onChange={(e) => setFormData({ ...formData, EndDate: e.target.value })}
                      className="input-field"
                    />
                  </div>
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="isActive"
                      checked={formData.IsActive}
                      onChange={(e) => setFormData({ ...formData, IsActive: e.target.checked })}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                      Active
                    </label>
                  </div>
                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      type="button"
                      onClick={() => {
                        setShowModal(false);
                        setEditingAnnouncement(null);
                        resetForm();
                      }}
                      className="btn-secondary"
                    >
                      Cancel
                    </button>
                    <button type="submit" className="btn-primary">
                      {editingAnnouncement ? 'Update' : 'Create'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
} 