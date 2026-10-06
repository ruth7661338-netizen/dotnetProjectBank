const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5100';

async function request(path, { method = 'GET', body, token } = {}) {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined
  });

  let data = null;
  try {
    data = await res.json();
  } catch {
    // אין גוף תשובה (למשל 204) - לא שגיאה
  }

  if (!res.ok) {
    const error = new Error(data?.message || `שגיאה בשרת (${res.status})`);
    error.status = res.status;
    error.data = data;
    throw error;
  }

  return data;
}

export const api = {
  login: (email, password) =>
    request('/api/auth/login', { method: 'POST', body: { email, password } }),

  register: (fullName, email, password) =>
    request('/api/auth/register', { method: 'POST', body: { fullName, email, password } }),

  // רשימת כל החשבונות עם pagination אמיתי - פקידות בנק בלבד (Clerk)
  getAccounts: (token, { pageNumber = 1, pageSize = 20 } = {}) =>
    request(`/api/accounts?pageNumber=${pageNumber}&pageSize=${pageSize}`, { token }),

  getAccount: (token, id) => request(`/api/accounts/${id}`, { token }),

  getTransactions: (token, id) => request(`/api/accounts/${id}/transactions`, { token }),

  // הלקוחה רואה את עצמה ואת החשבונות שלה דרך הלקוח (customer), לא דרך רשימת כל החשבונות
  getCustomer: (token, id) => request(`/api/customers/${id}`, { token }),

  createAccount: (token, body) => request('/api/accounts', { method: 'POST', body, token }),

  // type: "Deposit" | "Withdraw"
  postTransaction: (token, accountId, body) =>
    request(`/api/accounts/${accountId}/transactions`, { method: 'POST', body, token }),

  transfer: (token, accountId, body) =>
    request(`/api/accounts/${accountId}/transfers`, { method: 'POST', body, token })
};
