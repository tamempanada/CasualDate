# Deployment Checklist

Use this before the first public release and for each schema-changing deployment. Casual Date is not production-ready merely because it runs locally: hosting, production MySQL, persistent storage, HTTPS, and real browser smoke tests still need to be configured and verified.

## 1. Repository and Release

- Review `git status` and inspect every modified, deleted, and untracked source file. This checkout currently contains extensive uncommitted work; do not stage everything blindly.
- Keep generated build output, local settings, database backups, and private uploads out of Git. Confirm `git check-ignore` for local settings and uploads.
- Commit the intended changes in reviewable units, then push the reviewed branch. This document's preparation does not create commits or push.
- Ensure the GitHub repository is private if source or assets contain personal information; review committed artwork/music licenses and permission before publication.
- Build from a clean clone/CI runner, not only from an existing `bin/` directory.
- The project references ImageSharp 4.1.2. A Release build in this environment fails without the Six Labors license input. Obtain an appropriate license for the project's use and configure the full key as the `SIXLABORS_LICENSE_KEY` build secret (or provide a private `sixlabors.lic` file); never commit a private key. The official [ImageSharp licensing instructions](https://docs.sixlabors.com/articles/imagesharp/index.html#how-to-use-the-license-file) document these options.

## 2. Production Configuration and Secrets

Configure these environment variables in the host's secret/configuration manager; do not commit real values:

| Variable | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | MySQL connection string for the production database |
| `SeedAdmin__Email` | Initial/managed seed administrator email |
| `SeedAdmin__Password` | Strong private administrator password |

The seed routine synchronizes the configured administrator password at application startup. Keep the configured password private and stable, and understand that changing it changes the seed account password on a subsequent startup. Confirm the intended account and role assignments before enabling public traffic.

- Use a dedicated least-privilege MySQL account; do not deploy with `root`.
- Restrict database network access to the application host and use TLS where supported by the hosting/database setup.
- Set `ASPNETCORE_ENVIRONMENT=Production`; do not publish Development settings or diagnostics.
- Require HTTPS and verify proxy/forwarded-header behavior when TLS terminates at a reverse proxy.
- Persist ASP.NET Core Data Protection keys across restarts (and share them if running multiple instances) so authentication cookies remain usable.

## 3. Database and Migrations

1. Take and verify a restorable backup of the existing database before upgrading it.
2. Confirm the target database and server version, then inspect the migration list and generated SQL for pending migrations against a safe copy of production data.
3. Apply the intended migrations and inspect `__EFMigrationsHistory` and the required tables/columns after startup.
4. Review application logs for migration or seed warnings. Current startup code catches migration/seed exceptions and may continue serving; a healthy process does not prove the database is current.
5. Verify player accounts, wallet balances, Gacha pulls/collection, progress, uploaded-media metadata, and admin access still exist. Never reset production data to resolve a schema mismatch.

For local inspection, from the project directory:

```powershell
dotnet ef migrations list --context ApplicationDbContext
dotnet ef migrations has-pending-model-changes --context ApplicationDbContext
```

The runtime includes legacy schema-repair code as well as EF migrations. Review the database state carefully when updating older installations; do not assume migration failure is harmless.

## 4. Persistent Files and Backups

- Uploads are stored under `wwwroot/uploads/` on the local filesystem. The current checkout contains real uploaded images and audio; these are deliberately not included in source control.
- Production hosting must mount this path on durable storage or replace local storage with a durable media service before deployment. Ephemeral container filesystems will lose uploads on redeploy/restart.
- Back up uploads together with the database/media metadata and test a restore. Keep the database and file backups from the same approximate point in time.
- Check file permissions, capacity, upload size limits, and HTTPS before allowing admin uploads.

## 5. Build and Functional Smoke Tests

- Restore packages and build Release from a clean checkout using the required .NET SDK.
- Configure the Six Labors license secret before Release builds and `dotnet publish` when building ImageSharp 4.1.2.
- Run any automated tests present. The current repository has no discovered test project, so critical flows require manual/browser verification.
- Verify first-run database migration/seed on a disposable database before touching production.
- Test admin login, access control, account creation, settings save, media upload/delete, and player login.
- Test Photobooth permission denied and real capture, Cake and Perfect Date completion, quiz endings, Hangman progress, Blackjack actions, Love Blocks state, Gacha 1/10 pulls and pity/collection/discard, wallet balance/history, and completion rewards.
- Test mobile/desktop layouts, HTTPS, music across navigation, and browser refresh/session behavior.
- Confirm logs contain no connection strings, passwords, or personal data; confirm the error page does not expose developer diagnostics.

## 6. Rollout and Recovery

- Deploy first to a staging environment using the same hosting and database configuration pattern as production.
- Verify health, critical player/admin workflows, migration history, logs, and persistent uploads after restart.
- Have a rollback plan for both application version and database. Prefer forward-fix migrations; do not assume rolling back code reverses data changes safely.
- Keep a release note with the commit hash, migration IDs, backup reference, smoke-test result, and any known limitations.
