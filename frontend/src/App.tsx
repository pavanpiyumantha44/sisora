import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './hooks/useAuth';
import ProtectedRoute from './components/common/ProtectedRoute';

// auth pages
import LoginPage from './pages/auth/LoginPage.tsx';
import RegisterPage from './pages/auth/RegisterPage.tsx';

// driver pages
import DriverDashboard from './pages/driver/DriverDashboard.tsx';

// parent pages
import ParentDashboard from './pages/parent/ParentDashboard.tsx';

const App = () => {
  const { user, isAuthenticated } = useAuth();

  const getDefaultRoute = () => {
    if (!isAuthenticated) return '/login';
    if (user?.role === 'Driver') return '/driver';
    if (user?.role === 'Parent') return '/parent';
    return '/login';
  };

  return (
    <Routes>
      {/* public */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      {/* driver */}
      <Route path="/driver/*" element={
        <ProtectedRoute allowedRoles={['Driver']}>
          <DriverDashboard />
        </ProtectedRoute>
      } />

      {/* parent */}
      <Route path="/parent/*" element={
        <ProtectedRoute allowedRoles={['Parent']}>
          <ParentDashboard />
        </ProtectedRoute>
      } />

      {/* fallback */}
      <Route path="/" element={<Navigate to={getDefaultRoute()} replace />} />
      <Route path="/unauthorized" element={
        <div style={{ padding: '2rem', textAlign: 'center' }}>
          <h2>Unauthorized</h2>
          <p>You don't have permission to view this page.</p>
        </div>
      } />
      <Route path="*" element={<Navigate to={getDefaultRoute()} replace />} />
    </Routes>
  );
};

export default App;