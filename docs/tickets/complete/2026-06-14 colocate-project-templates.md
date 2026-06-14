# Colocate the `dotnet new` project templates into this repo

## Context

**Current behavior**: The PlayPlayMini project templates (the `dotnet new` templates published as the `BenMakesGames.PlayPlayMini.Templates` NuGet package) live in a separate repository, `PlayPlayMiniTemplates` (https://github.com/BenMakesGames/PlayPlayMiniTemplates). That repo is small and self-contained: a `templatepack.csproj` (a `PackageType=Template` content-only pack with its own independent version line, currently `5.0.0`), a templates-specific `README.md` and `LICENSE.md`, and a `templates/` tree containing three templates — `skeleton`, `serilog`, and `steam`. Each template is a full sample project (`MyNamespace.csproj`, `Program.cs`, `Content/`, `GameStates/`, `.template.config/template.json`, etc.) and references the framework by **published** NuGet version (`<PackageReference Include="BenMakesGames.PlayPlayMini" Version="8.1.0" />`), not by project reference. Because the templates live in a different repo, keeping the templates and the framework in step is a manual cross-repo chore, and the template package is built and published entirely separately.

**New behavior**: The templates live in this repository under a top-level `Templates/` folder, and `Templates/templatepack.csproj` is a member of the solution (`BenMakesGames.PlayPlayMini.slnx`). A `dotnet build -c Release` of the solution packs the `BenMakesGames.PlayPlayMini.Templates` package alongside the framework packages, and `build.ps1` sweeps and offers to push it like any other package. The templates still reference the framework by published NuGet version (not a project reference), and that pinned version is still bumped deliberately on stable release — colocation buys proximity, not automatic version sync. Cross-references in the repo's docs and the pack's own NuGet metadata point at this repo as the templates' home.

## Scope

### In scope

- Copy the `PlayPlayMiniTemplates` repo's tracked contents into a new top-level `Templates/` folder (the pack project, its README/LICENSE, and the `templates/` tree).
- Add `Templates/templatepack.csproj` to `BenMakesGames.PlayPlayMini.slnx`.
- Repoint the pack's `PackageProjectUrl` / `RepositoryUrl` to this repo, and update the doc cross-references that point at the old standalone repo.
- A clean `dotnet build -c Release` to confirm the templates package still packs correctly from the new path.

### Out of scope

- **Archiving / redirecting the old `PlayPlayMiniTemplates` GitHub repo.** That's a separate follow-up once the templates have a confirmed home here; this ticket does not touch the remote repo.
- **Automating the template's pinned framework version.** The template's `BenMakesGames.PlayPlayMini` `PackageReference` version stays a deliberate, manually-bumped value (see Constraints) — no build step rewrites it, no auto-sync to this repo's version.
- **Changing the templates themselves** (their code, content, or the set of templates). This is a move + wire-up, not a template revision. Bumping the pinned framework version to the current stable release is a normal release-checklist action, not part of this ticket.
- **Adding the inner template `MyNamespace.csproj` projects to the solution.** They are template *content*, not buildable members of this solution (see Constraints).
- **Any package-publishing CI.** This repo publishes manually via `build.ps1`; that stays.

## Relevant Docs & Anchors

- **Source repo**: `PlayPlayMiniTemplates` (https://github.com/BenMakesGames/PlayPlayMiniTemplates) — copy its tracked files (the pack project, README, LICENSE, and `templates/` tree). Reference it by URL/name; do not embed any local filesystem path in code or docs.
- **Pack project**: `templatepack.csproj` in the source repo — note `PackageType=Template`, `IncludeBuildOutput=false`, `ContentTargetFolders=content`, `Compile Remove="**\*"`, the `Content Include="templates\**\*" Exclude="...bin/obj..."`, and the `None Include` entries for `LICENSE.md` / `README.md` (relative to the project). Its `PackageVersion` (`5.0.0`) is independent of the framework `Version`.
- **Solution**: `BenMakesGames.PlayPlayMini.slnx` — add the new project as a `<Project Path="..." />` entry, mirroring the existing entries.
- **Build/release flow**: `build.ps1` — it runs `dotnet clean/build -c Release`, then recursively sweeps `*.nupkg`/`*.snupkg` newer than the build start and offers to copy + `dotnet nuget push --skip-duplicate`. Understand that the templates package will flow through this automatically once it's in the solution.
- **Framework pack reference for placement contrast**: `BenMakesGames.PlayPlayMini/BenMakesGames.PlayPlayMini.csproj` — its `Version` (currently `8.2.0-rc6`) is the framework version and is **not** what the templates should pin.
- **Doc cross-references to update**: root `README.md` (the line linking to `PlayPlayMiniTemplates`), `BenMakesGames.PlayPlayMini/README.md`, `BenMakesGames.PlayPlayMini/UPGRADE.md`, and `docfx/index.md` (the `dotnet new install` / `dotnet new playplaymini.skeleton` references).

## Constraints & Gotchas

- **Place the pack in its own folder, not the repo root.** `templatepack.csproj` packs a `README.md` and `LICENSE.md` *relative to itself*, and the templates README is install/usage instructions — different content from the repo's root `README.md`/`LICENSE.md`. Keeping the whole pack (project + its own README/LICENSE + `templates/`) together under `Templates/` preserves the relative paths and avoids the pack grabbing the framework's README. Do not point the pack at the repo-root README/LICENSE.
- **Keep the inner `MyNamespace.csproj` template projects out of the solution.** They are content shipped inside the package; they reference the framework via NuGet (not the sibling project) and must never be built as solution members. `Compile Remove="**\*"` in the pack already shields the pack build from the template `.cs` files. Add only `templatepack.csproj` to the `.slnx`.
- **`bin`/`obj` are already globally gitignored** at the repo root, which covers any build output generated under the template projects — no per-folder ignore needed.
- **Template version pinning tracks the last *stable* framework release, not this repo's version.** This repo is often at a prerelease (`8.2.0-rc6` today). The templates must reference a framework version that exists on NuGet.org as a stable package, so the pinned `8.1.0` is intentionally behind. A `ProjectReference` is wrong for the same reason (consumers of `dotnet new` need a published package). Treat the bump as a release-checklist step.
- **`build.ps1` pushes with `--skip-duplicate`.** Because the templates package version (`5.0.0`) is independent, the unified build will only actually publish a new templates package when that version is bumped; otherwise the push is a no-op skip. So adding the pack to the solution does not force a templates release on every framework build.

## Open Decisions

1. **Templates `LICENSE.md`: keep a separate copy vs. dedupe against the repo root.** Default: keep the copy that travels with the pack (simplest; the pack references its own adjacent `LICENSE.md`). Dedup against the root license is only worth it if they're byte-identical and you'd rather repoint the `None Include` — decide during implementation.
2. **Folder name / casing for the new top-level folder.** Default: `Templates/` (matches the framework's PascalCase project-folder convention). The inner `templates/` content folder name comes from the source repo and should stay as-is so the pack's `Content Include="templates\**\*"` glob keeps resolving.

## Acceptance Criteria

- [ ] A top-level `Templates/` folder exists containing `templatepack.csproj`, a templates-specific `README.md` and `LICENSE.md`, and the `templates/{skeleton,serilog,steam}/` trees with their `.template.config/template.json`, project files, and content intact.
- [ ] `BenMakesGames.PlayPlayMini.slnx` includes a `<Project Path="Templates/templatepack.csproj" />` entry (path may vary with the chosen folder name) and includes none of the inner `MyNamespace.csproj` template projects.
- [ ] `templatepack.csproj`'s `PackageProjectUrl` and `RepositoryUrl` point at this repository (the PlayPlayMini repo), not the old `PlayPlayMiniTemplates` repo.
- [ ] The templates' `BenMakesGames.PlayPlayMini` `PackageReference` version is unchanged by any automated step (still a manually-set stable version; no build logic rewrites it).
- [ ] `dotnet build -c Release` produces a `BenMakesGames.PlayPlayMini.Templates` `.nupkg` whose contents include the `templates/` tree under the package's content folder (i.e., the pack still works from the new location).
- [ ] No code or doc committed in this repo contains a local filesystem path to the templates source (no drive letters / absolute developer paths); the source is referred to by repo name/URL only.
- [ ] Root `README.md`, `BenMakesGames.PlayPlayMini/README.md`, `BenMakesGames.PlayPlayMini/UPGRADE.md`, and `docfx/index.md` reference the templates as living in this repo (the stale standalone-repo link is updated or removed); the `dotnet new install BenMakesGames.PlayPlayMini.Templates` install instruction still reads correctly.

## Implementation

### 1. Copy the templates into a `Templates/` folder

Bring the tracked contents of the `PlayPlayMiniTemplates` repo (https://github.com/BenMakesGames/PlayPlayMiniTemplates) into a new top-level `Templates/` folder in this repo: `templatepack.csproj`, the templates' `README.md` and `LICENSE.md`, and the entire `templates/` tree (the `skeleton`, `serilog`, and `steam` directories with their `.template.config/`, `.config/`, `Content/`, `GameStates/`, project, and asset files). Do not copy `bin/`, `obj/`, `.idea/`, or the source repo's `.git`. Keep the inner `templates/` content-folder name exactly as-is so the pack's `Content Include="templates\**\*"` glob still resolves.

### 2. Repoint the pack's NuGet metadata to this repo

In `Templates/templatepack.csproj`, change `PackageProjectUrl` and `RepositoryUrl` from the old standalone-repo URL to this repository's URL (mirror the URLs used in `BenMakesGames.PlayPlayMini/BenMakesGames.PlayPlayMini.csproj`). Leave the independent `PackageVersion` (`5.0.0`), `PackageId`, `PackageType`, and the template's `BenMakesGames.PlayPlayMini` `PackageReference` version untouched.

### 3. Add the pack to the solution

Add `Templates/templatepack.csproj` to `BenMakesGames.PlayPlayMini.slnx` as a new `<Project Path="..." />` entry, matching the formatting of the existing project entries. Add only this one project — not the inner `MyNamespace.csproj` template projects.

### 4. Update doc cross-references

Update the references that point readers at the old standalone repo so they describe the templates as part of this repo: the `PlayPlayMiniTemplates` link in the root `README.md`, and the template mentions in `BenMakesGames.PlayPlayMini/README.md`, `BenMakesGames.PlayPlayMini/UPGRADE.md`, and `docfx/index.md`. The `dotnet new install BenMakesGames.PlayPlayMini.Templates` and `dotnet new playplaymini.skeleton` usage lines stay (the package name is unchanged); just fix any "see the other repo" framing and the stale repo URL.

### 5. Build and verify the package packs

Run `dotnet build -c Release` and confirm the `BenMakesGames.PlayPlayMini.Templates` package is produced and contains the `templates/` tree under its content folder (inspect the generated `.nupkg`, e.g. unzip and check the `content/` path). Confirm the build is clean and the framework packages still build as before.

## Test Plan

- [ ] `dotnet build -c Release` completes cleanly; both the framework packages and `BenMakesGames.PlayPlayMini.Templates` `.nupkg` are produced.
- [ ] Inspect the generated `BenMakesGames.PlayPlayMini.Templates.*.nupkg` (it's a zip): confirm the `templates/skeleton`, `templates/serilog`, and `templates/steam` files are present under the package content folder, and the templates' README/LICENSE are packed.
- [ ] Smoke-test the templates locally: `dotnet new install` the freshly-built package (from the local `.nupkg`), then run `dotnet new playplaymini.skeleton -n SmokeTest` in a scratch directory and confirm a project is scaffolded; `dotnet new uninstall` afterward. (Generated project will reference the published framework version, which is expected.)
- [ ] `git grep` the repo for any absolute/local filesystem path to the templates source (drive letter or developer home path) — confirm none were committed.
- [ ] Open the rendered docs / READMEs and confirm the templates are described as part of this repo and the install instructions read correctly.
