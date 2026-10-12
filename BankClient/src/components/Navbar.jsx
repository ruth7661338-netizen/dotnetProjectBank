import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { auth, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <nav className="border-b border-brand-dark/10 bg-white shadow-sm">
      <div className="mx-auto flex min-h-16 w-full max-w-6xl flex-wrap items-center gap-x-6 gap-y-3 px-4 py-3 sm:px-6 lg:px-8">
      <strong className="text-lg font-bold text-brand">מערכת הבנק</strong>
      {auth ? (
        <>
          <Link className="text-sm font-semibold text-slate-600 transition hover:text-brand" to="/accounts">{auth.role === 'Clerk' ? 'כל החשבונות' : 'החשבונות שלי'}</Link>
          {auth.role === 'Clerk' && (
            <Link className="text-sm font-semibold text-slate-600 transition hover:text-brand" to="/clerks/new">הוספת פקיד/ה</Link>
          )}
          <span className="ms-auto text-sm text-slate-600">
            {auth.fullName} ({auth.role === 'Clerk' ? 'פקידה' : 'לקוחה'})
          </span>
          <button className="secondary-button" onClick={handleLogout}>
            התנתקות
          </button>
        </>
      ) : (
        <>
          <Link className="ms-auto text-sm font-semibold text-slate-600 transition hover:text-brand" to="/login">התחברות</Link>
          <Link className="primary-button text-sm" to="/register">הרשמה</Link>
        </>
      )}
      </div>
    </nav>
  );
}
