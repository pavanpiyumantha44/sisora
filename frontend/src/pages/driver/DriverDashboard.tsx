import { useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import RouteList from '../../components/driver/RouteList.tsx';
import StudentList from '../../components/driver/StudentList.tsx';
import ActiveTrip from '../../components/driver/ActiveTrip.tsx';

const DriverDashboard = () => {
  const { user, logout } = useAuth();
  const [selectedRouteId, setSelectedRouteId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'routes' | 'trip'>('routes');

  return (
    <div style={styles.container}>
      {/* header */}
      <div style={styles.header}>
        <div>
          <p style={styles.greeting}>Hello, {user?.fullName}</p>
          <p style={styles.role}>Driver</p>
        </div>
        <button style={styles.logoutBtn} onClick={logout}>Logout</button>
      </div>

      {/* tabs */}
      <div style={styles.tabs}>
        <button
          style={{ ...styles.tab, ...(activeTab === 'routes' ? styles.tabActive : {}) }}
          onClick={() => setActiveTab('routes')}
        >
          My Routes
        </button>
        <button
          style={{ ...styles.tab, ...(activeTab === 'trip' ? styles.tabActive : {}) }}
          onClick={() => setActiveTab('trip')}
        >
          Active Trip
        </button>
      </div>

      {/* content */}
      <div style={styles.content}>
        {activeTab === 'routes' && (
          <>
            <RouteList
              onSelectRoute={(id) => setSelectedRouteId(id)}
              selectedRouteId={selectedRouteId}
            />
            {selectedRouteId && (
              <StudentList routeId={selectedRouteId} />
            )}
          </>
        )}
        {activeTab === 'trip' && (
          <ActiveTrip />
        )}
      </div>
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: {
    minHeight: '100vh',
    background: '#f5f5f5',
    paddingBottom: '2rem',
  },
  header: {
    background: '#1a73e8',
    color: '#fff',
    padding: '1rem 1.5rem',
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  greeting: { margin: 0, fontWeight: 600, fontSize: 16 },
  role: { margin: 0, fontSize: 12, opacity: 0.8 },
  logoutBtn: {
    background: 'rgba(255,255,255,0.2)',
    border: 'none',
    color: '#fff',
    padding: '6px 14px',
    borderRadius: 8,
    cursor: 'pointer',
    fontSize: 13,
  },
  tabs: {
    display: 'flex',
    background: '#fff',
    borderBottom: '1px solid #eee',
  },
  tab: {
    flex: 1,
    padding: '12px',
    border: 'none',
    background: 'none',
    cursor: 'pointer',
    fontSize: 14,
    color: '#666',
  },
  tabActive: {
    color: '#1a73e8',
    fontWeight: 600,
    borderBottom: '2px solid #1a73e8',
  },
  content: {
    padding: '1rem',
    maxWidth: 600,
    margin: '0 auto',
  },
};

export default DriverDashboard;