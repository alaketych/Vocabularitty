import api from './api';
import axios from 'axios';
import {
  API_GET_DICTIONARIES,
  API_GET_LANGUAGE,
  API_GET_LANGUAGES,
  API_GET_DICTIONARY,
  API_GET_DICTIONARY_WORDS,
  API_CREATE_DICTIONARY,
  API_UPDATE_DICTIONARY,
  API_DELETE_DICTIONARY,
  API_CREATE_WORD,
  API_DELETE_DICTIONARY_WORD,
} from '../routes/_index';
import type { 
  DictionarySummary, 
  WordSummary, 
  LanguageSummary 
} from '../components/../models/_index'

export type Page<T> = { pageNumber: number; pageSize: number; data: T[] };
export type OperationResponse = { isSuccessfull: boolean; message: string };

export async function getWords(id: string, pageNumber = 1, pageSize = 12, signal?: AbortSignal) {
  const response = await api.get<Page<WordSummary>>(API_GET_DICTIONARY_WORDS(id), { params: { pageNumber, pageSize }, signal });
  return response.data;
}

async function withIcon(dictionary: DictionarySummary, signal?: AbortSignal) {
  try {
    const language = await api.get<{ icon?: string }>(API_GET_LANGUAGE(dictionary.language_id), { signal });
    return { ...dictionary, icon: language.data.icon };
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) return dictionary;
    throw error;
  }
}

export async function getLanguages(pageNumber = 1, pageSize = 100, signal?: AbortSignal) {
  const response = await api.get<Page<LanguageSummary>>(API_GET_LANGUAGES, { params: { pageNumber, pageSize }, signal });
  return response.data;
}

export async function getDictionary(id: string, signal?: AbortSignal) {
  const response = await api.get<DictionarySummary>(API_GET_DICTIONARY(id), { signal });
  return withIcon(response.data, signal);
}

export async function getDictionaries(pageNumber = 1, pageSize = 12, signal?: AbortSignal) {
  const response = await api.get<Page<DictionarySummary>>(API_GET_DICTIONARIES, { params: { pageNumber, pageSize }, signal });
  const data = await Promise.all(response.data.data.map(async dictionary => {
    const [details, words] = await Promise.all([withIcon(dictionary, signal), getWords(dictionary.id, 1, 3, signal)]);
    return { ...details, words: words.data };
  }));
  return { ...response.data, data };
}

export const createDictionary = (dictionary: DictionarySummary) =>
  api.post<OperationResponse>(API_CREATE_DICTIONARY, {
    dictionary_name: dictionary.dictionary_name, language_id: dictionary.language_id,
  });

export const updateDictionary = (
  dictionaryId: string,
  dictionaryTitle: string,
  languageId: string
) =>
  api.put<OperationResponse>(API_UPDATE_DICTIONARY(dictionaryId), {
    dictionary_name: dictionaryTitle,
    language_id: languageId,
  });

export const deleteDictionary = (dictionaryId: string) =>
  api.delete(API_DELETE_DICTIONARY(dictionaryId));

export const createWord = (
  dictionaryId: string,
  word: WordSummary
) =>
  api.post<OperationResponse>(API_CREATE_WORD(dictionaryId), {
    original_word: word.original_word,
    original_transcriptioned_word: word.original_transcriptioned_word,
    translated_word: word.translated_word,
  });

export const deleteWord = (
  dictionaryId: string,
  wordId: string
) =>
  api.delete(API_DELETE_DICTIONARY_WORD(dictionaryId, wordId));
