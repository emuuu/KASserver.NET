# Contributing to KASserver.NET

Thank you for considering contributing to KASserver.NET! Every contribution helps make this library better.

## Reporting Issues

- **Bug Reports:** Use the [Bug Report template](https://github.com/emuuu/KASserver.NET/issues/new?template=bug_report.yml) and include reproduction steps, expected/actual behavior, and your environment (.NET version, OS).
- **Feature Requests:** Use the [Feature Request template](https://github.com/emuuu/KASserver.NET/issues/new?template=feature_request.yml) and describe the use case you're trying to solve.

## Development Setup

### Prerequisites

- [.NET 8.0, 9.0, or 10.0 SDK](https://dotnet.microsoft.com/download)
- An ALL-INKL.COM account with KAS API access (only needed for integration testing against the live API)

### Clone & Build

```bash
git clone https://github.com/emuuu/KASserver.NET.git
cd KASserver.NET
dotnet restore
dotnet build
```

### Run Tests

```bash
dotnet test tests/KASserver.Client.Tests/
```

## Working Against the KAS API

This library is built strictly against the [official KAS API documentation](https://kasapi.kasserver.com/dokumentation/phpdoc/). When adding new actions:

- Verify parameter names, types, and defaults against the PHPDoc — do not guess.
- Remember that the KAS WSDL is faulty (declares `Result`, server returns `return`); all calls go through the raw-SOAP transport.
- Honor the `KasFloodDelay` from each response — never hardcode a delay.

## Pull Request Guidelines

1. **Branch from `main`** — create a feature or fix branch (e.g. `feat/dns-actions` or `fix/issue-42`).
2. **Write tests** — all new functionality should include unit tests.
3. **Fill out the PR template** — describe what changed and why.
4. **Keep PRs focused** — one logical change per PR.
5. **Ensure CI passes** — all matrix builds (net8.0, net9.0, net10.0) must be green.

## Changelog

Add an entry under the `## [Unreleased]` heading in [CHANGELOG.md](CHANGELOG.md) for any user-facing change, grouped under `### Added` / `### Changed` / `### Fixed` (Keep a Changelog format). You do **not** create a version section by hand — the release process does that for you.

## Releasing

Releases are cut by running the **Create Release Tag** workflow (`workflow_dispatch`) with the target SemVer version. For a stable release it automatically:

1. verifies CI is green on `main`,
2. promotes the `[Unreleased]` changelog section into a dated `## [version]` section (and regenerates the compare links) via `.github/scripts/update_changelog.py`, committing that to `main` — so it **fails if `[Unreleased]` is empty**,
3. creates and pushes the `kasserver/v<version>` tag, which triggers the pack + NuGet publish + GitHub Release.

Prereleases (`-preview.N`) skip the changelog promotion.

## Code Style

This project uses an `.editorconfig` for consistent formatting:

- 4 spaces indentation (no tabs)
- UTF-8 encoding, LF line endings
- PascalCase for public members, `_camelCase` for private fields
- `System` usings sorted first

## Commit Conventions

We follow [Conventional Commits](https://www.conventionalcommits.org/):

| Prefix | Purpose |
|--------|---------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `docs:` | Documentation only |
| `test:` | Adding or updating tests |
| `chore:` | Build, CI, dependencies |
| `refactor:` | Code change that neither fixes a bug nor adds a feature |

Example: `feat: add DNS record actions`

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE.txt).
