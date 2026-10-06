import { Routes, Route, Navigate } from 'react-router-dom';
import Navbar from './components/Navbar';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import AccountsListPage from './pages/AccountsListPage';
import AccountDetailPage from './pages/AccountDetailPage';
import MoneyActionPage from './pages/MoneyActionPage';

export default function App() {
  return (
    <>
      <Navbar />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route
          path="/accounts"
          element={
            <ProtectedRoute>
              <AccountsListPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/accounts/:id"
          element={
            <ProtectedRoute>
              <AccountDetailPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/accounts/:id/action"
          element={
            <ProtectedRoute>
              <MoneyActionPage />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/accounts" replace />} />
        <Route path="*" element={<Navigate to="/accounts" replace />} />
      </Routes>
    </>
  );
}
