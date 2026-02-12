import { Outlet } from 'react-router-dom'
import SidebarNav from './SidebarNav'
import TopBar from './TopBar'

export default function AppLayout() {
  return (
    <div className="min-h-full bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100">
      <div className="mx-auto flex min-h-full max-w-7xl">
        <aside className="hidden w-64 shrink-0 border-r border-slate-200 bg-white px-4 py-5 dark:border-slate-800 dark:bg-slate-950 md:block">
          <div className="mb-6">
            <div className="text-sm font-semibold">BodyStack</div>
            <div className="text-xs text-slate-500 dark:text-slate-400">Integrations</div>
          </div>
          <SidebarNav />
        </aside>

        <div className="flex min-w-0 flex-1 flex-col">
          <TopBar />
          <main className="flex-1 px-4 py-6 sm:px-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  )
}
