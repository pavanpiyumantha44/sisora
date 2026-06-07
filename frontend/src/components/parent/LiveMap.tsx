import { useState, useEffect, useRef } from 'react';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import * as signalR from '@microsoft/signalr';
import { Student, LiveLocation, Trip } from '../../types';
import { getActiveTrip } from '../../api/trip';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

// fix leaflet default marker icon issue with vite
delete (L.Icon.Default.prototype as unknown as Record<string, unknown>)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

interface Props {
  student: Student;
}

const LiveMap = ({ student }: Props) => {
  const [activeTrip, setActiveTrip] = useState<Trip | null>(null);
  const [location, setLocation] = useState<LiveLocation | null>(null);
  const [studentStatus, setStudentStatus] = useState<string>('Waiting');
  const [signalLost, setSignalLost] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  // fetch active trip for this student's route
  useEffect(() => {
    const fetchActiveTrip = async () => {
      try {
        const res = await getActiveTrip(student.routeId);
        if (res.data.success && res.data.data) {
          setActiveTrip(res.data.data);
        }
      } catch {
        // no active trip
      }
    };
    fetchActiveTrip();
  }, [student]);

  // connect to SignalR when active trip found
  useEffect(() => {
    if (!activeTrip) return;

    const token = localStorage.getItem('accessToken');

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_HUB_URL}`, {
        accessTokenFactory: () => token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    // listen for location updates
    connection.on('LocationUpdated', (data: LiveLocation) => {
      setLocation(data);
      setSignalLost(false);
    });

    // listen for student events
    connection.on('StudentEventRecorded', (data: { studentId: string; eventType: string }) => {
      if (data.studentId === student.id) {
        setStudentStatus(data.eventType === 'PickedUp' ? 'Picked up ✓' : 'Dropped off ✓');
      }
    });

    // listen for trip end
    connection.on('TripEnded', () => {
      setActiveTrip(null);
      setLocation(null);
      setStudentStatus('Trip ended');
    });

    connection.start()
      .then(() => connection.invoke('JoinTripGroup', activeTrip.id))
      .catch(console.error);

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [activeTrip, student.id]);

  // signal loss detection — if no update in 15s show warning
  useEffect(() => {
    if (!location) return;
    const timer = setTimeout(() => setSignalLost(true), 15000);
    return () => clearTimeout(timer);
  }, [location]);

  const defaultCenter: [number, number] = [6.9271, 79.8612]; // Colombo
  const mapCenter: [number, number] = location
    ? [location.latitude, location.longitude]
    : defaultCenter;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <div>
          <p style={styles.childName}>{student.fullName}</p>
          <p style={styles.school}>{student.schoolName}</p>
        </div>
        <div style={{
          ...styles.statusBadge,
          background: studentStatus.includes('✓') ? '#e8f5e9' : '#fff3e0',
          color: studentStatus.includes('✓') ? '#2e7d32' : '#f57c00',
        }}>
          {studentStatus}
        </div>
      </div>

      {signalLost && (
        <div style={styles.signalLost}>
          ⚠ Signal lost — showing last known location
        </div>
      )}

      {!activeTrip ? (
        <div style={styles.noTrip}>
          <p>🚐</p>
          <p>No active trip right now.</p>
          <p style={styles.noTripSub}>You'll get a notification when the van starts.</p>
        </div>
      ) : (
        <MapContainer
          center={mapCenter}
          zoom={14}
          style={styles.map}
        >
          <TileLayer
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            attribution='&copy; OpenStreetMap contributors'
          />
          {location && (
            <Marker position={[location.latitude, location.longitude]}>
              <Popup>
                Van location<br />
                Last updated: {new Date(location.timestamp).toLocaleTimeString()}
              </Popup>
            </Marker>
          )}
        </MapContainer>
      )}

      {activeTrip && (
        <div style={styles.tripInfo}>
          <p style={styles.tripInfoText}>
            🟢 Trip active · {activeTrip.tripType} · Started {new Date(activeTrip.startedAt).toLocaleTimeString()}
          </p>
        </div>
      )}
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: {
    background: '#fff', borderRadius: 12, overflow: 'hidden',
    marginTop: '1rem',
  },
  header: {
    padding: '1rem', display: 'flex',
    justifyContent: 'space-between', alignItems: 'center',
  },
  childName: { margin: 0, fontWeight: 600, fontSize: 15 },
  school: { margin: 0, fontSize: 12, color: '#888', marginTop: 2 },
  statusBadge: {
    fontSize: 12, fontWeight: 600,
    padding: '4px 12px', borderRadius: 20,
  },
  signalLost: {
    background: '#fff3e0', color: '#f57c00',
    padding: '8px 1rem', fontSize: 13,
  },
  noTrip: {
    textAlign: 'center', padding: '2rem',
    color: '#666', fontSize: 14,
  },
  noTripSub: { fontSize: 12, color: '#aaa', marginTop: 4 },
  map: { height: 300, width: '100%' },
  tripInfo: {
    padding: '0.75rem 1rem',
    borderTop: '1px solid #f0f0f0',
  },
  tripInfoText: { margin: 0, fontSize: 12, color: '#555' },
};

export default LiveMap;