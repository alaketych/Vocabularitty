import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.DEV ? '/api' : (import.meta.env.VITE_BACKEND_API || '/api'),
  timeout: 15000,
});

export default api;
