import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { CandidatureDetail } from './pages/CandidatureDetail'
import { Candidatures } from './pages/Candidatures'
import { Entreprises } from './pages/Entreprises'
import { Offres } from './pages/Offres'
import { TableauDeBord } from './pages/TableauDeBord'
import './styles.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<TableauDeBord />} />
          <Route path="candidatures" element={<Candidatures />} />
          <Route path="candidatures/:id" element={<CandidatureDetail />} />
          <Route path="offres" element={<Offres />} />
          <Route path="entreprises" element={<Entreprises />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>,
)
