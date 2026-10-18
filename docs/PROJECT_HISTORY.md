# Casual Date Project History

This is a feature-oriented summary of the project's progression. The detailed `STEP_*.md` files record the original implementation milestones; this document consolidates them with the later systems added as the project grew. It describes the repository as inspected on 2026-10-18 and is not a claim that every production deployment check has passed.

## Foundation and Core Experience

- Started as an ASP.NET Core MVC application with Razor views, JavaScript, MySQL, EF Core, and Identity.
- Added the invitation and main menu, administrator-managed game settings and media, and personalized content.
- Built the original games: Photobooth with camera capture and printable strip, Bake a Cake, Perfect Date, and How Well Do You Know Me? quiz.
- Added configured results, completion flow, personalized endings, and menu progress.

## Admin, Media, and Music

- Expanded protected administration for game content, users, media, and settings.
- Added image/audio management and game-specific media, including quiz answer images and Perfect Date choices.
- Added a persistent site-level music player with album artwork, playback controls, queue and selection behavior; ordinary screen navigation is intended not to restart playback.
- Added the invitation No-button behavior and result presentation refinements.

## Player Accounts and Persistent Game State

- Added player accounts created by an administrator, player profiles, and persistent token wallet/transaction records.
- Added server-side game completion attempts and reward processing to make completion grants idempotent.
- Added the Girlfriend Gacha reward pool, token-priced pulls, pity state, pull history, collection snapshots, cosmetics/companions, and an admin-managed banner.
- Added persistent Hangman progress and rewards, Blackjack rounds, and the Love Blocks game with saved run state and configurable rewards.
- Replaced the Scrapbook Diary menu entry with Love Blocks while retaining the historical scrapbook implementation/data model.

## Persistence and Compatibility Notes

- The EF migration history has evolved alongside player profiles, wallets, Gacha, game progress, completion verification, cosmetics, and Love Blocks.
- `Program.cs` also contains runtime schema-repair SQL intended to tolerate legacy databases that experienced partial or duplicate migrations. This is compatibility code, not a substitute for reviewing migration status.
- Keep the existing MySQL database and uploaded media when deploying. Do not reset the schema to resolve migration issues.
- Historical milestones remain in `docs/STEP_*.md`; some early setup documents describe the starter state and may not reflect today's architecture. Use the current README and deployment checklist for setup.

## Current Repository State (2026-10-18)

- The configured Git remote is `https://github.com/tamempanada/CasualDate.git`; the local branch is `main` and tracks `origin/main`.
- The checked-out revision is the initial repository commit. The working tree contains substantial uncommitted feature changes, including Love Blocks, new migrations, controllers, views, and assets.
- These local changes have not been committed or pushed by this preparation task. Review them as one or more coherent commits before merging/publishing.
- Build outputs and private uploads are local data and should not be committed. See `.gitignore` and [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md).

## Verification Boundaries

The project has no discovered automated test project in the current checkout. A successful compile or migration-model check is useful but does not replace browser testing, MySQL migration verification, or production smoke testing. Record those results at release time rather than retroactively assuming success.
