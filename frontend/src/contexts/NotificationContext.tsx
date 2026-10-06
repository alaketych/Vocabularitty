import { 
    useState,
    useContext,
    createContext,
    PropsWithChildren
} from 'react'

type NotificationState = {
    isSuccessfull: boolean;
    closed: boolean;
    message: string;
}

type NotificationContextType = {
    notification: NotificationState,
    showNotification: (
        isSuccessfull: boolean,
        message: string,
    ) => void,
    closeNotification: () => void
}

const NotificationContext = createContext<NotificationContextType | null>(null);

export function NotificationProvider({ children }: PropsWithChildren) {
    const [notification, setNotification] = useState<NotificationState>({
        isSuccessfull: false,
        closed: true,
        message: ''
    });

    const showNotification = (
        isSuccessfull: boolean,
        message: string
    ) => {
        setNotification({
        isSuccessfull,
        closed: false,
        message,
        });
    };

    const closeNotification = () => {
        setNotification(state => ({
        ...state,
        closed: true,
        }));
    };

    return (
        <NotificationContext.Provider
            value={{
                notification,
                showNotification,
                closeNotification
            }}
        >
            { children }
        </NotificationContext.Provider>
    )
}

export function useNotification() {
    const context = useContext(NotificationContext);

    if(!context) {
        throw new Error('useNotification must be used inside Providerr.')
    }

    return context;
}