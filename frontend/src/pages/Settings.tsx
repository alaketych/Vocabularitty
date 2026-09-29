import { Settings as SettingsIcon } from 'lucide-react';
import { useEffect } from 'react';

type Props = {
  reducedMotion: boolean;
  onReducedMotionChange: (value: boolean) => void;
};

export default function Settings({ reducedMotion, onReducedMotionChange }: Props) {
  useEffect(() => { document.title = 'Settings · Vocabularity'; }, []);

  return (
    <div className="page">
      <section className="page-heading">
        <span className="eyebrow"><SettingsIcon size={15} aria-hidden="true" /> MAKE IT YOURS</span>
        <h1>Settings<span>.</span></h1>
        <p>A few small details to make you feel at home.</p>
      </section>
      <section className="settings-card" aria-labelledby="appearance-heading">
        <div>
          <h2 id="appearance-heading">Appearance</h2>
          <p>Personalize how your workspace feels.</p>
        </div>
        <label className="preference">
          <span><strong>Reduce motion</strong><small>Turn off menu and hover animations.</small></span>
          <input type="checkbox" checked={reducedMotion}
            onChange={event => onReducedMotionChange(event.target.checked)} />
        </label>
      </section>
    </div>
  );
}