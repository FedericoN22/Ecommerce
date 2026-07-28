# E-commerce API

API RESTful para un e-commerce desarrollada con ASP.NET Core 8, siguiendo una arquitectura limpia por capas.

## ¿Qué hace la API?

Permite gestionar un sistema de comercio electrónico completo: autenticación de usuarios, catálogo de productos con búsqueda y filtros, carrito de compras, y checkout con pagos reales mediante Stripe.

### Principales funcionalidades

- **Autenticación** — Registro e inicio de sesión con JWT, roles de usuario (Admin/User).
- **Catálogo** — Productos con búsqueda, filtrado por categoría, paginación.
- **Carrito** — Agregar, actualizar, eliminar productos y vaciar carrito.
- **Checkout** — Creación de órdenes, integración con Stripe Checkout, webhooks para confirmación de pago.
- **Administración** — CRUD completo de productos y categorías (protegido con rol Admin).

## Características

- Autenticación basada en **JWT**.
- Roles **Admin** y **User** con autorización por endpoint.
- CRUD completo de **productos** y **categorías**.
- **Carrito de compras** por usuario.
- **Checkout** con **Stripe** (modo pago único).
- **Webhook** de Stripe para confirmar pagos y actualizar inventario.
- **Validaciones** con FluentValidation en todos los DTOs.
- **Manejo global de errores** con middleware.
- **Background service** para expiración automática de órdenes pendientes.
- Documentación interactiva con **Swagger**.
- **113 tests** (unitarios y de integración).

## Tecnologías

| Tecnología                                    | Propósito                               |
| --------------------------------------------- | --------------------------------------- |
| **ASP.NET Core 8** (Minimal APIs)             | Framework web                           |
| **Entity Framework Core 8**                   | ORM                                     |
| **PostgreSQL** (Npgsql)                       | Base de datos                           |
| **ASP.NET Core Identity**                     | Gestión de usuarios y roles             |
| **JWT Bearer**                                | Autenticación                           |
| **Stripe.net**                                | Pasarela de pagos                       |
| **FluentValidation**                          | Validación de DTOs                      |
| **SharpGrip.FluentValidation.AutoValidation** | Validación automática en endpoints      |
| **xUnit**                                     | Testing                                 |
| **Moq**                                       | Mocking en tests                        |
| **SQLite In-Memory**                          | Base de datos para tests de integración |
| **Microsoft.AspNetCore.Mvc.Testing**          | Test server para integración            |
| **Swagger / Swashbuckle**                     | Documentación interactiva               |

## Arquitectura

El proyecto sigue una arquitectura limpia en 4 capas:

```
┌─────────────────────────────────────┐
│           API (Endpoints)           │
│  Program.cs / Endpoints/ / Middleware│
├─────────────────────────────────────┤
│         Application (Services)       │
│  DTOs / Interfaces / Services / Validators
├─────────────────────────────────────┤
│         Domain (Entities)           │
│  Entidades e interfaces base        │
├─────────────────────────────────────┤
│       Infrastructure (Data/Services) │
│  DbContext / Configurations / Identity│
│  StripePaymentGateway / Background   │
└─────────────────────────────────────┘
```

- **Domain** — Entidades del negocio e interfaces base (`IAuditableEntity`).
- **Application** — DTOs, interfaces de servicios, implementación de la lógica de negocio, validadores.
- **Infrastructure** — Persistencia (EF Core DbContext + configuraciones Fluent API), Identity, pasarela de pagos (Stripe), background services.
- **API** — Endpoints (Minimal APIs), middleware global de errores, configuración (CORS, JWT, Swagger).

## Estructura del proyecto

```
E-commerceApi/
├── Application/
│   ├── DTOs/              # Auth, Cart, Category, Common, Order, Payment, Product, Queries
│   ├── Exceptions/        # InsufficientStockException
│   ├── Interfaces/        # IAuthService, ICartService, ICategoryService, etc.
│   ├── Services/          # AuthService, CartService, CategoryService, OrderService, ProductService
│   └── Validators/        # FluentValidation (Auth, Cart, Category, Product)
├── Domain/
│   └── Entities/          # productETT, categoryETT, cartETT, cartItemETT, OrderETT, orderItemETT
├── Infrastructure/
│   ├── Configuration/     # Fluent API entity configurations
│   ├── Data/              # AppDbContext
│   ├── identity/          # ApplicationUsers
│   └── Services/          # StripePaymentGateway, PendingOrderExpirationService
├── Endpoints/             # authEndpoints, adminEndpoints, publicCatalogEndpoints, cartEndpoints, orderEndpoints
├── Middleware/            # ExceptionMiddleware
├── extension/             # CORS, Database, Identity, JWT, Service, Swagger extensions
├── Migrations/            # EF Core migrations
├── Program.cs
├── appsettings.json
└── appsettings.template.json
```

## Instalación y configuración

### Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Stripe CLI](https://stripe.com/docs/stripe-cli) (para webhook local)
- Cuenta de Stripe (claves API)

### Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/ecommerce-api.git
cd ecommerce-api
```

### Restaurar paquetes

```bash
dotnet restore
```

### Configurar appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ecommerce;Username=postgres;Password=tu_password"
  },
  "Jwt": {
    "Key": "UnaClaveSuperSeguraDeMasDe32Caracteres",
    "Issuer": "ECommerceApi",
    "Audience": "ECommerceClient",
    "DurationInMinutes": "60"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_...",
    "SuccessUrl": "https://tusitio.com/success?sessionId={CHECKOUT_SESSION_ID}",
    "CancelUrl": "https://tusitio.com/cancel"
  }
}
```

### Ejecutar migraciones

```bash
dotnet ef database update
```

### Ejecutar la aplicación

```bash
dotnet run
```

La API estará disponible en `http://localhost:5003` y Swagger en `http://localhost:5003/swagger`.

## Configuración de Stripe

### Claves

1. Crea una cuenta en [Stripe](https://dashboard.stripe.com/).
2. Obtén tu **Secret Key** desde el dashboard (modo prueba).
3. Obtén tu **Webhook Secret** configurando un endpoint de webhook.
4. Copia ambas claves en `appsettings.json` en la sección `Stripe`.

### Stripe CLI (webhook local)

Para probar webhooks en desarrollo:

```bash
stripe login
stripe listen --forward-to https://localhost:5003/api/webhooks/stripe
```

Esto mostrará un `webhook-secret` que debes copiar en `appsettings.json`.

Para simular un pago exitoso:

```bash
stripe trigger checkout.session.completed
```

## Endpoints principales

| Grupo        | Método | Ruta                         | Auth  | Descripción                                           |
| ------------ | ------ | ---------------------------- | ----- | ----------------------------------------------------- |
| **Auth**     | POST   | `/api/auth/register`         | No    | Registro de usuario                                   |
|              | POST   | `/api/auth/login`            | No    | Inicio de sesión                                      |
| **Catálogo** | GET    | `/api/products`              | No    | Listar productos (search, categoryId, page, pageSize) |
|              | GET    | `/api/products/{id}`         | No    | Detalle de producto                                   |
|              | GET    | `/api/categories`            | No    | Listar categorías                                     |
| **Admin**    | GET    | `/api/admin/categories`      | Admin | Listar categorías                                     |
|              | POST   | `/api/admin/categories`      | Admin | Crear categoría                                       |
|              | PUT    | `/api/admin/categories/{id}` | Admin | Actualizar categoría                                  |
|              | DELETE | `/api/admin/categories/{id}` | Admin | Eliminar categoría                                    |
|              | GET    | `/api/admin/products`        | Admin | Listar productos                                      |
|              | POST   | `/api/admin/products`        | Admin | Crear producto                                        |
|              | PUT    | `/api/admin/products/{id}`   | Admin | Actualizar producto                                   |
|              | DELETE | `/api/admin/products/{id}`   | Admin | Eliminar producto                                     |
| **Carrito**  | GET    | `/api/cart`                  | User  | Ver carrito                                           |
|              | POST   | `/api/cart`                  | User  | Agregar producto                                      |
|              | PUT    | `/api/cart/{cartItemId}`     | User  | Actualizar cantidad                                   |
|              | DELETE | `/api/cart/{cartItemId}`     | User  | Eliminar ítem                                         |
|              | DELETE | `/api/cart`                  | User  | Vaciar carrito                                        |
| **Checkout** | POST   | `/api/checkout`              | User  | Crear checkout (Stripe)                               |
| **Webhook**  | POST   | `/api/webhooks/stripe`       | No    | Webhook de Stripe                                     |

Para más detalle sobre parámetros y schemas, ejecuta la aplicación y accede a Swagger.

## Tests

```bash
dotnet test
```

Se ejecutan **113 tests** que cubren:

- **Validators** — Validación de cada DTO (campos requeridos, formato, longitudes).
- **Services** — Lógica de negocio: registro/login, CRUD de productos/categorías, carrito, checkout, confirmación de pago, expiración de órdenes.
- **Middleware** — Mapeo de excepciones a códigos HTTP (404, 400, 401, 409, 500).
- **Endpoints** — Tests de integración con SQLite In-Memory y WebApplicationFactory (autenticación, autorización por roles, flujos completos).

Tecnologías de testing: **xUnit**, **Moq**, **SQLite In-Memory**, **Microsoft.AspNetCore.Mvc.Testing**.

## Posibles mejoras futuras

- **Docker** — Contenerización de la API y base de datos.
- **CI/CD** — Pipeline de integración continua con GitHub Actions.
- **Caché** — Redis para catálogo de productos y sesiones.
- **Observabilidad** — Serilog, OpenTelemetry, métricas.
- **Rate Limiting** — Protección contra abusos en endpoints públicos.
- **Paginación con cursor** — Para catálogos grandes.
- **Imágenes de productos** — Subida y almacenamiento (cloud storage).

PROJECT-URL https://roadmap.sh/projects/ecommerce-api
