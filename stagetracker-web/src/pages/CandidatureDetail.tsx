import { useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api'
import { Etat } from '../components/Etat'
import { FormulaireEntrevue } from '../components/FormulaireEntrevue'
import { Historique } from '../components/Historique'
import { StatutBadge } from '../components/StatutBadge'
import { formaterDate, formaterDateHeure } from '../format'
import { useDonnees } from '../hooks/useDonnees'
import { libellesStatut, libellesTypeEntrevue, Statut } from '../types'

export function CandidatureDetail() {
  const id = Number(useParams().id)
  const { donnees: c, erreur, chargement, recharger } = useDonnees(() => api.candidature(id), [id])

  const [nouveauStatut, setNouveauStatut] = useState<Statut>(Statut.Envoyee)
  const [commentaire, setCommentaire] = useState('')
  const [erreurAction, setErreurAction] = useState<string | null>(null)

  // Toutes les actions suivent le meme modele : appel API, puis rechargement
  async function executer(action: () => Promise<unknown>) {
    setErreurAction(null)
    try {
      await action()
      await recharger()
    } catch (e) {
      setErreurAction(e instanceof Error ? e.message : 'Erreur inconnue')
    }
  }

  function changerStatut(e: FormEvent) {
    e.preventDefault()
    executer(async () => {
      await api.changerStatut(id, nouveauStatut, commentaire || undefined)
      setCommentaire('')
    })
  }

  return (
    <Etat chargement={chargement} erreur={erreur}>
      {c && (
        <>
          <Link to="/candidatures" className="retour">← Candidatures</Link>
          <div className="titre-page">
            <h1>{c.nomEntreprise} — {c.titreOffre}</h1>
            <StatutBadge statut={c.statut} />
          </div>

          <dl className="details">
            <dt>Créée le</dt><dd>{formaterDate(c.dateCreation)}</dd>
            <dt>Envoyée le</dt><dd>{formaterDate(c.dateEnvoi)}</dd>
            <dt>Dernière relance</dt><dd>{formaterDate(c.dateDerniereRelance)}</dd>
            <dt>Prochaine relance</dt><dd>{formaterDate(c.dateProchaineRelance)}</dd>
            {c.notes && (<><dt>Notes</dt><dd>{c.notes}</dd></>)}
          </dl>

          {erreurAction && <p className="erreur" role="alert">{erreurAction}</p>}

          <section>
            <h2>Changer le statut</h2>
            <form className="formulaire en-ligne" onSubmit={changerStatut}>
              <select
                aria-label="Nouveau statut"
                value={nouveauStatut}
                onChange={(e) => setNouveauStatut(Number(e.target.value) as Statut)}
              >
                {Object.entries(libellesStatut).map(([valeur, libelle]) => (
                  <option key={valeur} value={valeur}>{libelle}</option>
                ))}
              </select>
              <input placeholder="Commentaire (optionnel)" value={commentaire}
                onChange={(e) => setCommentaire(e.target.value)} />
              <button type="submit">Enregistrer</button>
              {c.statut === Statut.Envoyee && (
                <button type="button" className="secondaire" onClick={() => executer(() => api.relancer(id))}>
                  J’ai relancé
                </button>
              )}
            </form>
          </section>

          <section>
            <h2>Historique</h2>
            <Historique evenements={c.evenements} />
          </section>

          <section>
            <h2>Entrevues</h2>
            {c.entrevues.length === 0 ? (
              <p className="discret">Aucune entrevue pour l’instant.</p>
            ) : (
              <ul className="liste">
                {c.entrevues.map((e) => (
                  <li key={e.id}>
                    <span>{formaterDateHeure(e.date)}</span>
                    <span>{libellesTypeEntrevue[e.type]}{e.dureeMinutes ? ` · ${e.dureeMinutes} min` : ''}</span>
                    {e.interlocuteurs && <span className="discret">{e.interlocuteurs}</span>}
                  </li>
                ))}
              </ul>
            )}
            <FormulaireEntrevue onAjouter={(entrevue) => executer(() => api.ajouterEntrevue(id, entrevue))} />
          </section>
        </>
      )}
    </Etat>
  )
}
