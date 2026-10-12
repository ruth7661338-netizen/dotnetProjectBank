import { useEffect, useState, useCallback } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';
import { getAccountTypeLabel, getTransactionTypeLabel } from '../utils/labels';

export default function AccountDetailPage() {
  const { id } = useParams();
  const { auth } = useAuth();

  const [account, setAccount] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [accountResult, transactionsResult] = await Promise.all([
        api.getAccount(auth.token, id),
        api.getTransactions(auth.token, id)
      ]);
      setAccount(accountResult);
      setTransactions(transactionsResult);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [auth, id]);

  useEffect(() => {
    load();
  }, [load]);

  if (loading) return <main className="page-shell text-sm text-slate-500">טוענת...</main>;
  if (error) return <main className="page-shell"><div className="error-banner">{error}</div></main>;
  if (!account) return null;

  return (
    <main className="page-shell">
      <p className="mb-5">
        <Link className="text-sm font-semibold text-brand hover:underline" to="/accounts">&rarr; חזרה לרשימה</Link>
      </p>

      <div className="panel mb-6">
        <p className="text-sm font-semibold text-brand">פרטי חשבון</p>
        <h2 className="mt-1 text-2xl font-bold text-ink">חשבון {account.accountNumber}</h2>
        <p className="mt-3 text-sm text-slate-600">סוג: {getAccountTypeLabel(account.type)}</p>
        <p className="mt-2 text-3xl font-bold text-brand">
          {account.balance.toLocaleString()} ₪
        </p>
        <p className="mt-3 text-sm text-slate-600">לקוח: {account.customerName}</p>
        <div className="mt-3">
          {account.tags?.map((t) => (
            <span className="tag" key={t}>
              {t}
            </span>
          ))}
        </div>
        <Link to={`/accounts/${id}/action`}>
          <button className="primary-button mt-5">הפקדה / משיכה / העברה</button>
        </Link>
      </div>

      <div className="panel">
        <h3 className="mb-4 text-lg font-bold text-ink">היסטוריית תנועות</h3>
        {transactions.length === 0 ? (
          <p>אין עדיין תנועות בחשבון.</p>
        ) : (
          <div className="overflow-x-auto">
          <table className="data-table">
            <thead>
              <tr>
                <th>תאריך</th>
                <th>סוג</th>
                <th>סכום</th>
                <th>יתרה לאחר</th>
                <th>הערה</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((t) => (
                <tr key={t.id}>
                  <td>{new Date(t.date).toLocaleString('he-IL')}</td>
                  <td>{getTransactionTypeLabel(t.type)}</td>
                  <td>{t.amount.toLocaleString()} ₪</td>
                  <td>{t.balanceAfter.toLocaleString()} ₪</td>
                  <td>{t.note}</td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>
        )}
      </div>
    </main>
  );
}
