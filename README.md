# 🍽️ Restaurante App - Sistema de Gestión

Sistema de escritorio para la gestión integral de un restaurante, desarrollado en **C# (.NET) con WinForms**, aplicando arquitectura en capas para mantener el código organizado, escalable y fácil de mantener.

## 📋 Funcionalidades

- **Autenticación de usuarios** con inicio de sesión seguro
- **Gestión de clientes** (registro, edición, consulta)
- **Gestión de mesas** del restaurante
- **Gestión de platos/menú** con categorías y precios
- **Gestión de pedidos** y detalle de pedidos
- **Reportes** con exportación de datos
- **Encriptación** de información sensible

## 🛠️ Tecnologías utilizadas

- **C# / .NET**
- **WinForms** (interfaz de usuario)
- **SQL Server** (base de datos)
- **Arquitectura en 3 capas:**
  - `App.UI` → Interfaz de usuario (formularios)
  - `App.BLL` → Lógica de negocio (servicios)
  - `App.DAL` → Acceso a datos (repositorios y conexión)

## 🏗️ Arquitectura

El proyecto sigue el patrón de **arquitectura en capas**, separando responsabilidades:
Esto permite que cada capa tenga una única responsabilidad, facilitando el mantenimiento y las pruebas del sistema.

## 📸 Capturas de pantalla

<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/9bfd0cae-120f-4f3b-b990-0317804fd11a" />
<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/39133930-0615-4882-88f0-2aaa290b7c84" />
<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/0576ab1f-3d22-4008-b328-a5dd76373daf" />
<img width="357" height="268" alt="image" src="https://github.com/user-attachments/assets/0ef4b224-680c-4d5a-a943-75f5f5d1fd3f" />



## 🚀 Cómo ejecutarlo

1. Clonar el repositorio
2. Abrir `RestauranteApp.slnx` con Visual Studio
3. Configurar la cadena de conexión en `App.Config` con tu servidor SQL Server local
4. Ejecutar el proyecto `App.UI`

## 👨‍💻 Autor

Desarrollado por [Carlos Wilches](https://github.com/carlithoswilches) como proyecto de aprendizaje en desarrollo de software.
