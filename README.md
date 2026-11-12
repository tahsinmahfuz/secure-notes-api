# Secure Notes API

ASP.NET Core Web API with Clean Architecture, MongoDB.Driver, JWT authentication, role-based access control, and a vanilla HTML/CSS/JS test client.

## Solution Structure

- `Domain`: Entities (`User`, `Note`, `Post`), `Role` enum, read models, and repository/security interfaces.
- `Application`: Request/response DTOs, `PagedResult<T>`, manual mapping helpers, and services (`AuthService`, `UserService`, `NoteService`, `PostService`).
- `Infrastructure`: `MongoDbContext`, `MongoClassMappings`, `MongoIndexInitializer`, `DataSeeder`, MongoDB repositories, `JwtProvider`, and `PasswordHasher` (BCrypt).
- `WebApi`: REST controllers, JWT bearer configuration, exception middleware, and static frontend under `WebApi/wwwroot/`.

## MongoDB Indexes

All custom indexes are defined in `Infrastructure/Persistence/MongoIndexInitializer.cs` and created on application startup:

1. `users`: `{ email: 1 }` (unique)
2. `users`: `{ createdAt: -1 }`
3. `users`: `{ interests: 1 }`
4. `notes`: `{ userId: 1, createdAt: -1 }`
5. `notes`: `{ createdAt: -1 }`
6. `posts`: `{ userId: 1, createdAt: -1 }`
7. `posts`: `{ createdAt: -1 }`

## Aggregation Pipelines

- `GET /api/users/interests?page=1&limit=10`: Executes a single `users` aggregation pipeline starting with `$match: { interests: { $exists: true, $ne: [] } }` (using the `interests` multikey index), followed by `$unwind`, `$group`, `$sort`, and `$facet` pagination.
- `GET /api/users/{id}/posts?page=1&limit=10`: Executes a single `users` aggregation pipeline with `$match` by `_id`, `$lookup` on `posts` (`localField: "_id"`, `foreignField: "userId"`, sorted and paginated sub-pipeline), and `$project` excluding `passwordHash`.

## API Endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

### Notes (Authenticated)
- `GET /api/notes?page=1&limit=10` (current user's own notes)
- `GET /api/notes/all?userId=&page=1&limit=10` (Admin only)
- `GET /api/notes/{id}` (owner or Admin)
- `POST /api/notes`
- `PUT /api/notes/{id}` (owner only)
- `DELETE /api/notes/{id}` (owner only)

### Posts
- `GET /api/posts?page=1&limit=10` (public)
- `GET /api/posts/{id}` (public)
- `POST /api/posts` (authenticated)
- `PUT /api/posts/{id}` (owner only)
- `DELETE /api/posts/{id}` (owner only)

### Users
- `GET /api/users/me` (authenticated)
- `PUT /api/users/me` (authenticated)
- `GET /api/users/interests?page=1&limit=10` (authenticated)
- `GET /api/users/{id}/posts?page=1&limit=10` (public)
- `GET /api/users?page=1&limit=10` (Admin only)
- `GET /api/users/{id}` (Admin only)
- `POST /api/users` (Admin only)
- `PUT /api/users/{id}` (Admin only)
- `DELETE /api/users/{id}` (Admin only)

## Configuration

Set values in `WebApi/appsettings.json` or via environment variables:

- `MONGODB_URI`: MongoDB connection string (e.g., MongoDB Atlas URI)
- `MONGODB_DATABASE`: Database name (defaults to `secure_notes`)
- `JWT_SECRET`: Signing key for JWT tokens (minimum 32 characters)
- `CORS_ORIGINS`: Optional comma-separated list of allowed frontend origins

Default seeded admin credentials in `WebApi/appsettings.json`:

- Email: `admin@example.com`
- Password: `ChangeMe123!`

Change `SeedAdmin` and `Jwt:Secret` before deploying to production.

## Running Locally

```bash
dotnet run --project WebApi/WebApi.csproj
```

Open `http://localhost:5176` in a browser to access the static test frontend.

## Deployment

- Backend (Render): Deploy as a Docker Web Service using the root `Dockerfile` and configure `MONGODB_URI`, `MONGODB_DATABASE`, and `JWT_SECRET`.
- Frontend (Vercel or Netlify): Deploy the `WebApi/wwwroot` directory as a static site and set the API URL field in the top bar (or deploy together from `WebApi`).

