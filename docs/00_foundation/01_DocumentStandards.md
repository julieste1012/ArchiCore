# Document Standards

| Campo | Valor |
|--------|-------|
| Documento | 01_DocumentStandards |
| Versión | 1.0.0 |
| Estado | Approved |
| Sprint | Sprint 0 |
| Última actualización | 2026-07-11 |

---

# Propósito

Este documento define el estándar oficial para la creación y mantenimiento de toda la documentación del proyecto.

Su cumplimiento es obligatorio para cualquier documento almacenado en el repositorio.

---

# Idioma

La documentación técnica se escribirá en inglés.

Las reglas de negocio, términos archivísticos colombianos y conceptos propios del dominio conservarán su nombre oficial en español.

Ejemplos:

- Serie Documental
- Subserie Documental
- Expediente
- Carpeta
- Folio

No deberán traducirse.

---

# Estructura obligatoria

Todo documento deberá seguir el siguiente orden.

1. Encabezado
2. Propósito
3. Alcance
4. Contenido principal
5. Referencias (si aplica)
6. Historial de versiones

---

# Encabezado

Todo documento deberá comenzar con una tabla como la siguiente.

| Campo | Valor |
|--------|-------|
| Documento | Nombre del documento |
| Versión | x.y.z |
| Estado | Draft / Review / Approved / Deprecated |
| Sprint | Sprint correspondiente |
| Última actualización | YYYY-MM-DD |

---

# Versionado

Se utilizará Semantic Versioning.

| Versión | Significado |
|----------|-------------|
| 0.x.x | Documento en construcción |
| 1.0.0 | Primera versión aprobada |
| 1.x.x | Mejoras compatibles |
| 2.0.0 | Cambios importantes |

---

# Estados permitidos

Solo existen cuatro estados.

- Draft
- Review
- Approved
- Deprecated

No deberán utilizarse otros estados.

---

# Títulos

Todos los títulos utilizarán Markdown.

Ejemplo:

# Título principal

## Sección

### Subsección

No deberán utilizarse tamaños manuales de fuente.

---

# Tablas

Las tablas se utilizarán para:

- metadatos
- comparaciones
- listados
- matrices

Evitar tablas para texto extenso.

---

# Diagramas

Todo diagrama deberá almacenarse en:

assets/diagrams/

No se incrustarán imágenes dentro de la documentación si pueden mantenerse como archivos independientes.

---

# Referencias cruzadas

Cuando un documento dependa de otro deberá mencionarlo explícitamente.

Ejemplo.

Depende de:

- Master Blueprint
- Business Rules

---

# Nombres de archivos

Todos los nombres utilizarán PascalCase.

Ejemplos.

Good:

BusinessRules.md

AuthenticationStrategy.md

DatabaseDesign.md

Bad:

business rules.md

Documento final.md

archivo1.md

---

# Escritura

Las reglas son:

- frases claras
- lenguaje técnico
- evitar ambigüedad
- evitar opiniones personales
- evitar información duplicada

---

# Historial de versiones

Todo documento finalizará con una tabla de cambios.

| Versión | Fecha | Descripción |
|----------|--------|-------------|

---

# Regla final

La documentación forma parte del software.

Actualizar código sin actualizar la documentación correspondiente se considera una tarea incompleta.