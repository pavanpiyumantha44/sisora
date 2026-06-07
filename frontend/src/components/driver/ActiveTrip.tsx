import { useState, useEffect, useRef } from 'react';
import { getMyRoutes, getStudents as fetchStudents } from '../../api/driver';
import { startTrip, endTrip, updateLocation, recordTripEvent } from '../../api/trip';
import { ServiceRoute, Student, Trip } from '../../types';

const ActiveTrip = () => {
  const [routes, setRoutes] = useState<ServiceRoute[]>([]);
  const [selectedRouteId, setSelectedRouteId] = useState('');
  const [tripType, setTripType] = useState('Morning');
  const [activeTrip, setActiveTrip] = useState<Trip | null>(null);
  const [students, setStudents] = useState<Student[]>([]);
  const [processedStudents, setProcessedStudents] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(false);
  const locationInterval = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    getMyRoutes().then(res => {
      if (res.data.success && res.data.data) setRoutes(res.data.data);
    });
    return () => {
      if (locationInterval.current) clearInterval(locationInterval.current);
    };
  }, []);

  const startLocationBroadcast = (tripId: string) => {
    locationInterval.current = setInterval(() => {
      navigator.geolocation.getCurrentPosition(
        pos => {
          updateLocation(tripId, pos.coords.latitude, pos.coords.longitude)
            .catch(err => {
              if (err.response?.status === 403 || err.response?.status === 401) {
                // token expired — stop broadcasting
                if (locationInterval.current) clearInterval(locationInterval.current);
              }
            });
        },
        err => console.warn('Geolocation error:', err)
      );
    }, 5000);
  };

  const handleStartTrip = async () => {
    if (!selectedRouteId) return;
    setLoading(true);
    try {
      const res = await startTrip(selectedRouteId, tripType);
      if (res.data.success && res.data.data) {
        setActiveTrip(res.data.data);
        startLocationBroadcast(res.data.data.id);
        const studRes = await fetchStudents(selectedRouteId);
        if (studRes.data.success && studRes.data.data) {
          setStudents(studRes.data.data);
        }
      }
    } finally {
      setLoading(false);
    }
  };

  const handleEndTrip = async () => {
    if (!activeTrip) return;
    if (!confirm('End this trip?')) return;
    setLoading(true);
    try {
      await endTrip(activeTrip.id);
      if (locationInterval.current) clearInterval(locationInterval.current);
      setActiveTrip(null);
      setStudents([]);
      setProcessedStudents(new Set());
    } finally {
      setLoading(false);
    }
  };

  const handleStudentEvent = async (studentId: string) => {
    if (!activeTrip) return;
    navigator.geolocation.getCurrentPosition(async pos => {
      const res = await recordTripEvent(activeTrip.id, {
        studentId,
        latitude: pos.coords.latitude,
        longitude: pos.coords.longitude,
      });
      if (res.data.success) {
        setProcessedStudents(prev => new Set([...prev, studentId]));
      }
    });
  };

  if (!activeTrip) {
    return (
      <div style={styles.container}>
        <h3 style={styles.title}>Start a Trip</h3>
        <div style={styles.form}>
          <select style={styles.input} value={selectedRouteId}
            onChange={e => setSelectedRouteId(e.target.value)}>
            <option value="">Select route</option>
            {routes.map(r => (
              <option key={r.id} value={r.id}>{r.name}</option>
            ))}
          </select>
          <select style={styles.input} value={tripType}
            onChange={e => setTripType(e.target.value)}>
            <option value="Morning">Morning (pickup)</option>
            <option value="Afternoon">Afternoon (dropoff)</option>
          </select>
          <button
            style={{ ...styles.startBtn, opacity: !selectedRouteId ? 0.5 : 1 }}
            onClick={handleStartTrip}
            disabled={!selectedRouteId || loading}
          >
            {loading ? 'Starting...' : 'Start Trip'}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div style={styles.container}>
      <div style={styles.tripHeader}>
        <div>
          <p style={styles.tripTitle}>Trip Active 🟢</p>
          <p style={styles.tripSub}>{activeTrip.routeName} · {activeTrip.tripType}</p>
        </div>
        <button style={styles.endBtn} onClick={handleEndTrip} disabled={loading}>
          End Trip
        </button>
      </div>

      <p style={styles.instruction}>
        Tap a student when you {activeTrip.tripType === 'Morning' ? 'pick them up' : 'drop them off'}.
      </p>

      {students.map(student => {
        const done = processedStudents.has(student.id);
        return (
          <div key={student.id} style={{ ...styles.studentCard, opacity: done ? 0.5 : 1 }}>
            <div>
              <p style={styles.studentName}>{student.fullName}</p>
              <p style={styles.studentSchool}>{student.schoolName}</p>
            </div>
            <button
              style={{ ...styles.eventBtn, background: done ? '#888' : '#1a73e8' }}
              onClick={() => handleStudentEvent(student.id)}
              disabled={done}
            >
              {done
                ? (activeTrip.tripType === 'Morning' ? 'Picked up ✓' : 'Dropped off ✓')
                : (activeTrip.tripType === 'Morning' ? 'Picked up' : 'Dropped off')
              }
            </button>
          </div>
        );
      })}
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: { background: '#fff', borderRadius: 12, padding: '1rem' },
  title: { margin: '0 0 16px', fontSize: 16, fontWeight: 600 },
  form: { display: 'flex', flexDirection: 'column', gap: 10 },
  input: {
    padding: '10px 12px', border: '1px solid #ddd',
    borderRadius: 8, fontSize: 14, outline: 'none',
  },
  startBtn: {
    background: '#1a73e8', color: '#fff', border: 'none',
    borderRadius: 8, padding: '11px', fontSize: 15, fontWeight: 600, cursor: 'pointer',
  },
  tripHeader: {
    display: 'flex', justifyContent: 'space-between',
    alignItems: 'center', marginBottom: 16,
  },
  tripTitle: { margin: 0, fontWeight: 700, fontSize: 16, color: '#2e7d32' },
  tripSub: { margin: 0, fontSize: 12, color: '#888', marginTop: 2 },
  endBtn: {
    background: '#d32f2f', color: '#fff', border: 'none',
    borderRadius: 8, padding: '8px 16px', cursor: 'pointer', fontWeight: 600,
  },
  instruction: { fontSize: 13, color: '#666', marginBottom: 12 },
  studentCard: {
    display: 'flex', justifyContent: 'space-between', alignItems: 'center',
    background: '#f9f9f9', borderRadius: 10, padding: '0.85rem 1rem', marginBottom: 8,
  },
  studentName: { margin: 0, fontWeight: 600, fontSize: 14 },
  studentSchool: { margin: 0, fontSize: 12, color: '#888', marginTop: 2 },
  eventBtn: {
    color: '#fff', border: 'none', borderRadius: 8,
    padding: '7px 14px', cursor: 'pointer', fontSize: 13, fontWeight: 600,
  },
};

export default ActiveTrip;