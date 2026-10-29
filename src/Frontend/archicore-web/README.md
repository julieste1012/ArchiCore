# ArchiCore Web

Primera base funcional del frontend de ArchiCore: React + TypeScript + Vite.

## Requisitos

- Node.js 22.12 o superior.
- ASP.NET Core API ejecutándose en `https://localhost:7197`.
- PostgreSQL disponible y la migración de autenticación aplicada.
- Un usuario SystemAdministrator inicializado mediante las variables de entorno de la API.

## Inicio local

Desde esta carpeta:

```powershell
npm install
npm run dev
```

Abre http://127.0.0.1:5173. Vite hace proxy de `/api` y `/health` a la API local. No se configura ninguna clave secreta del backend dentro del frontend.

## Alcance actual

- Login contra `POST /api/auth/login`.
- Renovación de la sesión mediante `POST /api/auth/refresh`.
- Cierre de sesión contra `POST /api/auth/logout`.
- Panel con conteos reales de clientes, contratos, proyectos y bodegas.
- Listados y alta inicial de esas cuatro entidades.
- La API limita temporalmente estos módulos al rol SystemAdministrator hasta que se apruebe la matriz de permisos definitiva.

La sesión y los tokens se mantienen solo en memoria del navegador. Una recarga exige volver a iniciar sesión; este comportamiento evita guardar refresh tokens en localStorage/sessionStorage. Para producción se debe evaluar una estrategia con refresh token en cookie HttpOnly, Secure y SameSite junto con protección CSRF.

## Comandos

```powershell
npm run typecheck
npm run build
```
