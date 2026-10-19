// Formatage des dates en francais (Quebec)

export function formaterDate(iso: string | null | undefined): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleDateString('fr-CA', { day: 'numeric', month: 'long', year: 'numeric' })
}

export function formaterDateHeure(iso: string | null | undefined): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleString('fr-CA', {
    day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit',
  })
}
