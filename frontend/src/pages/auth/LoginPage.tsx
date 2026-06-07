import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { loginDriver, loginParent, loginAdmin } from '../../api/auth';
import { useAuth } from '../../hooks/useAuth';
import { requestNotificationPermission } from '../../utils/firebase';
import { registerFcmToken } from '../../api/auth';

const LoginPage = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [role, setRole] = useState<'Driver' | 'Parent' | 'Admin'>('Parent');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      let response;

      if (role === 'Driver') {
        response = await loginDriver({ phone, password });
      } else if (role === 'Parent') {
        response = await loginParent({ phone, password });
      } else {
        response = await loginAdmin({ email, password });
      }

      const data = response.data;

      if (!data.success || !data.data) {
        setError(data.message);
        return;
      }

      login({
        accessToken: data.data.accessToken,
        refreshToken: data.data.refreshToken,
        role: data.data.role as 'Driver' | 'Parent' | 'Admin',
        fullName: data.data.fullName,
        userId: data.data.userId
      });
      
      const fcmToken = await requestNotificationPermission();
      if (fcmToken) {
        await registerFcmToken(fcmToken);
      }

      if (data.data.role === 'Driver') navigate('/driver');
      else if (data.data.role === 'Parent') navigate('/parent');

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
        <p style={styles.tagline}>School transport tracking</p>

        {/* role selector */}
        <div style={styles.roleRow}>
          {(['Parent', 'Driver', 'Admin'] as const).map(r => (
            <button
              key={r}
              onClick={() => setRole(r)}
              style={{
                ...styles.roleBtn,
                ...(role === r ? styles.roleBtnActive : {})
              }}
            >
              {r}
            </button>
          ))}
        </div>

        <form onSubmit={handleSubmit} style={styles.form}>
          {role === 'Admin' ? (
            <input
              style={styles.input}
              type="email"
              placeholder="Email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
            />
          ) : (
            <input
              style={styles.input}
              type="tel"
              placeholder="Phone number"
              value={phone}
              onChange={e => setPhone(e.target.value)}
              required
            />
          )}

          <input
            style={styles.input}
            type="password"
            placeholder="Password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            required
          />

          {error && <p style={styles.error}>{error}</p>}

          <button style={styles.submitBtn} type="submit" disabled={loading}>
            {loading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        {role !== 'Admin' && (
          <p style={styles.registerLink}>
            Don't have an account?{' '}
            <Link to="/register">Register</Link>
          </p>
        )}
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
  },
  card: {
    background: '#fff',
    borderRadius: 12,
    padding: '2rem',
    width: '100%',
    maxWidth: 400,
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
  roleRow: {
    display: 'flex',
    gap: 8,
    marginBottom: 20,
  },
  roleBtn: {
    flex: 1,
    padding: '8px 0',
    border: '1px solid #ddd',
    borderRadius: 8,
    background: '#fff',
    cursor: 'pointer',
    fontSize: 13,
    color: '#555',
  },
  roleBtnActive: {
    background: '#1a73e8',
    color: '#fff',
    border: '1px solid #1a73e8',
    fontWeight: 600,
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    gap: 12,
  },
  input: {
    padding: '10px 14px',
    border: '1px solid #ddd',
    borderRadius: 8,
    fontSize: 14,
    outline: 'none',
  },
  error: {
    color: '#d32f2f',
    fontSize: 13,
    margin: 0,
  },
  submitBtn: {
    padding: '11px',
    background: '#1a73e8',
    color: '#fff',
    border: 'none',
    borderRadius: 8,
    fontSize: 15,
    fontWeight: 600,
    cursor: 'pointer',
    marginTop: 4,
  },
  registerLink: {
    textAlign: 'center',
    fontSize: 13,
    color: '#666',
    marginTop: 16,
  },
};

export default LoginPage;