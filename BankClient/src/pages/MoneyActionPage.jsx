import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';

const ACTIONS = [
  { value: 'Deposit', label: 'הפקדה' },
  { value: 'Withdraw', label: 'משיכה' },
  { value: 'Transfer', label: 'העברה לחשבון אחר' }
];

export default function MoneyActionPage() {
  const { id } = useParams();
  const { auth } = useAuth();

  const [action, setAction] = useState('Deposit');
  const [amount, setAmount] = useState('');
  const [note, setNote] = useState('');
  const [toAccountId, setToAccountId] = useState('');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [conflict, setConflict] = useState(null); // הודעת 409 מוצגת בנפרד מכל שגיאה אחרת
  const [success, setSuccess] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setConflict(null);
    setSuccess(null);
    setLoading(true);

    try {
      let result;
      const numericAmount = Number(amount);

      if (action === 'Transfer') {
        result = await api.transfer(auth.token, id, {
          toAccountId: Number(toAccountId),
          amount: numericAmount,
          note: note || undefined
        });
      } else {
        result = await api.postTransaction(auth.token, id, {
          type: action,
          amount: numericAmount,
          note: note || undefined
        });
      }

      setSuccess(`הפעולה בוצעה בהצלחה. יתרה חדשה: ${result.balance.toLocaleString()} ₪`);
      setAmount('');
      setNote('');
      setToAccountId('');
    } catch (err) {
      if (err.status === 409) {
        // זה בדיוק המצב של תחרות על המשאב המוגבל - מישהו אחר עדכן את החשבון בין הקריאה לשמירה.
        // מוצג בבירור, נפרד משגיאות ולידציה רגילות, עם אפשרות לנסות שוב.
        setConflict(err.message);
      } else {
        setError(err.message);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="page-shell">
      <p className="mb-5">
        <Link className="text-sm font-semibold text-forest hover:underline" to={`/accounts/${id}`}>&rarr; חזרה לחשבון</Link>
      </p>

      <div className="panel w-full max-w-xl">
        <p className="text-sm font-semibold text-forest">ניהול חשבון</p>
        <h2 className="mb-6 mt-1 text-2xl font-bold text-ink">פעולה על חשבון {id}</h2>

        {conflict && (
          <div className="conflict-banner">
            <strong>הפעולה נכשלה (409 - התנגשות): </strong>
            {conflict}
            <div className="mt-3">
              <button className="primary-button" onClick={handleSubmit} disabled={loading}>
                נסי שוב
              </button>
            </div>
          </div>
        )}
        {error && <div className="error-banner">{error}</div>}
        {success && <div className="mb-5 rounded-md border border-emerald-200 bg-emerald-50 px-4 py-3 text-emerald-900">{success}</div>}

        <form className="space-y-4" onSubmit={handleSubmit}>
          <label className="block text-sm font-medium text-slate-700">סוג פעולה
          <select className="field" value={action} onChange={(e) => setAction(e.target.value)}>
            {ACTIONS.map((a) => (
              <option key={a.value} value={a.value}>
                {a.label}
              </option>
            ))}
          </select>
          </label>

          {action === 'Transfer' && (
            <label className="block text-sm font-medium text-slate-700">מזהה חשבון יעד
              <input className="field"
                type="number"
                value={toAccountId}
                onChange={(e) => setToAccountId(e.target.value)}
                required
              />
            </label>
          )}

          <label className="block text-sm font-medium text-slate-700">סכום
          <input className="field"
            type="number"
            step="0.01"
            min="0.01"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            required
          />
          </label>

          <label className="block text-sm font-medium text-slate-700">הערה (אופציונלי)
          <input className="field" value={note} onChange={(e) => setNote(e.target.value)} />
          </label>

          <button className="primary-button w-full" type="submit" disabled={loading}>
            {loading ? 'מבצעת...' : 'ביצוע'}
          </button>
        </form>
      </div>
    </main>
  );
}
