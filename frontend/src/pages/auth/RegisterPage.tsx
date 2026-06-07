import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { registerDriver, registerParent } from '../../api/auth';
import { useAuth } from '../../hooks/useAuth';

const RegisterPage = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [role, setRole] = useState<'Parent' | 'Driver'>('Parent');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  // parent fields
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');

  // driver extra fields
  const [nic, setNic] = useState('');
  const [vehicleModel, setVehicleModel] = useState('');
  const [registrationNumber, setRegistrationNumber] = useState('');
  const [vehicleColor, setVehicleColor] = useState('');
  const [vehicleType, setVehicleType] = useState('Van');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      let response;

      if (role === 'Parent') {
        response = await registerParent({ fullName, phone, password });
      } else {
        response = await registerDriver({
          fullName, phone, password, nic,
          vehicleModel, registrationNumber,
          vehicleColor, vehicleType
        });
      }

      const data = response.data;

      if (!data.success || !data.data) {
        setError(data.message);
        return;
      }

      if (role === 'Parent') {
        login({
          accessToken: data.data.accessToken,
          refreshToken: data.data.refreshToken,
          role: 'Parent',
          fullName: data.data.fullName,
          userId: data.data.userId
        });
        navigate('/parent');
      } else {
        // driver is pending — redirect to login with message
        navigate('/login');
      }

    } catch {
      setError('Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        <h1 style={styles.logo}>Sisora</h1>
        <p style={styles.tagline}>Create your account</p>

        <div style={styles.roleRow}>
          {(['Parent', 'Driver'] as const).map(r => (
            <button
              key={r}
              onClick={() => setRole(r)}
              style={{ ...styles.roleBtn, ...(role === r ? styles.roleBtnActive : {}) }}
            >
              {r}
            </button>
          ))}
        </div>

        <form onSubmit={handleSubmit} style={styles.form}>
          <input style={styles.input} placeholder="Full name" value={fullName}
            onChange={e => setFullName(e.target.value)} required />
          <input style={styles.input} placeholder="Phone number" value={phone}
            onChange={e => setPhone(e.target.value)} required />
          <input style={styles.input} type="password" placeholder="Password" value={password}
            onChange={e => setPassword(e.target.value)} required />

          {role === 'Driver' && (
            <>
              <input style={styles.input} placeholder="NIC number" value={nic}
                onChange={e => setNic(e.target.value)} required />
              <input style={styles.input} placeholder="Vehicle model (e.g. Toyota HiAce)" value={vehicleModel}
                onChange={e => setVehicleModel(e.target.value)} required />
              <input style={styles.input} placeholder="Registration number" value={registrationNumber}
                onChange={e => setRegistrationNumber(e.target.value)} required />
              <input style={styles.input} placeholder="Vehicle color" value={vehicleColor}
                onChange={e => setVehicleColor(e.target.value)} />
              <select style={styles.input} value={vehicleType}
                onChange={e => setVehicleType(e.target.value)}>
                <option value="Van">Van</option>
                <option value="MiniVan">Mini Van</option>
                <option value="Bus">Bus</option>
              </select>
              <p style={styles.notice}>
                Your account will be reviewed by an admin before you can start trips.
              </p>
            </>
          )}

          {error && <p style={styles.error}>{error}</p>}

          <button style={styles.submitBtn} type="submit" disabled={loading}>
            {loading ? 'Creating account...' : 'Create account'}
          </button>
        </form>

        <p style={styles.loginLink}>
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: {
    minHeight: '100vh',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    background: '#f5f5f5',
    padding: '1rem',
  },
  card: {
    background: '#fff',
    borderRadius: 12,
    padding: '2rem',
    width: '100%',
    maxWidth: 420,
    boxShadow: '0 2px 16px rgba(0,0,0,0.08)',
  },
  logo: {
    textAlign: 'center',
    fontSize: 28,
    fontWeight: 700,
    color: '#1a73e8',
    margin: 0,
  },
  tagline: {
    textAlign: 'center',
    color: '#888',
    fontSize: 13,
    marginTop: 4,
    marginBottom: 24,
  },
  roleRow: { display: 'flex', gap: 8, marginBottom: 20 },
  roleBtn: {
    flex: 1, padding: '8px 0', border: '1px solid #ddd',
    borderRadius: 8, background: '#fff', cursor: 'pointer', fontSize: 13, color: '#555',
  },
  roleBtnActive: {
    background: '#1a73e8', color: '#fff',
    border: '1px solid #1a73e8', fontWeight: 600,
  },
  form: { display: 'flex', flexDirection: 'column', gap: 12 },
  input: {
    padding: '10px 14px', border: '1px solid #ddd',
    borderRadius: 8, fontSize: 14, outline: 'none',
  },
  notice: { fontSize: 12, color: '#888', margin: 0 },
  error: { color: '#d32f2f', fontSize: 13, margin: 0 },
  submitBtn: {
    padding: '11px', background: '#1a73e8', color: '#fff',
    border: 'none', borderRadius: 8, fontSize: 15,
    fontWeight: 600, cursor: 'pointer', marginTop: 4,
  },
  loginLink: { textAlign: 'center', fontSize: 13, color: '#666', marginTop: 16 },
};

export default RegisterPage;