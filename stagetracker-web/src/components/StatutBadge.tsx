import { libellesStatut, type Statut } from '../types'

// Pastille de couleur pour un statut. La couleur vient de la classe CSS
// (statut-0, statut-1, ...) definie dans styles.css.
export function StatutBadge({ statut }: { statut: Statut }) {
  return <span className={`badge statut-${statut}`}>{libellesStatut[statut]}</span>
}
