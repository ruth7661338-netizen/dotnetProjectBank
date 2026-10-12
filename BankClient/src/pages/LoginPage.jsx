import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
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
    <div className="page-shell flex min-h-[calc(100vh-4rem)] items-start justify-center pt-10 sm:pt-16">
      <div className="panel w-full max-w-md">
        <p className="mb-2 text-sm font-semibold text-forest">מערכת הבנק</p>
        <h2 className="mb-6 text-2xl font-bold text-ink">התחברות</h2>
        {error && <div className="error-banner">{error}</div>}
        <form className="space-y-4" onSubmit={handleSubmit}>
          <label className="block text-sm font-medium text-slate-700">אימייל
          <input className="field" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>

          <label className="block text-sm font-medium text-slate-700">סיסמה
          <input className="field" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </label>

          <button className="primary-button w-full" type="submit" disabled={loading}>
            {loading ? 'מתחברת...' : 'התחברות'}
          </button>
        </form>
        <p className="mt-5 text-sm text-slate-600">
          אין לך חשבון? <Link className="font-semibold text-forest hover:underline" to="/register">הרשמה</Link>
        </p>
      </div>
    </div>
  );
}
