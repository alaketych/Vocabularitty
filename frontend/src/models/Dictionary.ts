import { ReactNode } from "react";
import { WordSummary } from './_index'

export type DictionarySummary = {
    id: string;
    dictionary_name: string;
    language_id: string;
    icon?: ReactNode;
    words?: readonly WordSummary[];
};