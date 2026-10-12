import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';
import { isValidIsraeliId } from '../utils/validation';

export default function AddClerkPage() {
  const { auth } = useAuth();
  const [fullName, setFullName] = useState('');
  const [idNumber, setIdNumber] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [created, setCreated] = useState(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setCreated(null);
    if (!isValidIsraeliId(idNumber)) {
      setError('מספר תעודת זהות לא תקין');
      return;
    }
    setLoading(true);
    try {
      const result = await api.createClerk(auth.token, { fullName, idNumber, email, password });
      setCreated(result);
      setFullName('');
      setIdNumber('');
      setEmail('');
      setPassword('');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="page-shell flex min-h-[calc(100vh-4rem)] items-start justify-center pt-10 sm:pt-16">
      <div className="panel w-full max-w-md">
        <p className="mb-2 text-sm font-semibold text-brand">ניהול צוות</p>
        <h2 className="mb-3 text-2xl font-bold text-ink">הוספת פקיד/ה</h2>
        <p className="mb-5 text-sm leading-6 text-slate-600">
          המשתמש החדש יקבל הרשאות פקיד/ה מלאות. את/ה נשאר/ת מחובר/ת כעצמך.
        </p>
        {error && <div className="error-banner">{error}</div>}
        {created && (
          <div className="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            הפקיד/ה {created.fullName} ({created.email}) נוספ/ה בהצלחה.
          </div>
        )}
        <form className="space-y-4" onSubmit={handleSubmit}>
          <label className="block text-sm font-medium text-slate-700">שם מלא
          <input className="field" value={fullName} onChange={(e) => setFullName(e.target.value)} required />
          </label>

          <label className="block text-sm font-medium text-slate-700">תעודת זהות (9 ספרות)
          <input className="field"
            inputMode="numeric"
            maxLength={9}
            pattern="\d{9}"
            title="9 ספרות, כולל ספרת ביקורת"
            value={idNumber}
            onChange={(e) => setIdNumber(e.target.value.replace(/\D/g, ''))}
            required
          />
          </label>

          <label className="block text-sm font-medium text-slate-700">אימייל
          <input className="field" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>

          <label className="block text-sm font-medium text-slate-700">סיסמה ראשונית (8 תווים לפחות)
          <input className="field"
            type="password"
            value={password}
            minLength={8}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          </label>

          <button className="primary-button w-full" type="submit" disabled={loading}>
            {loading ? 'מוסיף...' : 'הוספת פקיד/ה'}
          </button>
        </form>
        <p className="mt-5 text-sm text-slate-600">
          <Link className="font-semibold text-brand hover:underline" to="/accounts">חזרה לכל החשבונות</Link>
        </p>
      </div>
    </div>
  );
}
