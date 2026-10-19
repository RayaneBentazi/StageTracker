import { useCallback, useEffect, useState } from 'react'

// Petit hook reutilisable pour charger des donnees de l'API.
// Il gere les trois etats d'un appel (chargement, erreur, donnees)
// et donne une fonction recharger() apres une modification.
export function useDonnees<T>(charger: () => Promise<T>, dependances: unknown[] = []) {
  const [donnees, setDonnees] = useState<T | null>(null)
  const [erreur, setErreur] = useState<string | null>(null)
  const [chargement, setChargement] = useState(true)

  const chargerMemo = useCallback(charger, dependances)

  // Pas de remise a true de "chargement" ici : apres une modification,
  // on garde l'ancien contenu affiche pendant qu'on recharge (pas de clignotement).
  const recharger = useCallback(async () => {
    setErreur(null)
    try {
      setDonnees(await chargerMemo())
    } catch (e) {
      setErreur(e instanceof Error ? e.message : 'Erreur inconnue')
    } finally {
      setChargement(false)
    }
  }, [chargerMemo])

  useEffect(() => {
    recharger()
  }, [recharger])

  return { donnees, erreur, chargement, recharger }
}
