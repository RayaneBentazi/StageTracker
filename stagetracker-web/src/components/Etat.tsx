import type { ReactNode } from 'react'

interface Props {
  chargement: boolean
  erreur: string | null
  children: ReactNode
}

// Affiche "Chargement..." ou l'erreur, sinon le contenu.
// Evite de repeter les memes if dans chaque page.
export function Etat({ chargement, erreur, children }: Props) {
  if (chargement) return <p className="discret">Chargement…</p>
  if (erreur) return <p className="erreur" role="alert">{erreur}</p>
  return <>{children}</>
}
