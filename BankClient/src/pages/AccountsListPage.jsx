import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';
import { getAccountTypeLabel } from '../utils/labels';

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
    <main className="page-shell">
      <h2 className="mb-6 text-2xl font-bold text-ink">{isClerk ? 'כל החשבונות' : 'החשבונות שלי'}</h2>
      {error && <div className="error-banner">{error}</div>}
      {loading && <p className="py-8 text-sm text-slate-500">טוענת...</p>}

      {!loading && isClerk && pagedResult && (
        <>
          <div className="overflow-x-auto rounded-lg border border-emerald-950/10 bg-white shadow-sm">
          <table className="data-table">
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
                  <td>{getAccountTypeLabel(acc.type)}</td>
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
                    <Link className="font-semibold text-forest hover:underline" to={`/accounts/${acc.id}`}>פרטים</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>

          <div className="mt-5 flex flex-wrap items-center gap-3 text-sm text-slate-600">
            <button className="secondary-button" disabled={pageNumber <= 1} onClick={() => setPageNumber((p) => p - 1)}>
              הקודם
            </button>
            <span>
              עמוד {pagedResult.pageNumber} מתוך {pagedResult.totalPages || 1} (סה"כ {pagedResult.totalCount} חשבונות)
            </span>
            <button className="secondary-button"
              disabled={pageNumber >= pagedResult.totalPages}
              onClick={() => setPageNumber((p) => p + 1)}
            >
              הבא
            </button>
          </div>
        </>
      )}

      {!loading && !isClerk && customer && (
        <div className="panel">
          <h3 className="text-lg font-bold text-ink">{customer.fullName}</h3>
          <p className="mb-5 mt-1 text-sm text-slate-500">{customer.email}</p>
          <div className="overflow-x-auto">
          <table className="data-table">
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
                  <td>{getAccountTypeLabel(acc.type)}</td>
                  <td>{acc.balance.toLocaleString()} ₪</td>
                  <td>
                    <Link className="font-semibold text-forest hover:underline" to={`/accounts/${acc.id}`}>פרטים</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>
        </div>
      )}
    </main>
  );
}
