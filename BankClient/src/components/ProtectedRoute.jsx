import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

// role (אופציונלי): מגביל את הדף לתפקיד מסוים, למשל 'Clerk'. משתמש בתפקיד אחר מוחזר לרשימת החשבונות.
export default function ProtectedRoute({ children, role }) {
  const { auth } = useAuth();
  if (!auth) {
    return <Navigate to="/login" replace />;
  }
  if (role && auth.role !== role) {
    return <Navigate to="/accounts" replace />;
  }
  return children;
}
