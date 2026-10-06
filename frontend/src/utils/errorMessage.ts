import axios from 'axios';

export function errorMessage(error: unknown) {
  if (!axios.isAxiosError(error)) {
    return 'Something went wrong. Please try again.';
  }

  return (
    error.response?.data?.message ??
    error.response?.data?.errorMessage ??
    error.response?.data?.ErrorMessage ??
    'Unable to load your data. Please try again.'
  );
}