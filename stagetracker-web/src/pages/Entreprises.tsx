import { useState, type FormEvent } from 'react'
import { api } from '../api'
import { Etat } from '../components/Etat'
import { useDonnees } from '../hooks/useDonnees'

export function Entreprises() {
  const { donnees, erreur, chargement, recharger } = useDonnees(api.entreprises)
  const [nom, setNom] = useState('')
  const [ville, setVille] = useState('')
  const [siteWeb, setSiteWeb] = useState('')

  async function ajouter(e: FormEvent) {
    e.preventDefault()
    await api.creerEntreprise({ nom, ville: ville || undefined, siteWeb: siteWeb || undefined })
    setNom('')
    setVille('')
    setSiteWeb('')
    await recharger()
  }

  return (
    <>
      <h1>Entreprises</h1>

      <form className="formulaire" onSubmit={ajouter}>
        <label>
          Nom
          <input required value={nom} onChange={(e) => setNom(e.target.value)} />
        </label>
        <label>
          Ville
          <input value={ville} onChange={(e) => setVille(e.target.value)} />
        </label>
        <label>
          Site web
          <input type="url" value={siteWeb} onChange={(e) => setSiteWeb(e.target.value)} />
        </label>
        <button type="submit">Ajouter</button>
      </form>

      <Etat chargement={chargement} erreur={erreur}>
        <table className="tableau">
          <thead>
            <tr><th>Nom</th><th>Ville</th><th>Offres</th></tr>
          </thead>
          <tbody>
            {(donnees ?? []).map((en) => (
              <tr key={en.id}>
                <td>{en.siteWeb ? <a href={en.siteWeb} target="_blank" rel="noreferrer">{en.nom}</a> : en.nom}</td>
                <td>{en.ville ?? '—'}</td>
                <td>{en.nombreOffres}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Etat>
    </>
  )
}
