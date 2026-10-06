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
    <nav>
      <strong>מערכת הבנק</strong>
      {auth ? (
        <>
          <Link to="/accounts">{auth.role === 'Clerk' ? 'כל החשבונות' : 'החשבונות שלי'}</Link>
          <span style={{ marginInlineStart: 'auto' }}>
            {auth.fullName} ({auth.role === 'Clerk' ? 'פקידה' : 'לקוחה'})
          </span>
          <button className="secondary" onClick={handleLogout}>
            התנתקות
          </button>
        </>
      ) : (
        <>
          <Link to="/login">התחברות</Link>
          <Link to="/register">הרשמה</Link>
        </>
      )}
    </nav>
  );
}
