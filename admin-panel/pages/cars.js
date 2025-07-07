import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import {
  MagnifyingGlassIcon,
  TruckIcon,
  SparklesIcon,
} from '@heroicons/react/24/outline';
import { carsAPI } from '../services/api';
import toast from 'react-hot-toast';

export default function Cars() {
  const [cars, setCars] = useState([]);
  const [santaCar, setSantaCar] = useState(null);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchCars();
  }, []);

  const fetchCars = async () => {
    try {
      const [carsResponse, santaResponse] = await Promise.all([
        carsAPI.getAll(),
        carsAPI.getSanta().catch(() => null), // Ignore errors for Santa car
      ]);
      
      setCars(carsResponse.data || []);
      if (santaResponse?.data) {
        setSantaCar(santaResponse.data);
      }
    } catch (error) {
      console.error('Error fetching cars:', error);
      toast.error('Failed to load cars');
    } finally {
      setLoading(false);
    }
  };

  const filteredCars = cars.filter(car =>
    car.Plate?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    car.Model?.toLowerCase().includes(searchTerm.toLowerCase())
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
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Cars</h1>
          <p className="mt-1 text-sm text-gray-500">
            View your vehicle fleet information
          </p>
        </div>

        {/* Santa's Special Car */}
        {santaCar && (
          <div className="card bg-gradient-to-r from-red-50 to-green-50 border-red-200">
            <div className="flex items-center space-x-4">
              <div className="flex-shrink-0">
                <div className="h-12 w-12 rounded-full bg-red-100 flex items-center justify-center">
                  <SparklesIcon className="h-6 w-6 text-red-600" />
                </div>
              </div>
              <div className="flex-1">
                <h3 className="text-lg font-medium text-gray-900">Santa's Special Car</h3>
                <p className="text-sm text-gray-600">
                  Plate: {santaCar.Plate} • Model: {santaCar.Model}
                </p>
              </div>
              <div className="flex-shrink-0">
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                  Special
                </span>
              </div>
            </div>
          </div>
        )}

        {/* Search */}
        <div className="relative">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search cars by plate or model..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="input-field pl-10"
          />
        </div>

        {/* Cars Grid */}
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {filteredCars.map((car) => (
            <div key={car.Id} className="card hover:shadow-md transition-shadow">
              <div className="flex items-center justify-center h-32 bg-gray-100 rounded-lg mb-4">
                <TruckIcon className="h-12 w-12 text-gray-400" />
              </div>
              <div className="text-center">
                <h3 className="text-lg font-medium text-gray-900">{car.Model}</h3>
                <p className="text-sm text-gray-500 mt-1">Plate: {car.Plate}</p>
                <p className="text-xs text-gray-400 mt-1">ID: {car.Id}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Empty State */}
        {filteredCars.length === 0 && !loading && (
          <div className="text-center py-12">
            <TruckIcon className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-2 text-sm font-medium text-gray-900">No cars found</h3>
            <p className="mt-1 text-sm text-gray-500">
              {searchTerm ? 'Try adjusting your search terms.' : 'No cars are currently registered.'}
            </p>
          </div>
        )}
      </div>
    </Layout>
  );
} 