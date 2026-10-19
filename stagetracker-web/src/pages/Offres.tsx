import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { Etat } from '../components/Etat'
import { StatutBadge } from '../components/StatutBadge'
import { formaterDate } from '../format'
import { useDonnees } from '../hooks/useDonnees'

export function Offres() {
  const navigate = useNavigate()
  const { donnees, erreur, chargement, recharger } = useDonnees(async () => {
    const [offres, entreprises] = await Promise.all([api.offres(), api.entreprises()])
    return { offres, entreprises }
  })

  const [entrepriseId, setEntrepriseId] = useState('')
  const [titre, setTitre] = useState('')
  const [url, setUrl] = useState('')
  const [dateLimite, setDateLimite] = useState('')
  const [erreurAction, setErreurAction] = useState<string | null>(null)

  async function ajouter(e: FormEvent) {
    e.preventDefault()
    setErreurAction(null)
    try {
      await api.creerOffre({
        entrepriseId: Number(entrepriseId),
        titre,
        url: url || undefined,
        dateLimite: dateLimite || undefined,
      })
      setTitre('')
      setUrl('')
      setDateLimite('')
      await recharger()
    } catch (err) {
      // Ex. 409 : "Cette offre existe deja."
      setErreurAction(err instanceof Error ? err.message : 'Erreur inconnue')
    }
  }

  async function postuler(offreId: number) {
    setErreurAction(null)
    try {
      const candidature = await api.creerCandidature(offreId)
      navigate(`/candidatures/${candidature.id}`)
    } catch (err) {
      setErreurAction(err instanceof Error ? err.message : 'Erreur inconnue')
    }
  }

  return (
    <Etat chargement={chargement} erreur={erreur}>
      {donnees && (
        <>
          <h1>Offres</h1>

          <form className="formulaire" onSubmit={ajouter}>
            <label>
              Entreprise
              <select required value={entrepriseId} onChange={(e) => setEntrepriseId(e.target.value)}>
                <option value="">Choisir…</option>
                {donnees.entreprises.map((en) => (
                  <option key={en.id} value={en.id}>{en.nom}</option>
                ))}
              </select>
            </label>
            <label>
              Titre du poste
              <input required value={titre} onChange={(e) => setTitre(e.target.value)} />
            </label>
            <label>
              Lien de l’offre
              <input type="url" value={url} onChange={(e) => setUrl(e.target.value)} />
            </label>
            <label>
              Date limite
              <input type="date" value={dateLimite} onChange={(e) => setDateLimite(e.target.value)} />
            </label>
            <button type="submit">Ajouter l’offre</button>
          </form>

          {erreurAction && <p className="erreur" role="alert">{erreurAction}</p>}

          {donnees.offres.length === 0 ? (
            <p className="discret">Aucune offre. Ajoute d’abord une entreprise, puis une offre.</p>
          ) : (
            <table className="tableau">
              <thead>
                <tr><th>Entreprise</th><th>Poste</th><th>Date limite</th><th>Candidature</th></tr>
              </thead>
              <tbody>
                {donnees.offres.map((o) => (
                  <tr key={o.id}>
                    <td>{o.nomEntreprise}</td>
                    <td>{o.url ? <a href={o.url} target="_blank" rel="noreferrer">{o.titre}</a> : o.titre}</td>
                    <td>{formaterDate(o.dateLimite)}</td>
                    <td>
                      {o.statutCandidature !== null
                        ? <StatutBadge statut={o.statutCandidature} />
                        : <button className="secondaire" onClick={() => postuler(o.id)}>Postuler</button>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </>
      )}
    </Etat>
  )
}
