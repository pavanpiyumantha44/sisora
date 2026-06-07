import { useState, useEffect } from 'react';
import { getStudents, addStudent, removeStudent, regenerateInviteCode } from '../../api/driver';
import { Student } from '../../types';

interface Props {
  routeId: string;
}

const StudentList = ({ routeId }: Props) => {
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [fullName, setFullName] = useState('');
  const [schoolName, setSchoolName] = useState('');
  const [pickupAddress, setPickupAddress] = useState('');
  const [saving, setSaving] = useState(false);

  const fetchStudents = async () => {
    setLoading(true);
    try {
      const res = await getStudents(routeId);
      if (res.data.success && res.data.data) {
        setStudents(res.data.data);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchStudents(); }, [routeId]);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const res = await addStudent(routeId, { fullName, schoolName, pickupAddress });
      if (res.data.success && res.data.data) {
        setStudents(prev => [...prev, res.data.data!]);
        setFullName(''); setSchoolName(''); setPickupAddress('');
        setShowForm(false);
      }
    } finally {
      setSaving(false);
    }
  };

  const handleRemove = async (studentId: string) => {
    if (!confirm('Remove this student?')) return;
    await removeStudent(routeId, studentId);
    setStudents(prev => prev.filter(s => s.id !== studentId));
  };

  const handleRegenerateCode = async (studentId: string) => {
    const res = await regenerateInviteCode(routeId, studentId);
    if (res.data.success) fetchStudents();
  };

  const shareWhatsApp = (student: Student) => {
    const msg = encodeURIComponent(
      `Hi! Please download the Sisora app and enter this code to track ${student.fullName}: *${student.inviteCode}*`
    );
    window.open(`https://wa.me/?text=${msg}`, '_blank');
  };

  if (loading) return <p style={{ color: '#888', fontSize: 14 }}>Loading students...</p>;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h3 style={styles.title}>Students</h3>
        <button style={styles.addBtn} onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancel' : '+ Add Student'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleAdd} style={styles.form}>
          <input style={styles.input} placeholder="Student full name"
            value={fullName} onChange={e => setFullName(e.target.value)} required />
          <input style={styles.input} placeholder="School name"
            value={schoolName} onChange={e => setSchoolName(e.target.value)} required />
          <input style={styles.input} placeholder="Pickup address (optional)"
            value={pickupAddress} onChange={e => setPickupAddress(e.target.value)} />
          <button style={styles.saveBtn} type="submit" disabled={saving}>
            {saving ? 'Adding...' : 'Add Student'}
          </button>
        </form>
      )}

      {students.length === 0 && (
        <p style={styles.empty}>No students on this route yet.</p>
      )}

      {students.map(student => (
        <div key={student.id} style={styles.card}>
          <div style={styles.cardTop}>
            <div>
              <p style={styles.name}>{student.fullName}</p>
              <p style={styles.school}>{student.schoolName}</p>
              {student.pickupAddress && (
                <p style={styles.address}>{student.pickupAddress}</p>
              )}
            </div>
            <button
              style={styles.removeBtn}
              onClick={() => handleRemove(student.id)}
            >
              Remove
            </button>
          </div>

          <div style={styles.codeRow}>
            <div style={styles.codeBox}>
              <span style={styles.codeLabel}>Invite code</span>
              <span style={styles.code}>{student.inviteCode}</span>
              <span style={{
                ...styles.codeStatus,
                color: student.inviteCodeUsed ? '#2e7d32' : '#f57c00'
              }}>
                {student.inviteCodeUsed ? 'Linked ✓' : 'Pending'}
              </span>
            </div>
            <div style={styles.codeActions}>
              <button style={styles.waBtn} onClick={() => shareWhatsApp(student)}>
                Share WhatsApp
              </button>
              {student.inviteCodeUsed && (
                <button
                  style={styles.regenBtn}
                  onClick={() => handleRegenerateCode(student.id)}
                >
                  New code
                </button>
              )}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: { marginTop: '1rem' },
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
  empty: { color: '#aaa', fontSize: 13, textAlign: 'center', padding: '1rem' },
  card: {
    background: '#fff', borderRadius: 10, padding: '1rem', marginBottom: 8,
  },
  cardTop: { display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' },
  name: { margin: 0, fontWeight: 600, fontSize: 14 },
  school: { margin: 0, fontSize: 12, color: '#555', marginTop: 2 },
  address: { margin: 0, fontSize: 12, color: '#888', marginTop: 2 },
  removeBtn: {
    background: 'none', border: 'none', color: '#d32f2f',
    cursor: 'pointer', fontSize: 12,
  },
  codeRow: {
    marginTop: 10, paddingTop: 10,
    borderTop: '1px solid #f0f0f0',
    display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8,
  },
  codeBox: { display: 'flex', alignItems: 'center', gap: 8 },
  codeLabel: { fontSize: 11, color: '#888' },
  code: {
    fontFamily: 'monospace', fontSize: 14, fontWeight: 700,
    background: '#f0f4ff', padding: '2px 8px', borderRadius: 6, color: '#1a73e8',
  },
  codeStatus: { fontSize: 11 },
  codeActions: { display: 'flex', gap: 8 },
  waBtn: {
    background: '#25D366', color: '#fff', border: 'none',
    borderRadius: 8, padding: '5px 10px', cursor: 'pointer', fontSize: 12,
  },
  regenBtn: {
    background: 'none', border: '1px solid #ddd',
    borderRadius: 8, padding: '5px 10px', cursor: 'pointer', fontSize: 12, color: '#555',
  },
};

export default StudentList;