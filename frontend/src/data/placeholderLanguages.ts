export type LanguageOption = {
  id: string;
  name: string;
  original_name: string;
  icon?: string;
};

export const placeholderLanguages: readonly LanguageOption[] = [
  { id: 'en', name: 'English', original_name: 'English', icon: '🇬🇧' },
  { id: 'es', name: 'Spanish', original_name: 'Español', icon: '🇪🇸' },
  { id: 'fr', name: 'French', original_name: 'Français', icon: '🇫🇷' },
];
