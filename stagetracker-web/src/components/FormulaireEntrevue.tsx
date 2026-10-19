import { useState, type FormEvent } from 'react'
import { libellesTypeEntrevue, TypeEntrevue } from '../types'

interface Props {
  onAjouter: (entrevue: {
    date: string; type: TypeEntrevue; dureeMinutes?: number; interlocuteurs?: string
  }) => Promise<void>
}

// Formulaire controle : chaque champ est relie a un state React
export function FormulaireEntrevue({ onAjouter }: Props) {
  const [date, setDate] = useState('')
  const [type, setType] = useState<TypeEntrevue>(TypeEntrevue.Video)
  const [duree, setDuree] = useState('60')
  const [interlocuteurs, setInterlocuteurs] = useState('')
  const [envoi, setEnvoi] = useState(false)

  async function soumettre(e: FormEvent) {
    e.preventDefault()
    setEnvoi(true)
    try {
      await onAjouter({
        date,
        type,
        dureeMinutes: duree ? Number(duree) : undefined,
        interlocuteurs: interlocuteurs || undefined,
      })
      setDate('')
      setInterlocuteurs('')
    } finally {
      setEnvoi(false)
    }
  }

  return (
    <form className="formulaire" onSubmit={soumettre}>
      <label>
        Date et heure
        <input type="datetime-local" required value={date} onChange={(e) => setDate(e.target.value)} />
      </label>
      <label>
        Type
        <select value={type} onChange={(e) => setType(Number(e.target.value) as TypeEntrevue)}>
          {Object.entries(libellesTypeEntrevue).map(([valeur, libelle]) => (
            <option key={valeur} value={valeur}>{libelle}</option>
          ))}
        </select>
      </label>
      <label>
        Durée (minutes)
        <input type="number" min="5" value={duree} onChange={(e) => setDuree(e.target.value)} />
      </label>
      <label>
        Interlocuteurs
        <input value={interlocuteurs} onChange={(e) => setInterlocuteurs(e.target.value)} />
      </label>
      <button type="submit" disabled={envoi}>{envoi ? 'Ajout…' : 'Ajouter l’entrevue'}</button>
    </form>
  )
}
