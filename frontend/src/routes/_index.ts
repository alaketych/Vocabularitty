// Language
export const API_CREATE_LANGUAGE = '/language';
export const API_GET_LANGUAGES = '/languages';
export const API_GET_LANGUAGE = (id: string) => `/language/${id}`;
export const API_UPDATE_LANGUAGE = (id: string) => `/language/${id}`;
export const API_DELETE_LANGUAGE = (id: string) => `/language/${id}`;


// Dictionary
export const API_CREATE_DICTIONARY = '/dictionary';
export const API_GET_DICTIONARIES = '/dictionaries';
export const API_GET_DICTIONARY = (id: string) => `/dictionary/${id}`;
export const API_UPDATE_DICTIONARY = (id: string) => `/dictionary/${id}`;
export const API_DELETE_DICTIONARY = (id: string) => `/dictionary/${id}`;
export const API_UPDATE_DICTIONARY_ORDER = '/dictionary/order';


// Words
export const API_CREATE_WORD = (dictionaryId: string) =>
  `/dictionary/${dictionaryId}/word`;

export const API_GET_DICTIONARY_WORDS = (dictionaryId: string) =>
  `/dictionary/${dictionaryId}/words`;

export const API_GET_DICTIONARY_WORD = (dictionaryId: string, wordId: string) =>
  `/dictionary/${dictionaryId}/word/${wordId}`;

export const API_UPDATE_DICTIONARY_WORD = (dictionaryId: string, wordId: string) =>
  `/dictionary/${dictionaryId}/word/${wordId}`;

export const API_DELETE_DICTIONARY_WORD = (dictionaryId: string, wordId: string) =>
  `/dictionary/${dictionaryId}/word/${wordId}`;


// User
export const API_GET_USERS = '/users';
export const API_GET_USER = (id: string) => `/user/${id}`;
export const API_GET_USER_DICTIONARIES = (id: string) =>
  `/user/${id}/dictionaries`;


// User Actions
export const API_LOGIN = '/user/login';
export const API_REGISTER = '/user/register';
export const API_GET_USER_ACTIVITY = '/user/activities';