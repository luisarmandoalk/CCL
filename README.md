# CCL
Aplicacion inventario CCL

Descripción del proyecto

La solución CCL es una aplicación web básica para gestionar el inventario de productos de la empresa. 
Funcionamiento: un usuario autenticado inicia sesión, registra movimientos de inventario, y consulta el estado actual de los productos.
El sistema cubre tres funciones principales:
Autenticación de usuario mediante JWT.
Registro de movimientos de inventario, tanto entradas como salidas.
Consulta del inventario actual con nombre y cantidad de cada producto.

Funcionamiento
El usuario ingresa con credenciales fijas en memoria.
El backend genera un token JWT.
El frontend guarda ese token y lo envía en cada petición protegida.
El usuario puede registrar entradas o salidas de productos.
La aplicación actualiza el inventario en PostgreSQL.
El usuario consulta la lista de productos con sus cantidades actuales.

Tecnologías usadas
Backend:
C#
ASP.NET Core
Entity Framework Core
JWT Bearer Authentication
PostgreSQL

El backend expone una API REST sencilla con estos endpoints:
POST /auth/login
POST /productos/movimiento
GET /productos/inventario

La base de datos usa una sola tabla llamada productos, con los campos:
id
nombre
cantidad

Los datos iniciales se cargan de forma manual al iniciar la aplicación.

Frontend:

Angular v19
TypeScript
Reactive Forms
HttpClient

El frontend tiene una interfaz simple con:
Pantalla de login
Formulario para registrar movimientos
Pantalla de inventario
Validaciones básicas de campos obligatorios
Manejo del token JWT para llamar a la API

Base de datos
PostgreSQL
CRUD y movimientos manejados directamente con EF Core

Notas importantes
En la máquina donde se implementó, el backend quedó corriendo con el SDK disponible localmente, que fue .NET 10. La estructura del proyecto sigue siendo totalmente compatible con la idea original del ejercicio y se puede ajustar a .NET 9 si el entorno lo requiere.
