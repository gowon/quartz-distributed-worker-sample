# Core.Quartz.EFCore

## Implementation Notes

- Problem: Quartz.NET does not manage the database schema when using database store
- Solution: Wrap database schema in EF Core management so Migration can be checked/applied programmatically
- References:
  - <https://github.com/appany/AppAny.Quartz.EntityFrameworkCore.Migrations>
