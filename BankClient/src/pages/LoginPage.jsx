import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('customer@bank.local');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await login(email, password);
      navigate('/accounts');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="page">
      <div className="card" style={{ maxWidth: 400, margin: '40px auto' }}>
        <h2>התחברות</h2>
        {error && <div className="error-banner">{error}</div>}
        <form onSubmit={handleSubmit}>
          <label>אימייל</label>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />

          <label>סיסמה</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />

          <button type="submit" disabled={loading}>
            {loading ? 'מתחברת...' : 'התחברות'}
          </button>
        </form>
        <p style={{ marginTop: 16, fontSize: '0.85rem', color: '#666' }}>
          משתמשי דמו: clerk@bank.local / customer@bank.local, סיסמה Demo1234!
        </p>
        <p>
          אין לך חשבון? <Link to="/register">הרשמה</Link>
        </p>
      </div>
    </div>
  );
}
