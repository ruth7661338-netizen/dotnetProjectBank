import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';

export default function AccountsListPage() {
  const { auth } = useAuth();
  const isClerk = auth.role === 'Clerk';

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [pagedResult, setPagedResult] = useState(null); // Clerk
  const [customer, setCustomer] = useState(null); // Customer
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      if (isClerk) {
        const result = await api.getAccounts(auth.token, { pageNumber, pageSize });
        setPagedResult(result);
      } else {
        const result = await api.getCustomer(auth.token, auth.customerId);
        setCustomer(result);
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [auth, isClerk, pageNumber, pageSize]);

  useEffect(() => {
    load();
  }, [load]);

  return (
    <div className="page">
      <h2>{isClerk ? 'כל החשבונות' : 'החשבונות שלי'}</h2>
      {error && <div className="error-banner">{error}</div>}
      {loading && <p>טוענת...</p>}

      {!loading && isClerk && pagedResult && (
        <>
          <table>
            <thead>
              <tr>
                <th>מספר חשבון</th>
                <th>סוג</th>
                <th>יתרה</th>
                <th>לקוח</th>
                <th>תגיות</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {pagedResult.items.map((acc) => (
                <tr key={acc.id}>
                  <td>{acc.accountNumber}</td>
                  <td>{acc.type}</td>
                  <td>{acc.balance.toLocaleString()} ₪</td>
                  <td>{acc.customerName}</td>
                  <td>
                    {acc.tags?.map((t) => (
                      <span className="tag" key={t}>
                        {t}
                      </span>
                    ))}
                  </td>
                  <td>
                    <Link to={`/accounts/${acc.id}`}>פרטים</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div style={{ marginTop: 16, display: 'flex', gap: 8, alignItems: 'center' }}>
            <button disabled={pageNumber <= 1} onClick={() => setPageNumber((p) => p - 1)}>
              הקודם
            </button>
            <span>
              עמוד {pagedResult.pageNumber} מתוך {pagedResult.totalPages || 1} (סה"כ {pagedResult.totalCount} חשבונות)
            </span>
            <button
              disabled={pageNumber >= pagedResult.totalPages}
              onClick={() => setPageNumber((p) => p + 1)}
            >
              הבא
            </button>
          </div>
        </>
      )}

      {!loading && !isClerk && customer && (
        <div className="card">
          <h3>{customer.fullName}</h3>
          <p>{customer.email}</p>
          <table>
            <thead>
              <tr>
                <th>מספר חשבון</th>
                <th>סוג</th>
                <th>יתרה</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {customer.accounts.map((acc) => (
                <tr key={acc.id}>
                  <td>{acc.accountNumber}</td>
                  <td>{acc.type}</td>
                  <td>{acc.balance.toLocaleString()} ₪</td>
                  <td>
                    <Link to={`/accounts/${acc.id}`}>פרטים</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
