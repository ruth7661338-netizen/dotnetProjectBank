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
    <div className="page-shell flex min-h-[calc(100vh-4rem)] items-start justify-center pt-10 sm:pt-16">
      <div className="panel w-full max-w-md">
        <p className="mb-2 text-sm font-semibold text-forest">מערכת הבנק</p>
        <h2 className="mb-3 text-2xl font-bold text-ink">הרשמה</h2>
        <p className="mb-5 text-sm leading-6 text-slate-600">
          הרשמה יוצרת חשבון התחברות בתפקיד לקוחה. פתיחת חשבון בנק בפועל נעשית ע"י פקידה.
        </p>
        {error && <div className="error-banner">{error}</div>}
        <form className="space-y-4" onSubmit={handleSubmit}>
          <label className="block text-sm font-medium text-slate-700">שם מלא
          <input className="field" value={fullName} onChange={(e) => setFullName(e.target.value)} required />
          </label>

          <label className="block text-sm font-medium text-slate-700">אימייל
          <input className="field" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>

          <label className="block text-sm font-medium text-slate-700">סיסמה (8 תווים לפחות)
          <input className="field"
            type="password"
            value={password}
            minLength={8}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          </label>

          <button className="primary-button w-full" type="submit" disabled={loading}>
            {loading ? 'נרשמת...' : 'הרשמה'}
          </button>
        </form>
        <p className="mt-5 text-sm text-slate-600">
          כבר יש לך חשבון? <Link className="font-semibold text-forest hover:underline" to="/login">התחברות</Link>
        </p>
      </div>
    </div>
  );
}
