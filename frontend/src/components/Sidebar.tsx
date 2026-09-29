import { BookOpen, House, Menu, Settings, ArrowUpRight } from 'lucide-react';
import { NavLink } from 'react-router-dom';

type Props = { collapsed: boolean; onToggle: () => void };

export default function Sidebar({ collapsed, onToggle }: Props) {
  return (
    <aside className="sidebar" aria-label="Application sidebar">
      <div className="sidebar-top">
        <button
          className="menu-toggle"
          onClick={onToggle}
          aria-label={collapsed ? 'Expand menu' : 'Collapse menu'}
          aria-expanded={!collapsed}
          aria-controls="main-navigation"
          title={collapsed ? 'Expand menu' : 'Collapse menu'}
        >
          <Menu size={22} strokeWidth={1.7} aria-hidden="true" />
        </button>
        <NavLink to="/" className="brand" aria-label="Vocabularity home">
          <img className="brand-mark" src="/vocabularity.svg" alt="" width="58" height="58" />
          <span className="brand-name">Vocabularity<span className="brand-dot">.</span></span>
          <span className="brand-caption">A home for your words</span>
        </NavLink>
      </div>

      <nav id="main-navigation" className="sidebar-navigation" aria-label="Main navigation">
        <NavLink to="/" end className="navigation-link" title="Home">
          <House size={22} strokeWidth={1.7} aria-hidden="true" />
          <span className="navigation-label">Home</span>
        </NavLink>
        <NavLink to="/dictionary" className="navigation-link" title="Library">
          <BookOpen size={22} strokeWidth={1.7} aria-hidden="true" />
          <span className="navigation-label">Library</span>
          <ArrowUpRight className="navigation-arrow" size={16} aria-hidden="true" />
        </NavLink>
        <div className="sidebar-bottom">
          <span className="sidebar-note">Little by little.<br />Word by word.</span>
          <NavLink to="/settings" className="navigation-link settings-link" title="Settings">
            <Settings size={22} strokeWidth={1.7} aria-hidden="true" />
            <span className="navigation-label">Settings</span>
          </NavLink>
        </div>
      </nav>
    </aside>
  );
}
