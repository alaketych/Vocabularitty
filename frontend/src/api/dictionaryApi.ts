import api from './api';
import {
  API_GET_DICTIONARIES,
  API_GET_LANGUAGES,
  API_GET_DICTIONARY_WORDS,
  API_CREATE_DICTIONARY,
  API_UPDATE_DICTIONARY,
  API_DELETE_DICTIONARY,
  API_CREATE_WORD,
  API_DELETE_DICTIONARY_WORD,
} from '../routes/_index';

import type { DictionarySummary } from '../components/Dictionary/DictionaryList';
import type { WordSummary } from '../components/Dictionary/Word';

export const getDictionaries = async () => {
  const [response, languages] = await Promise.all([
    api.get<DictionarySummary[]>(API_GET_DICTIONARIES),
    api.get<{ id: string; icon?: string }[]>(API_GET_LANGUAGES),
  ]);
  const data = await Promise.all(response.data.map(async dictionary => {
    const words = await api.get<WordSummary[]>(API_GET_DICTIONARY_WORDS(dictionary.id));
    return { ...dictionary, words: words.data,
      icon: languages.data.find(language => language.id === dictionary.language_id)?.icon };
  }));
  return { ...response, data };
};

export const createDictionary = (dictionary: DictionarySummary) =>
  api.post(API_CREATE_DICTIONARY, {
    dictionary_name: dictionary.dictionary_name, language_id: dictionary.language_id,
  });

export const updateDictionary = (
  dictionaryId: string,
  dictionaryTitle: string
) =>
  api.put(API_UPDATE_DICTIONARY(dictionaryId), {
    dictionary_name: dictionaryTitle,
  });

export const deleteDictionary = (dictionaryId: string) =>
  api.delete(API_DELETE_DICTIONARY(dictionaryId));

export const createWord = (
  dictionaryId: string,
  word: WordSummary
) =>
  api.post(API_CREATE_WORD(dictionaryId), {
    original_word: word.original_word,
    original_transcriptioned_word: word.original_transcriptioned_word,
    translated_word: word.translated_word,
  });

export const deleteWord = (
  dictionaryId: string,
  wordId: string
) =>
  api.delete(API_DELETE_DICTIONARY_WORD(dictionaryId, wordId));
