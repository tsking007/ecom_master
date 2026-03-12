import { ToastContainer } from 'react-toastify';
import AppRouter from './routes/AppRouter.jsx';
import { useTokenRefresh } from './hooks/useTokenRefresh.js';
import './App.css';

function App() {
  useTokenRefresh();

  return (
    <>
      <AppRouter />
      <ToastContainer
        position="top-right"
        autoClose={3000}
        hideProgressBar={false}
        newestOnTop
        closeOnClick
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="colored"
      />
    </>
  );
}

export default App;