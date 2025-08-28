Activity Flavors (Content + Branding)

- Core domain is generic (ActivityEntry with Quantity) and maps to existing DB tables/columns, so current data stays intact.
- Per-app content and branding are provided via DI:
  - `IActivityContent`: units, minimal quantity, text/messages.
  - `IActivityBranding`: app name, logo/splash, theme colors.

Defaults

- Pushups (default): `PushupContent`, `PushupBranding`.
- Reading (example): `ReadingContent`, `ReadingBranding`.

Switching Flavor

- At runtime via env var `ONEAPP_FLAVOR`:
  - `reading` → Reading flavor
  - any other value or unset → Pushups

Creating a New Flavor

- Add classes implementing `IActivityContent` and `IActivityBranding` under `OnePushup/Flavors/<YourFlavor>/`.
- Register them in `MauiProgram.CreateMauiApp()` (replace or conditionally add).
- Provide assets in `wwwroot` and adjust `LogoPath`/`SplashPath`.

UI Wiring

- `DailyActivity` uses `IActivityContent` for titles, prompts, and thresholds (minimal quantity and “more” input).
- `MainLayout` uses `IActivityBranding` for app title and splash/logo.
- Android notifications use `IActivityContent` for channel name/description and messages.

Data Preservation

- `ActivityEntry` maps to legacy table `TrainingEntries` with column `NumberOfRepetitions` → `Quantity` using Fluent API in `OneActivityDbContext`.
- No migration is required to keep existing user data.

