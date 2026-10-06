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
    <div className="page">
      <p>
        <Link to={`/accounts/${id}`}>&rarr; חזרה לחשבון</Link>
      </p>

      <div className="card" style={{ maxWidth: 420 }}>
        <h2>פעולה על חשבון {id}</h2>

        {conflict && (
          <div className="conflict-banner">
            <strong>הפעולה נכשלה (409 - התנגשות): </strong>
            {conflict}
            <div style={{ marginTop: 8 }}>
              <button onClick={handleSubmit} disabled={loading}>
                נסי שוב
              </button>
            </div>
          </div>
        )}
        {error && <div className="error-banner">{error}</div>}
        {success && <div className="card" style={{ background: '#eaf7ea' }}>{success}</div>}

        <form onSubmit={handleSubmit}>
          <label>סוג פעולה</label>
          <select value={action} onChange={(e) => setAction(e.target.value)}>
            {ACTIONS.map((a) => (
              <option key={a.value} value={a.value}>
                {a.label}
              </option>
            ))}
          </select>

          {action === 'Transfer' && (
            <>
              <label>מזהה חשבון יעד</label>
              <input
                type="number"
                value={toAccountId}
                onChange={(e) => setToAccountId(e.target.value)}
                required
              />
            </>
          )}

          <label>סכום</label>
          <input
            type="number"
            step="0.01"
            min="0.01"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            required
          />

          <label>הערה (אופציונלי)</label>
          <input value={note} onChange={(e) => setNote(e.target.value)} />

          <button type="submit" disabled={loading}>
            {loading ? 'מבצעת...' : 'ביצוע'}
          </button>
        </form>
      </div>
    </div>
  );
}
