import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import {
  PlusIcon,
  PencilIcon,
  TrashIcon,
  EyeIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  XCircleIcon,
} from '@heroicons/react/24/outline';
import { examsAPI } from '../services/api';
import toast from 'react-hot-toast';

const examTypes = [
  { value: 0, label: 'Quiz' },
  { value: 1, label: 'Midterm' },
  { value: 2, label: 'Final' },
  { value: 3, label: 'Mock Exam' },
];

export default function Exams() {
  const [exams, setExams] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingExam, setEditingExam] = useState(null);
  const [formData, setFormData] = useState({
    Title: '',
    Description: '',
    Duration: { Ticks: 0, Days: 0, Hours: 0, Minutes: 0 },
    StartDate: '',
    EndDate: '',
    Type: 0,
    Sections: [],
  });

  useEffect(() => {
    fetchExams();
  }, []);

  const fetchExams = async () => {
    try {
      const response = await examsAPI.getAll();
      setExams(response.data || []);
    } catch (error) {
      console.error('Error fetching exams:', error);
      toast.error('Failed to load exams');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const examData = {
        ...formData,
        Duration: new Date(0, 0, formData.Duration.Days, formData.Duration.Hours, formData.Duration.Minutes).getTime(),
      };

      if (editingExam) {
        await examsAPI.update(editingExam.Id, examData);
        toast.success('Exam updated successfully');
      } else {
        await examsAPI.create(examData);
        toast.success('Exam created successfully');
      }
      setShowModal(false);
      setEditingExam(null);
      resetForm();
      fetchExams();
    } catch (error) {
      console.error('Error saving exam:', error);
      toast.error('Failed to save exam');
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Are you sure you want to delete this exam?')) return;
    
    try {
      await examsAPI.delete(id);
      toast.success('Exam deleted successfully');
      fetchExams();
    } catch (error) {
      console.error('Error deleting exam:', error);
      toast.error('Failed to delete exam');
    }
  };

  const handlePublish = async (id) => {
    try {
      await examsAPI.publish(id);
      toast.success('Exam published successfully');
      fetchExams();
    } catch (error) {
      console.error('Error publishing exam:', error);
      toast.error('Failed to publish exam');
    }
  };

  const handleUnpublish = async (id) => {
    try {
      await examsAPI.unpublish(id);
      toast.success('Exam unpublished successfully');
      fetchExams();
    } catch (error) {
      console.error('Error unpublishing exam:', error);
      toast.error('Failed to unpublish exam');
    }
  };

  const handleEdit = (exam) => {
    setEditingExam(exam);
    setFormData({
      Title: exam.Title || '',
      Description: exam.Description || '',
      Duration: {
        Ticks: exam.Duration?.Ticks || 0,
        Days: exam.Duration?.Days || 0,
        Hours: exam.Duration?.Hours || 0,
        Minutes: exam.Duration?.Minutes || 0,
      },
      StartDate: exam.StartDate ? new Date(exam.StartDate).toISOString().split('T')[0] : '',
      EndDate: exam.EndDate ? new Date(exam.EndDate).toISOString().split('T')[0] : '',
      Type: exam.Type || 0,
      Sections: exam.Sections || [],
    });
    setShowModal(true);
  };

  const resetForm = () => {
    setFormData({
      Title: '',
      Description: '',
      Duration: { Ticks: 0, Days: 0, Hours: 0, Minutes: 0 },
      StartDate: '',
      EndDate: '',
      Type: 0,
      Sections: [],
    });
  };

  const filteredExams = exams.filter(exam =>
    exam.Title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    exam.Description?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getExamTypeLabel = (type) => {
    return examTypes.find(t => t.value === type)?.label || 'Unknown';
  };

  const formatDuration = (duration) => {
    if (!duration) return '0h 0m';
    const hours = Math.floor(duration / 36000000000);
    const minutes = Math.floor((duration % 36000000000) / 600000000);
    return `${hours}h ${minutes}m`;
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
            <h1 className="text-2xl font-bold text-gray-900">Exams</h1>
            <p className="mt-1 text-sm text-gray-500">
              Manage exam definitions and configurations
            </p>
          </div>
          <button
            onClick={() => {
              setEditingExam(null);
              resetForm();
              setShowModal(true);
            }}
            className="btn-primary flex items-center"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Create Exam
          </button>
        </div>

        {/* Search */}
        <div className="relative">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search exams..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input-field pl-10"
          />
        </div>

        {/* Exams Grid */}
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {filteredExams.map((exam) => (
            <div key={exam.Id} className="card hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div className="flex-1">
                  <h3 className="text-lg font-medium text-gray-900 truncate">{exam.Title}</h3>
                  <p className="text-sm text-gray-500 mt-1">{exam.Description}</p>
                </div>
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                  exam.IsPublished 
                    ? 'bg-green-100 text-green-800' 
                    : 'bg-yellow-100 text-yellow-800'
                }`}>
                  {exam.IsPublished ? 'Published' : 'Draft'}
                </span>
              </div>
              
              <div className="space-y-2 text-sm text-gray-600">
                <div className="flex justify-between">
                  <span>Type:</span>
                  <span className="font-medium">{getExamTypeLabel(exam.Type)}</span>
                </div>
                <div className="flex justify-between">
                  <span>Duration:</span>
                  <span className="font-medium">{formatDuration(exam.Duration)}</span>
                </div>
                <div className="flex justify-between">
                  <span>Points:</span>
                  <span className="font-medium">{exam.TotalPoints}</span>
                </div>
                <div className="flex justify-between">
                  <span>Start Date:</span>
                  <span className="font-medium">{new Date(exam.StartDate).toLocaleDateString()}</span>
                </div>
                <div className="flex justify-between">
                  <span>End Date:</span>
                  <span className="font-medium">{new Date(exam.EndDate).toLocaleDateString()}</span>
                </div>
              </div>

              <div className="flex justify-between items-center mt-4 pt-4 border-t border-gray-200">
                <div className="flex space-x-2">
                  <button
                    onClick={() => handleEdit(exam)}
                    className="text-blue-600 hover:text-blue-900 p-1"
                    title="Edit"
                  >
                    <PencilIcon className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => handleDelete(exam.Id)}
                    className="text-red-600 hover:text-red-900 p-1"
                    title="Delete"
                  >
                    <TrashIcon className="h-4 w-4" />
                  </button>
                </div>
                <div className="flex space-x-2">
                  {exam.IsPublished ? (
                    <button
                      onClick={() => handleUnpublish(exam.Id)}
                      className="btn-secondary text-xs"
                    >
                      Unpublish
                    </button>
                  ) : (
                    <button
                      onClick={() => handlePublish(exam.Id)}
                      className="btn-primary text-xs"
                    >
                      Publish
                    </button>
                  )}
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
                  {editingExam ? 'Edit Exam' : 'Create Exam'}
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
                    <label className="block text-sm font-medium text-gray-700">Description</label>
                    <textarea
                      value={formData.Description}
                      onChange={(e) => setFormData({ ...formData, Description: e.target.value })}
                      className="input-field"
                      rows={3}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Type</label>
                    <select
                      required
                      value={formData.Type}
                      onChange={(e) => setFormData({ ...formData, Type: parseInt(e.target.value) })}
                      className="input-field"
                    >
                      {examTypes.map(type => (
                        <option key={type.value} value={type.value}>{type.label}</option>
                      ))}
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Duration (Hours)</label>
                      <input
                        type="number"
                        min="0"
                        value={formData.Duration.Hours}
                        onChange={(e) => setFormData({
                          ...formData,
                          Duration: { ...formData.Duration, Hours: parseInt(e.target.value) || 0 }
                        })}
                        className="input-field"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Duration (Minutes)</label>
                      <input
                        type="number"
                        min="0"
                        value={formData.Duration.Minutes}
                        onChange={(e) => setFormData({
                          ...formData,
                          Duration: { ...formData.Duration, Minutes: parseInt(e.target.value) || 0 }
                        })}
                        className="input-field"
                      />
                    </div>
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
                    <label className="block text-sm font-medium text-gray-700">End Date</label>
                    <input
                      type="datetime-local"
                      required
                      value={formData.EndDate}
                      onChange={(e) => setFormData({ ...formData, EndDate: e.target.value })}
                      className="input-field"
                    />
                  </div>
                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      type="button"
                      onClick={() => {
                        setShowModal(false);
                        setEditingExam(null);
                        resetForm();
                      }}
                      className="btn-secondary"
                    >
                      Cancel
                    </button>
                    <button type="submit" className="btn-primary">
                      {editingExam ? 'Update' : 'Create'}
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