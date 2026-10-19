// Types qui correspondent aux DTO renvoyes par l'API ASP.NET Core.
// L'API envoie les enums sous forme de nombres.

export const Statut = {
  Reperee: 0,
  Envoyee: 1,
  Entrevue: 2,
  OffreRecue: 3,
  Refus: 4,
  Abandonnee: 5,
} as const
export type Statut = (typeof Statut)[keyof typeof Statut]

export const libellesStatut: Record<Statut, string> = {
  0: 'Repérée',
  1: 'Envoyée',
  2: 'Entrevue',
  3: 'Offre reçue',
  4: 'Refus',
  5: 'Abandonnée',
}

export const TypeEntrevue = {
  Telephonique: 0,
  Video: 1,
  EnPersonne: 2,
  Technique: 3,
} as const
export type TypeEntrevue = (typeof TypeEntrevue)[keyof typeof TypeEntrevue]

export const libellesTypeEntrevue: Record<TypeEntrevue, string> = {
  0: 'Téléphonique',
  1: 'Vidéo',
  2: 'En personne',
  3: 'Technique',
}

export interface Entreprise {
  id: number
  nom: string
  siteWeb: string | null
  ville: string | null
  notes: string | null
  nombreOffres: number
}

export interface Offre {
  id: number
  titre: string
  url: string | null
  source: string | null
  ville: string | null
  dateLimite: string | null
  dateDecouverte: string
  nomEntreprise: string
  statutCandidature: Statut | null
}

export interface Evenement {
  statut: Statut
  date: string
  commentaire: string | null
}

export interface Entrevue {
  id: number
  candidatureId: number
  date: string
  type: TypeEntrevue
  dureeMinutes: number | null
  interlocuteurs: string | null
  notes: string | null
}

export interface EntrevueAVenir {
  id: number
  candidatureId: number
  titreOffre: string
  nomEntreprise: string
  date: string
  type: TypeEntrevue
  interlocuteurs: string | null
}

export interface Candidature {
  id: number
  offreId: number
  titreOffre: string
  nomEntreprise: string
  statut: Statut
  dateCreation: string
  dateEnvoi: string | null
  dateDerniereRelance: string | null
  dateProchaineRelance: string | null
  notes: string | null
  evenements: Evenement[]
  entrevues: Entrevue[]
}

export interface Statistiques {
  totalEntreprises: number
  totalOffres: number
  totalCandidatures: number
  parStatut: Record<string, number>
  candidaturesEnvoyees: number
  tauxDeReponse: number
  aRelancer: number
  entrevuesAVenir: number
}
