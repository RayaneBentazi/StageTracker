import { render, screen } from '@testing-library/react'
import { CarteStat } from './CarteStat'
import { Etat } from './Etat'
import { Historique } from './Historique'
import { StatutBadge } from './StatutBadge'
import { Statut } from '../types'

describe('StatutBadge', () => {
  it('affiche le libelle en francais du statut', () => {
    render(<StatutBadge statut={Statut.OffreRecue} />)
    expect(screen.getByText('Offre reçue')).toHaveClass('statut-3')
  })
})

describe('CarteStat', () => {
  it('affiche le titre, la valeur et le detail', () => {
    render(<CarteStat titre="Taux de réponse" valeur="50 %" detail="2 envoyées" />)
    expect(screen.getByText('Taux de réponse')).toBeInTheDocument()
    expect(screen.getByText('50 %')).toBeInTheDocument()
    expect(screen.getByText('2 envoyées')).toBeInTheDocument()
  })
})

describe('Etat', () => {
  it('affiche le chargement avant le contenu', () => {
    render(<Etat chargement erreur={null}><p>Contenu</p></Etat>)
    expect(screen.getByText('Chargement…')).toBeInTheDocument()
    expect(screen.queryByText('Contenu')).not.toBeInTheDocument()
  })

  it("affiche l'erreur", () => {
    render(<Etat chargement={false} erreur="Serveur injoignable"><p>Contenu</p></Etat>)
    expect(screen.getByRole('alert')).toHaveTextContent('Serveur injoignable')
  })

  it('affiche le contenu quand tout est charge', () => {
    render(<Etat chargement={false} erreur={null}><p>Contenu</p></Etat>)
    expect(screen.getByText('Contenu')).toBeInTheDocument()
  })
})

describe('Historique', () => {
  it('affiche un element par evenement', () => {
    render(
      <Historique evenements={[
        { statut: Statut.Reperee, date: '2026-10-01T12:00:00', commentaire: 'Candidature creee' },
        { statut: Statut.Envoyee, date: '2026-10-02T12:00:00', commentaire: null },
      ]} />,
    )
    expect(screen.getAllByRole('listitem')).toHaveLength(2)
    expect(screen.getByText('Candidature creee')).toBeInTheDocument()
  })
})
