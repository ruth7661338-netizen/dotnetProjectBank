import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await register(fullName, email, password);
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
        <h2>הרשמה</h2>
        <p style={{ fontSize: '0.85rem', color: '#666' }}>
          הרשמה יוצרת חשבון התחברות בתפקיד לקוחה. פתיחת חשבון בנק בפועל נעשית ע"י פקידה.
        </p>
        {error && <div className="error-banner">{error}</div>}
        <form onSubmit={handleSubmit}>
          <label>שם מלא</label>
          <input value={fullName} onChange={(e) => setFullName(e.target.value)} required />

          <label>אימייל</label>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />

          <label>סיסמה (8 תווים לפחות)</label>
          <input
            type="password"
            value={password}
            minLength={8}
            onChange={(e) => setPassword(e.target.value)}
            required
          />

          <button type="submit" disabled={loading}>
            {loading ? 'נרשמת...' : 'הרשמה'}
          </button>
        </form>
        <p>
          כבר יש לך חשבון? <Link to="/login">התחברות</Link>
        </p>
      </div>
    </div>
  );
}
