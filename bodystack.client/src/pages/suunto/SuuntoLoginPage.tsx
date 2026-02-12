// import { useMemo, useState } from 'react'
// import { useLocation, useNavigate } from 'react-router-dom'
// import Card from '../../components/ui/Card'

// const SUUNTO_AUTH_STORAGE_KEY = 'suunto.sstAuthorization'

// type LocationState = {
//   from?: string
// }

// export default function SuuntoLoginPage() {
//   const navigate = useNavigate()
//   const location = useLocation()

//   const from = useMemo(() => {
//     const state = location.state as LocationState | null
//     return state?.from ?? '/suunto'
//   }, [location.state])

//   const [value, setValue] = useState(() => localStorage.getItem(SUUNTO_AUTH_STORAGE_KEY) ?? '')
//   const [error, setError] = useState<string | null>(null)

//   const onSubmit = (e: React.FormEvent) => {
//     e.preventDefault()
//     setError(null)

//     const trimmed = value.trim()
//     if (!trimmed) {
//       setError('SSTAuthorization is required')
//       return
//     }

//     localStorage.setItem(SUUNTO_AUTH_STORAGE_KEY, trimmed)
//     navigate(from, { replace: true })
//   }

//   const onClear = () => {
//     localStorage.removeItem(SUUNTO_AUTH_STORAGE_KEY)
//     setValue('')
//   }

//   return (
//     <div className="mx-auto w-full max-w-lg space-y-6">
//       <div>
//         <h2 className="text-xl font-semibold">Suunto</h2>
//         <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">Provide SSTAuthorization key to continue.</p>
//       </div>

//       <Card title="Connect Suunto">
//         <form onSubmit={onSubmit} className="space-y-4">
//           <label className="block">
//             <div className="text-sm font-medium text-slate-700 dark:text-slate-200">SSTAuthorization</div>
//             <input
//               value={value}
//               onChange={e => setValue(e.target.value)}
//               autoComplete="off"
//               spellCheck={false}
//               className="mt-2 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-100"
//             />
//           </label>

//           {error ? (
//             <div className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm text-rose-800 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-200">
//               {error}
//             </div>
//           ) : null}

//           <div className="grid gap-3 sm:grid-cols-2">
//             <button
//               type="submit"
//               className="rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
//             >
//               Save & continue
//             </button>
//             <button
//               type="button"
//               onClick={onClear}
//               className="rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 shadow-sm transition-colors hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800"
//             >
//               Clear
//             </button>
//           </div>
//         </form>
//       </Card>
//     </div>
//   )
// }
