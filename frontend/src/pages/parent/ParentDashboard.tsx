import { useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import ChildrenList from '../../components/parent/ChildrenList.tsx';
import LiveMap from '../../components/parent/LiveMap.tsx';
import { Student } from '../../types';

const ParentDashboard = () => {
  const { user, logout } = useAuth();
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);

  return (
    <div style={styles.container}>
      {/* header */}
      <div style={styles.header}>
        <div>
          <p style={styles.greeting}>Hello, {user?.fullName}</p>
          <p style={styles.role}>Parent</p>
        </div>
        <button style={styles.logoutBtn} onClick={logout}>Logout</button>
      </div>

      <div style={styles.content}>
        <ChildrenList
          onSelectStudent={setSelectedStudent}
          selectedStudent={selectedStudent}
        />
        {selectedStudent && (
          <LiveMap student={selectedStudent} />
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
  content: {
    padding: '1rem',
    maxWidth: 600,
    margin: '0 auto',
  },
};

export default ParentDashboard;