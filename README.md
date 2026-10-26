# 🍽️ Restaurante App · Sistema de gestión

> **EN:** A desktop management system for a restaurant, built with C# (.NET 10, WinForms) and SQL Server using a 3-layer architecture (UI, BLL, DAL). It has role-based login, customer, table, dish and order management, reports with charts, Excel and PDF export, and CSV import.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Estado](https://img.shields.io/badge/estado-proyecto_acad%C3%A9mico-blue)

## Qué es

Una aplicación de escritorio para manejar el día a día de un restaurante: clientes, mesas, menú, pedidos y reportes de ventas.

Es un proyecto académico. Mi objetivo fue aplicar una **arquitectura en capas** con acceso a datos usando **ADO.NET**, sin un ORM, para entender qué pasa entre la aplicación y la base de datos.

## Funcionalidades

**Inicio de sesión con perfiles:**
- las contraseñas se guardan como hash SHA-256
- el menú se adapta al perfil (por ejemplo, la gestión de usuarios solo la ve el Administrador)

**Gestión (CRUD)** de clientes, mesas, platos del menú y usuarios.

**Pedidos:** cada pedido se asocia a una mesa, a un cliente y al usuario que lo registra, con su detalle de platos.

**Reportes**, en pestañas:
- platos por categoría, con exportación a Excel y PDF
- ventas por categoría, con un gráfico dibujado a mano y exportación a Excel
- ventas por rango de fechas, con total general y exportación a PDF

**Importación de platos desde CSV,** con vista previa antes de confirmar. El archivo `platos_prueba.csv` sirve de ejemplo.

**Consultas parametrizadas** en todos los repositorios, para prevenir inyección SQL.

**Cadena de conexión en `App.config`,** nunca escrita directamente en el código.

## Arquitectura

```
App.UI   → Formularios WinForms (login, MDI, clientes, mesas, platos, pedidos, usuarios, reportes)
   │
App.BLL  → Servicios con las reglas de negocio, sesión, encriptación y exportación (Excel y PDF)
   │
App.DAL  → Conexión, modelos y repositorios con ADO.NET (Microsoft.Data.SqlClient)
   │
SQL Server
```

Cada capa solo habla con la que tiene debajo. La interfaz nunca toca la base de datos directamente.

## Tecnologías

| Área | Herramientas |
|---|---|
| Lenguaje y plataforma | C#, .NET 10, Windows Forms |
| Base de datos | SQL Server Express, con ADO.NET (`Microsoft.Data.SqlClient`) |
| Exportación | ClosedXML (Excel) y QuestPDF (PDF) |
| Seguridad | Hash SHA-256 para contraseñas y consultas parametrizadas |

## 📸 Capturas de pantalla

<img width="1366" height="768" alt="Pantalla principal" src="https://github.com/user-attachments/assets/9bfd0cae-120f-4f3b-b990-0317804fd11a" />
<img width="1366" height="768" alt="Gestión" src="https://github.com/user-attachments/assets/39133930-0615-4882-88f0-2aaa290b7c84" />
<img width="1366" height="768" alt="Reportes" src="https://github.com/user-attachments/assets/0576ab1f-3d22-4008-b328-a5dd76373daf" />
<img width="357" height="268" alt="Inicio de sesión" src="https://github.com/user-attachments/assets/0ef4b224-680c-4d5a-a943-75f5f5d1fd3f" />

## 🚀 Cómo ejecutarlo

**Requisitos:** Windows, el .NET 10 SDK, SQL Server Express y Visual Studio 2022 o superior.

1. Clona el repositorio.
2. Crea la base de datos `RestauranteDB` en tu instancia `localhost\SQLEXPRESS`.
3. Si tu servidor tiene otro nombre, ajusta la cadena de conexión en `App.UI/App.config`.
4. Abre `RestauranteApp.slnx` en Visual Studio y ejecuta el proyecto `App.UI`.

## Lo que aprendí

- A separar responsabilidades en capas y a organizar el acceso a datos con el patrón repositorio.
- A trabajar con ADO.NET a bajo nivel: conexiones, comandos y parámetros.
- A generar archivos Excel y PDF desde .NET.
- A manejar sesión y permisos según el perfil del usuario.

## Mejoras pendientes

- [ ] Agregar el script SQL para crear la base de datos y cargar datos de prueba.
- [ ] Cambiar SHA-256 por un hash con sal pensado para contraseñas (BCrypt o PBKDF2).
- [ ] Agregar pruebas unitarias a la capa BLL.

## 👨‍💻 Autor

Desarrollado por **Carlos Andrés Wilches Guerra** como proyecto académico · [GitHub](https://github.com/carlithoswilches) · candreswilches@gmail.com
