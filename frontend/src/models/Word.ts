export type WordSummary = {
  id: string;
  original_word: string;
  original_transcriptioned_word?: string | null;
  translated_word: string;
  createdAt: Date;
  updatedAt: Date;
};