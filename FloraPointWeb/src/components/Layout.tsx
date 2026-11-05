import { NavLink, Outlet } from "react-router-dom";

const navItems = [{ to: "/", label: "Mapa" }];

function Layout() {
  return (
    <div className="flex min-h-svh flex-col">
      <header className="border-b border-slate-200 bg-white">
        <nav className="mx-auto flex max-w-5xl items-center gap-6 px-4 py-3">
          <span className="text-lg font-bold text-emerald-700">FloraPoint</span>
          <ul className="flex gap-4 text-sm font-medium text-slate-600">
            {navItems.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  className={({ isActive }) =>
                    isActive ? "text-emerald-700" : "hover:text-emerald-700"
                  }
                >
                  {item.label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>
      </header>
      <Outlet />
    </div>
  );
}

export default Layout;
