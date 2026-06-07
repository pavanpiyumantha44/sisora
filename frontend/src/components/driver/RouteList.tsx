import { useState, useEffect } from 'react';
import { getMyRoutes, createRoute, deleteRoute } from '../../api/driver';
import { ServiceRoute } from '../../types';

interface Props {
  onSelectRoute: (id: string) => void;
  selectedRouteId: string | null;
}

const RouteList = ({ onSelectRoute, selectedRouteId }: Props) => {
  const [routes, setRoutes] = useState<ServiceRoute[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState('');
  const [area, setArea] = useState('');
  const [saving, setSaving] = useState(false);

  const fetchRoutes = async () => {
    try {
      const res = await getMyRoutes();
      if (res.data.success && res.data.data) {
        setRoutes(res.data.data);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchRoutes(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const res = await createRoute({ name, areaDescription: area });
      if (res.data.success && res.data.data) {
        setRoutes(prev => [res.data.data!, ...prev]);
        setName('');
        setArea('');
        setShowForm(false);
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (routeId: string) => {
    if (!confirm('Delete this route?')) return;
    await deleteRoute(routeId);
    setRoutes(prev => prev.filter(r => r.id !== routeId));
  };

  if (loading) return <p style={styles.loading}>Loading routes...</p>;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h3 style={styles.title}>Routes</h3>
        <button style={styles.addBtn} onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancel' : '+ Add Route'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} style={styles.form}>
          <input style={styles.input} placeholder="Route name"
            value={name} onChange={e => setName(e.target.value)} required />
          <input style={styles.input} placeholder="Area description"
            value={area} onChange={e => setArea(e.target.value)} required />
          <button style={styles.saveBtn} type="submit" disabled={saving}>
            {saving ? 'Saving...' : 'Save Route'}
          </button>
        </form>
      )}

      {routes.length === 0 && (
        <p style={styles.empty}>No routes yet. Add your first route.</p>
      )}

      {routes.map(route => (
        <div
          key={route.id}
          style={{
            ...styles.card,
            ...(selectedRouteId === route.id ? styles.cardActive : {})
          }}
          onClick={() => onSelectRoute(route.id)}
        >
          <div style={styles.cardTop}>
            <div>
              <p style={styles.routeName}>{route.name}</p>
              <p style={styles.routeArea}>{route.areaDescription}</p>
            </div>
            <div style={styles.cardRight}>
              <span style={styles.badge}>{route.studentCount} students</span>
              <button
                style={styles.deleteBtn}
                onClick={e => { e.stopPropagation(); handleDelete(route.id); }}
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: { marginBottom: '1rem' },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 },
  title: { margin: 0, fontSize: 16, fontWeight: 600 },
  addBtn: {
    background: '#1a73e8', color: '#fff', border: 'none',
    borderRadius: 8, padding: '6px 14px', cursor: 'pointer', fontSize: 13,
  },
  form: {
    background: '#fff', borderRadius: 10, padding: '1rem',
    marginBottom: 12, display: 'flex', flexDirection: 'column', gap: 8,
  },
  input: {
    padding: '9px 12px', border: '1px solid #ddd',
    borderRadius: 8, fontSize: 14, outline: 'none',
  },
  saveBtn: {
    background: '#1a73e8', color: '#fff', border: 'none',
    borderRadius: 8, padding: '9px', cursor: 'pointer', fontWeight: 600,
  },
  loading: { color: '#888', fontSize: 14 },
  empty: { color: '#aaa', fontSize: 13, textAlign: 'center', padding: '1rem' },
  card: {
    background: '#fff', borderRadius: 10, padding: '1rem',
    marginBottom: 8, cursor: 'pointer', border: '2px solid transparent',
  },
  cardActive: { border: '2px solid #1a73e8' },
  cardTop: { display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' },
  routeName: { margin: 0, fontWeight: 600, fontSize: 14 },
  routeArea: { margin: 0, fontSize: 12, color: '#888', marginTop: 2 },
  cardRight: { display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 6 },
  badge: {
    background: '#e8f0fe', color: '#1a73e8', fontSize: 11,
    padding: '2px 8px', borderRadius: 10,
  },
  deleteBtn: {
    background: 'none', border: 'none', color: '#d32f2f',
    cursor: 'pointer', fontSize: 12,
  },
};

export default RouteList;