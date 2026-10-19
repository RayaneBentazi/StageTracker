import { formaterDate } from '../format'
import type { Evenement } from '../types'
import { StatutBadge } from './StatutBadge'

// Ligne du temps des changements de statut d'une candidature
export function Historique({ evenements }: { evenements: Evenement[] }) {
  if (evenements.length === 0) return <p className="discret">Aucun événement.</p>

  return (
    <ol className="historique">
      {evenements.map((e, index) => (
        <li key={index}>
          <span className="historique-date">{formaterDate(e.date)}</span>
          <StatutBadge statut={e.statut} />
          {e.commentaire && <span className="historique-commentaire">{e.commentaire}</span>}
        </li>
      ))}
    </ol>
  )
}
