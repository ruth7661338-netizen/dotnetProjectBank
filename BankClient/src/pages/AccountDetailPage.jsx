import { useEffect, useState, useCallback } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';

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

  if (loading) return <div className="page">טוענת...</div>;
  if (error) return <div className="page error-banner">{error}</div>;
  if (!account) return null;

  return (
    <div className="page">
      <p>
        <Link to="/accounts">&rarr; חזרה לרשימה</Link>
      </p>

      <div className="card">
        <h2>חשבון {account.accountNumber}</h2>
        <p>סוג: {account.type}</p>
        <p style={{ fontSize: '1.4rem' }}>
          <strong>{account.balance.toLocaleString()} ₪</strong>
        </p>
        <p>לקוח: {account.customerName}</p>
        <div>
          {account.tags?.map((t) => (
            <span className="tag" key={t}>
              {t}
            </span>
          ))}
        </div>
        <Link to={`/accounts/${id}/action`}>
          <button style={{ marginTop: 12 }}>הפקדה / משיכה / העברה</button>
        </Link>
      </div>

      <div className="card">
        <h3>היסטוריית תנועות</h3>
        {transactions.length === 0 ? (
          <p>אין עדיין תנועות בחשבון.</p>
        ) : (
          <table>
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
                  <td>{t.type}</td>
                  <td>{t.amount.toLocaleString()} ₪</td>
                  <td>{t.balanceAfter.toLocaleString()} ₪</td>
                  <td>{t.note}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
