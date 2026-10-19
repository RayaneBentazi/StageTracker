import type {
  Candidature, Entreprise, EntrevueAVenir, Offre, Statistiques, Statut, TypeEntrevue,
} from './types'

// Tous les appels a l'API passent par ici. Les URL sont relatives (/api/...)
// parce que Vite redirige /api vers l'API en developpement (voir vite.config.ts).

export class ErreurApi extends Error {
  readonly statut: number

  constructor(statut: number, message: string) {
    super(message)
    this.statut = statut
  }
}

async function requete<T>(url: string, options: RequestInit = {}): Promise<T> {
  const reponse = await fetch(url, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options.headers },
  })

  if (!reponse.ok) {
    // L'API renvoie un message en texte pour les 400 et 409
    const texte = await reponse.text()
    throw new ErreurApi(reponse.status, texte || `Erreur ${reponse.status}`)
  }

  // 204 No Content : rien a lire
  if (reponse.status === 204) return undefined as T
  return reponse.json() as Promise<T>
}

const envoyer = <T>(url: string, methode: string, corps?: unknown) =>
  requete<T>(url, { method: methode, body: corps === undefined ? undefined : JSON.stringify(corps) })

export const api = {
  statistiques: () => requete<Statistiques>('/api/statistiques'),

  entreprises: () => requete<Entreprise[]>('/api/entreprises'),
  creerEntreprise: (e: { nom: string; ville?: string; siteWeb?: string; notes?: string }) =>
    envoyer<Entreprise>('/api/entreprises', 'POST', e),

  offres: () => requete<Offre[]>('/api/offres'),
  echeances: (jours = 7) => requete<Offre[]>(`/api/offres/echeances?jours=${jours}`),
  creerOffre: (o: {
    entrepriseId: number; titre: string; url?: string; source?: string; ville?: string; dateLimite?: string
  }) => envoyer<Offre>('/api/offres', 'POST', o),

  candidatures: () => requete<Candidature[]>('/api/candidatures'),
  candidature: (id: number) => requete<Candidature>(`/api/candidatures/${id}`),
  aRelancer: () => requete<Candidature[]>('/api/candidatures/a-relancer'),
  creerCandidature: (offreId: number, notes?: string) =>
    envoyer<Candidature>('/api/candidatures', 'POST', { offreId, notes }),
  changerStatut: (id: number, nouveauStatut: Statut, commentaire?: string) =>
    envoyer<Candidature>(`/api/candidatures/${id}/statut`, 'POST', { nouveauStatut, commentaire }),
  relancer: (id: number) => envoyer<Candidature>(`/api/candidatures/${id}/relance`, 'POST'),

  entrevuesAVenir: () => requete<EntrevueAVenir[]>('/api/entrevues/a-venir'),
  ajouterEntrevue: (candidatureId: number, e: {
    date: string; type: TypeEntrevue; dureeMinutes?: number; interlocuteurs?: string; notes?: string
  }) => envoyer(`/api/candidatures/${candidatureId}/entrevues`, 'POST', e),
}
