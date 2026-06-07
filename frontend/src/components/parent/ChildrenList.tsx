import { useState, useEffect } from 'react';
import { getMyChildren, redeemInviteCode } from '../../api/parent';
import { Student } from '../../types';

interface Props {
  onSelectStudent: (student: Student) => void;
  selectedStudent: Student | null;
}

const ChildrenList = ({ onSelectStudent, selectedStudent }: Props) => {
  const [children, setChildren] = useState<Student[]>([]);
  const [loading, setLoading] = useState(true);
  const [showRedeem, setShowRedeem] = useState(false);
  const [inviteCode, setInviteCode] = useState('');
  const [redeeming, setRedeeming] = useState(false);
  const [error, setError] = useState('');

  const fetchChildren = async () => {
    try {
      const res = await getMyChildren();
      if (res.data.success && res.data.data) {
        setChildren(res.data.data);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchChildren(); }, []);

  const handleRedeem = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setRedeeming(true);
    try {
      const res = await redeemInviteCode(inviteCode.trim().toUpperCase());
      if (res.data.success && res.data.data) {
        setChildren(prev => [...prev, res.data.data!]);
        setInviteCode('');
        setShowRedeem(false);
      } else {
        setError(res.data.message);
      }
    } catch {
      setError('Something went wrong. Please try again.');
    } finally {
      setRedeeming(false);
    }
  };

  if (loading) return <p style={{ color: '#888', fontSize: 14 }}>Loading...</p>;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h3 style={styles.title}>My Children</h3>
        <button style={styles.addBtn} onClick={() => setShowRedeem(!showRedeem)}>
          {showRedeem ? 'Cancel' : '+ Link Child'}
        </button>
      </div>

      {showRedeem && (
        <form onSubmit={handleRedeem} style={styles.form}>
          <input
            style={styles.input}
            placeholder="Enter invite code (e.g. SN-4K2X)"
            value={inviteCode}
            onChange={e => setInviteCode(e.target.value)}
            required
          />
          {error && <p style={styles.error}>{error}</p>}
          <button style={styles.redeemBtn} type="submit" disabled={redeeming}>
            {redeeming ? 'Linking...' : 'Link Child'}
          </button>
        </form>
      )}

      {children.length === 0 && (
        <p style={styles.empty}>
          No children linked yet. Ask your driver for an invite code.
        </p>
      )}

      {children.map(child => (
        <div
          key={child.id}
          style={{
            ...styles.card,
            ...(selectedStudent?.id === child.id ? styles.cardActive : {})
          }}
          onClick={() => onSelectStudent(child)}
        >
          <div style={styles.avatar}>
            {child.fullName.charAt(0).toUpperCase()}
          </div>
          <div>
            <p style={styles.name}>{child.fullName}</p>
            <p style={styles.school}>{child.schoolName}</p>
          </div>
          <span style={styles.arrow}>→</span>
        </div>
      ))}
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: { marginBottom: '1rem' },
  header: {
    display: 'flex', justifyContent: 'space-between',
    alignItems: 'center', marginBottom: 12,
  },
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
    textTransform: 'uppercase',
  },
  error: { color: '#d32f2f', fontSize: 13, margin: 0 },
  redeemBtn: {
    background: '#1a73e8', color: '#fff', border: 'none',
    borderRadius: 8, padding: '9px', cursor: 'pointer', fontWeight: 600,
  },
  empty: {
    color: '#aaa', fontSize: 13, textAlign: 'center',
    padding: '1.5rem 1rem', background: '#fff', borderRadius: 10,
  },
  card: {
    background: '#fff', borderRadius: 10, padding: '0.85rem 1rem',
    marginBottom: 8, cursor: 'pointer', border: '2px solid transparent',
    display: 'flex', alignItems: 'center', gap: 12,
  },
  cardActive: { border: '2px solid #1a73e8' },
  avatar: {
    width: 40, height: 40, borderRadius: '50%',
    background: '#e8f0fe', color: '#1a73e8',
    display: 'flex', alignItems: 'center', justifyContent: 'center',
    fontWeight: 700, fontSize: 16, flexShrink: 0,
  },
  name: { margin: 0, fontWeight: 600, fontSize: 14 },
  school: { margin: 0, fontSize: 12, color: '#888', marginTop: 2 },
  arrow: { marginLeft: 'auto', color: '#aaa', fontSize: 18 },
};

export default ChildrenList;