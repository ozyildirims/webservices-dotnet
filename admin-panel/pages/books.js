import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import {
  PlusIcon,
  PencilIcon,
  TrashIcon,
  MagnifyingGlassIcon,
  BookOpenIcon,
} from '@heroicons/react/24/outline';
import { booksAPI } from '../services/api';
import toast from 'react-hot-toast';

export default function Books() {
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingBook, setEditingBook] = useState(null);
  const [formData, setFormData] = useState({
    Title: '',
  });

  useEffect(() => {
    fetchBooks();
  }, []);

  const fetchBooks = async () => {
    try {
      const response = await booksAPI.getAll();
      setBooks(response.data || []);
    } catch (error) {
      console.error('Error fetching books:', error);
      toast.error('Failed to load books');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (editingBook) {
        await booksAPI.update(editingBook.Id, formData);
        toast.success('Book updated successfully');
      } else {
        await booksAPI.create(formData);
        toast.success('Book created successfully');
      }
      setShowModal(false);
      setEditingBook(null);
      resetForm();
      fetchBooks();
    } catch (error) {
      console.error('Error saving book:', error);
      toast.error('Failed to save book');
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Are you sure you want to delete this book?')) return;
    
    try {
      await booksAPI.delete(id);
      toast.success('Book deleted successfully');
      fetchBooks();
    } catch (error) {
      console.error('Error deleting book:', error);
      toast.error('Failed to delete book');
    }
  };

  const handleEdit = (book) => {
    setEditingBook(book);
    setFormData({
      Title: book.Title || '',
    });
    setShowModal(true);
  };

  const resetForm = () => {
    setFormData({
      Title: '',
    });
  };

  const filteredBooks = books.filter(book =>
    book.Title?.toLowerCase().includes(searchTerm.toLowerCase())
  );

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
            <h1 className="text-2xl font-bold text-gray-900">Books</h1>
            <p className="mt-1 text-sm text-gray-500">
              Manage your book collection
            </p>
          </div>
          <button
            onClick={() => {
              setEditingBook(null);
              resetForm();
              setShowModal(true);
            }}
            className="btn-primary flex items-center"
          >
            <PlusIcon className="h-4 w-4 mr-2" />
            Add Book
          </button>
        </div>

        {/* Search */}
        <div className="relative">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search books..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input-field pl-10"
          />
        </div>

        {/* Books Grid */}
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {filteredBooks.map((book) => (
            <div key={book.Id} className="card hover:shadow-md transition-shadow">
              <div className="flex items-center justify-center h-32 bg-gray-100 rounded-lg mb-4">
                <BookOpenIcon className="h-12 w-12 text-gray-400" />
              </div>
              <div className="text-center">
                <h3 className="text-lg font-medium text-gray-900 truncate">{book.Title}</h3>
                <p className="text-sm text-gray-500 mt-1">ID: {book.Id}</p>
              </div>
              <div className="flex justify-center space-x-2 mt-4 pt-4 border-t border-gray-200">
                <button
                  onClick={() => handleEdit(book)}
                  className="text-blue-600 hover:text-blue-900 p-2"
                  title="Edit"
                >
                  <PencilIcon className="h-4 w-4" />
                </button>
                <button
                  onClick={() => handleDelete(book.Id)}
                  className="text-red-600 hover:text-red-900 p-2"
                  title="Delete"
                >
                  <TrashIcon className="h-4 w-4" />
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Modal */}
        {showModal && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
              <div className="mt-3">
                <h3 className="text-lg font-medium text-gray-900 mb-4">
                  {editingBook ? 'Edit Book' : 'Add Book'}
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
                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      type="button"
                      onClick={() => {
                        setShowModal(false);
                        setEditingBook(null);
                        resetForm();
                      }}
                      className="btn-secondary"
                    >
                      Cancel
                    </button>
                    <button type="submit" className="btn-primary">
                      {editingBook ? 'Update' : 'Create'}
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