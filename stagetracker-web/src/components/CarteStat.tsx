interface Props {
  titre: string
  valeur: string | number
  detail?: string
  alerte?: boolean
}

// Une tuile du tableau de bord (ex. "Taux de reponse : 50 %")
export function CarteStat({ titre, valeur, detail, alerte = false }: Props) {
  return (
    <div className={`carte-stat${alerte ? ' alerte' : ''}`}>
      <span className="carte-stat-titre">{titre}</span>
      <span className="carte-stat-valeur">{valeur}</span>
      {detail && <span className="carte-stat-detail">{detail}</span>}
    </div>
  )
}
