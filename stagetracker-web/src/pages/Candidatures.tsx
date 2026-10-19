import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { Etat } from '../components/Etat'
import { StatutBadge } from '../components/StatutBadge'
import { formaterDate } from '../format'
import { useDonnees } from '../hooks/useDonnees'
import { libellesStatut, type Statut } from '../types'

export function Candidatures() {
  const { donnees, erreur, chargement } = useDonnees(api.candidatures)
  // "tous" ou un numero de statut. Le filtre se fait cote client.
  const [filtre, setFiltre] = useState<'tous' | Statut>('tous')

  const visibles = (donnees ?? []).filter((c) => filtre === 'tous' || c.statut === filtre)

  return (
    <>
      <div className="titre-page">
        <h1>Candidatures</h1>
        <select
          aria-label="Filtrer par statut"
          value={filtre}
          onChange={(e) => setFiltre(e.target.value === 'tous' ? 'tous' : (Number(e.target.value) as Statut))}
        >
          <option value="tous">Tous les statuts</option>
          {Object.entries(libellesStatut).map(([valeur, libelle]) => (
            <option key={valeur} value={valeur}>{libelle}</option>
          ))}
        </select>
      </div>

      <Etat chargement={chargement} erreur={erreur}>
        {visibles.length === 0 ? (
          <p className="discret">Aucune candidature.</p>
        ) : (
          <table className="tableau">
            <thead>
              <tr>
                <th>Entreprise</th>
                <th>Poste</th>
                <th>Statut</th>
                <th>Envoyée le</th>
                <th>Prochaine relance</th>
              </tr>
            </thead>
            <tbody>
              {visibles.map((c) => (
                <tr key={c.id}>
                  <td>{c.nomEntreprise}</td>
                  <td><Link to={`/candidatures/${c.id}`}>{c.titreOffre}</Link></td>
                  <td><StatutBadge statut={c.statut} /></td>
                  <td>{formaterDate(c.dateEnvoi)}</td>
                  <td>{formaterDate(c.dateProchaineRelance)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Etat>
    </>
  )
}
