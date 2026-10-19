import { NavLink, Outlet } from 'react-router-dom'

// Structure commune a toutes les pages : barre de navigation + contenu.
// <Outlet /> affiche la page qui correspond a l'URL.
export function Layout() {
  return (
    <div className="app">
      <header className="entete">
        <span className="logo">StageTracker</span>
        <nav>
          <NavLink to="/" end>Tableau de bord</NavLink>
          <NavLink to="/candidatures">Candidatures</NavLink>
          <NavLink to="/offres">Offres</NavLink>
          <NavLink to="/entreprises">Entreprises</NavLink>
        </nav>
      </header>
      <main className="contenu">
        <Outlet />
      </main>
    </div>
  )
}
