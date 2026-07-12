# Ubiquitous Language

## Objetivo

Este documento define el lenguaje oficial del dominio de ArchiCore.

Todos los desarrolladores deberán utilizar exactamente estos términos en el código, documentación y conversaciones técnicas.

Nunca deberán utilizarse sinónimos para representar el mismo concepto.

---

# Cliente

Empresa o entidad que contrata los servicios de MULTISERTEC.

Un cliente puede tener múltiples contratos.

---

# Contrato

Acuerdo comercial firmado con un cliente.

Define:

- Fecha de inicio
- Fecha de finalización
- Valor económico
- Servicios contratados

Un contrato puede dividirse en múltiples proyectos.

---

# Proyecto

Unidad operativa mediante la cual se ejecuta parte de un contrato.

Cada proyecto pertenece a un único contrato.

Cada proyecto posee su propio equipo de trabajo.

---

# Coordinador

Responsable operativo de uno o varios proyectos.

Puede cambiar durante la vida del proyecto.

Autoriza préstamos documentales.

Aprueba procesos internos.

---

# Carpeta

Unidad documental utilizada para almacenar documentos relacionados.

Puede cambiar de caja durante su ciclo de vida.

---

# Caja

Contenedor físico que almacena carpetas.

Puede cambiar de bodega siguiendo el procedimiento correspondiente.

Cada caja posee un código de barras único.

---

# Documento

Unidad mínima de información almacenada por el sistema.

Puede contener anexos.

Los anexos siempre se consideran parte del documento principal.

---

# Anexo

Documento complementario asociado a otro documento.

Siempre se ubica al final del documento principal.

No existe de forma independiente.

---

# Bodega

Espacio físico destinado al almacenamiento de cajas.

Una bodega contiene múltiples cajas.

---

# Digitalización

Proceso mediante el cual un documento físico se convierte en un archivo digital.

Si ocurre un error durante el proceso, debe repetirse inmediatamente.

---

# Préstamo

Salida temporal autorizada de una carpeta para consulta.

Debe registrar:

- Solicitante
- Fecha
- Autorización
- Estado
- Fecha de devolución (cuando aplique)

---

# Código de Barras

Identificador único utilizado para localizar elementos físicos.

Actualmente se utiliza principalmente para cajas, pero el sistema permitirá reutilizar este concepto para otros elementos.

---

# Usuario

Persona autenticada dentro del sistema.

El acceso dependerá del rol asignado.

---

# Rol

Conjunto de permisos asignados a un usuario.

Los permisos determinarán las acciones permitidas dentro del ERP.

---

# Expediente

Actualmente el negocio no ha definido con precisión este concepto.

No será modelado hasta que existan reglas claras.