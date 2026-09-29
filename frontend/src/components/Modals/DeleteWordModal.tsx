import type { RefObject } from 'react';
import { Trash2 } from 'lucide-react';
import Modal from './Modal';

type Props = {
    dialogRef: RefObject<HTMLDialogElement | null>;
    wordTitle: string;
    dictionaryTitle: string;
    onConfirm: () => void;
    onClose: () => void;
};

export default function DeleteWordDictionaryModal({ dialogRef, wordTitle, dictionaryTitle, onConfirm, onClose }: Props) {
    return (
        <Modal 
            dialogRef={dialogRef} title="Delete word?"
            description={`Remove “${wordTitle}” from your the ${dictionaryTitle} dictionary ?`}
            icon={<Trash2 size={25} />} onClose={onClose}
        footer={
        <>
          <button type="button" className="modal-cancel" autoFocus onClick={() => dialogRef.current?.close()}>Cancel</button>
          <button type="button" className="primary-button" onClick={() => {
            dialogRef.current?.close();
            onConfirm();
          }}>Delete Word</button>
        </>
      }
    />
  );
}