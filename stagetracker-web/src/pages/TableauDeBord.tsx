import { Link } from 'react-router-dom'
import { api } from '../api'
import { CarteStat } from '../components/CarteStat'
import { Etat } from '../components/Etat'
import { StatutBadge } from '../components/StatutBadge'
import { formaterDate, formaterDateHeure } from '../format'
import { useDonnees } from '../hooks/useDonnees'
import { libellesTypeEntrevue } from '../types'

export function TableauDeBord() {
  // Les quatre appels partent en meme temps grace a Promise.all
  const { donnees, erreur, chargement, recharger } = useDonnees(async () => {
    const [stats, aRelancer, entrevues, echeances] = await Promise.all([
      api.statistiques(),
      api.aRelancer(),
      api.entrevuesAVenir(),
      api.echeances(7),
    ])
    return { stats, aRelancer, entrevues, echeances }
  })

  async function relancer(id: number) {
    await api.relancer(id)
    await recharger()
  }

  return (
    <Etat chargement={chargement} erreur={erreur}>
      {donnees && (
        <>
          <h1>Tableau de bord</h1>

          <section className="grille-stats">
            <CarteStat titre="Candidatures" valeur={donnees.stats.totalCandidatures}
              detail={`${donnees.stats.candidaturesEnvoyees} envoyées`} />
            <CarteStat titre="Taux de réponse" valeur={`${donnees.stats.tauxDeReponse} %`} />
            <CarteStat titre="Entrevues à venir" valeur={donnees.stats.entrevuesAVenir} />
            <CarteStat titre="À relancer" valeur={donnees.stats.aRelancer} alerte={donnees.stats.aRelancer > 0} />
          </section>

          <section>
            <h2>À relancer</h2>
            {donnees.aRelancer.length === 0 ? (
              <p className="discret">Aucune relance en retard.</p>
            ) : (
              <ul className="liste">
                {donnees.aRelancer.map((c) => (
                  <li key={c.id}>
                    <Link to={`/candidatures/${c.id}`}>{c.nomEntreprise} — {c.titreOffre}</Link>
                    <span className="discret">prévue le {formaterDate(c.dateProchaineRelance)}</span>
                    <button onClick={() => relancer(c.id)}>J’ai relancé</button>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section>
            <h2>Prochaines entrevues</h2>
            {donnees.entrevues.length === 0 ? (
              <p className="discret">Aucune entrevue prévue.</p>
            ) : (
              <ul className="liste">
                {donnees.entrevues.map((e) => (
                  <li key={e.id}>
                    <Link to={`/candidatures/${e.candidatureId}`}>{e.nomEntreprise} — {e.titreOffre}</Link>
                    <span>{formaterDateHeure(e.date)} · {libellesTypeEntrevue[e.type]}</span>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section>
            <h2>Dates limites cette semaine</h2>
            {donnees.echeances.length === 0 ? (
              <p className="discret">Rien qui ferme dans les 7 prochains jours.</p>
            ) : (
              <ul className="liste">
                {donnees.echeances.map((o) => (
                  <li key={o.id}>
                    <span>{o.nomEntreprise} — {o.titre}</span>
                    <span className="discret">ferme le {formaterDate(o.dateLimite)}</span>
                    {o.statutCandidature !== null && <StatutBadge statut={o.statutCandidature} />}
                  </li>
                ))}
              </ul>
            )}
          </section>
        </>
      )}
    </Etat>
  )
}
