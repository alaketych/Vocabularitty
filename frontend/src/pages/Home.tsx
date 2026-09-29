import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Sprout } from 'lucide-react';

export default function Home() {
  useEffect(() => { document.title = 'Home · Vocabularity'; }, []);

  return (
    <div className="page">
      <section className="page-heading">
        <span className="eyebrow"><Sprout size={15} aria-hidden="true" /> A HOME FOR YOUR WORDS</span>
        <h1>Welcome to Vocabularity<span>.</span></h1>
        <p>Collect new words, build your dictionaries, and keep learning.</p>
      </section>
      <Link to="/dictionary" className="primary-button home-library-link">Open your library</Link>
    </div>
  );
}
