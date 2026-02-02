# AGENTS.md

## Project overview
- ASP.NET Core (net8.0) server.
- Entry point: `Application.cs` (`dotnet run` to start).
- Binds to `http://0.0.0.0:5000`.
- SQLite DB at `db/app.db` (created on startup).
- Admin static files served from `wwwroot/admin` at `/admin`.

## Conventions
- Use UTF-8 + LF for all text files.
- Use 4 spaces for indentation.
- Keep nullable enabled and avoid suppressing warnings without reason.

## Common commands
- Build: `dotnet build`
- Run: `dotnet run`
- Tests: no test project found in this repo.

## Recommended items
- Add a `.editorconfig` that enforces UTF-8 + LF and 4-space indentation.
- Add a test project and wire `dotnet test` into CI.
- Document API endpoints and admin flows in a short README.
