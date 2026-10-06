import { useState, type SubmitEvent } from 'react';
import axios from 'axios';
import api from '../../api/api';
import { LoginResponse } from '../../models/_index'
import { useNavigate } from 'react-router-dom';
import { API_LOGIN } from '../../routes/_index';
import { errorMessage } from '../../utils/errorMessage';
import { useNotification } from '../../contexts/NotificationContext';

export default function Login(){
    const navigate = useNavigate();
    const { showNotification } = useNotification();
    
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: SubmitEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (!email.trim() || !password) {
            return;
        }

        try {
            const response = await api.post<LoginResponse>(
                API_LOGIN,
                {
                email: email.trim(),
                password,
                }
            );

            if (response.data.isSuccessfull === false) {
                showNotification(
                    false,
                    response.data.message || 'Unable to log in.'
                );

                return;
            }

            const token = response.data.access_token;

            if(!token) {
                showNotification(false, 'Token failed.');
                return;
            }

            localStorage.setItem('token', token);
            showNotification(true, response.data.message || 'Successfully logged in.')

            navigate('/dictionary');
        }
        catch(error) {
            showNotification(false, errorMessage(error));
        }
        finally {
            setLoading(false);
        }
    }

    return (
        <form id="login" onSubmit={handleSubmit}>
            <label>
                <p>Email</p>
                <input type='email' name='email' id='email' required value={ email } autoComplete='email' onChange={e => setEmail(e.target.value)}></input>
            </label>

            <label>
                <p>Password</p>
                <input type='password' id='password' required value={password} autoComplete="current-password" onChange={e => setPassword(e.target.value)}></input>
            </label>

            <button type='submit' disabled={loading}>{loading ? 'Logging in...' : 'Login'}</button>
        </form>
    )
}