// import { useState } from 'react'
// import { useTranslation } from 'react-i18next'
// import { useNavigate } from 'react-router-dom'
// import { fitatuLogin } from '../api/fitatuApi'

// export default function LoginPage() {
//   const { t } = useTranslation()
//   const navigate = useNavigate()

//   const [username, setUsername] = useState('')
//   const [password, setPassword] = useState('')
//   const [isSubmitting, setIsSubmitting] = useState(false)
//   const [error, setError] = useState<string | null>(null)

//   const onSubmit = async (e: React.FormEvent) => {
//     e.preventDefault()
//     setError(null)
//     setIsSubmitting(true)

//     try {
//       await fitatuLogin({ username, password })
//       navigate('/fitatu', { replace: true })
//     } catch (err) {
//       setError(err instanceof Error ? err.message : 'Login failed')
//     } finally {
//       setIsSubmitting(false)
//     }
//   }

//   return (
//     <div className="min-h-full bg-slate-50 px-4 py-10 dark:bg-slate-950">
//       <div className="mx-auto w-full max-w-md">
//         <div className="mb-6 text-center">
//           <div className="text-sm font-semibold text-slate-900 dark:text-slate-100">BodyStack</div>
//           <div className="mt-1 text-sm text-slate-600 dark:text-slate-400">{t('login.title')}</div>
//         </div>

//         <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900">
//           <form onSubmit={onSubmit} className="space-y-4">
//             <label className="block">
//               <div className="text-sm font-medium text-slate-700 dark:text-slate-200">{t('login.username')}</div>
//               <input
//                 value={username}
//                 onChange={e => setUsername(e.target.value)}
//                 autoComplete="username"
//                 className="mt-2 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-100"
//               />
//             </label>

//             <label className="block">
//               <div className="text-sm font-medium text-slate-700 dark:text-slate-200">{t('login.password')}</div>
//               <input
//                 type="password"
//                 value={password}
//                 onChange={e => setPassword(e.target.value)}
//                 autoComplete="current-password"
//                 className="mt-2 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-100"
//               />
//             </label>

//             {error ? (
//               <div className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm text-rose-800 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-200">
//                 {error}
//               </div>
//             ) : null}

//             <button
//               type="submit"
//               disabled={isSubmitting}
//               className="w-full rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 disabled:opacity-60 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
//             >
//               {t('login.submit')}
//             </button>
//           </form>
//         </section>
//       </div>
//     </div>
//   )
// }
