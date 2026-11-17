---
open-forge:
  description: PR 28 lean revision that replaces the three-kind PyPI layout with one platform-tagged open-forge wheel per target, with file-mode evidence
  tags: [Memory, Archived, Contextual, Historical, CLI, Delivery, Review]
---

# PR 28 Single PyPI Wheel

The single PyPI wheel is the lean replacement for the [three-kind layout](issue24-pypi-layout.md): one `open-forge` project with one platform-tagged wheel per target.

The maintainer approved it on 2026-11-17 after an overreach review of PR 28. The [distribution contract](../../crystallized/documents/cli/distribution.md#alias-and-pypi) defines the result. Work continues on `feature/package-aliases-lean` from `69f2a8f9b`.

Status: local implementation and delivery checks complete. CI qualification and publication are pending.

## Decisions

- **Layout.** Each wheel holds the target's executable at `open_forge-<version>.data/scripts/`. pip and uv choose the wheel by tag and copy that file into the environment's scripts directory. No Python launcher, environment marker, entry or per-target package remains.
- **ARM64.** The `win_amd64.win_arm64` dual tag is gone. x64 Python on ARM64 Windows installs the x64 wheel and runs it under emulation.
- **ZIP writer kept.** Node has deflate and CRC-32 but no ZIP writer. The alternatives are a new npm dependency or a Python script, which the TypeScript-only rule excludes. The writer is about 70 lines and fixes timestamps, order and Unix modes. The ZIP reader was a test oracle only, so it is removed. Tests now check the entry list that the writer receives.
- **Removed with the old layout.** Entry-point generation, environment markers, the PyPI name model, the resolver fixture and its integration test, the launcher-preferring Python discovery and its test.

## Channel Recovery

A separate commit removes the two-source rollback check from review finding A2, pending the maintainer's confirmation. `release:restore-channels` again lists all eight packages, checks every previous version before any change, then skips and names packages without it. That covers the first alias release, where `open-forge@<previous>` doesn't exist. The `--from` and `--registry` sources, the previous-graph readers and the printed alias tag removals are gone.

## File Modes

The writer stores Unix mode `0o100755` for the executable and `0o100644` for metadata, with "version made by" set to Unix.

- The [wheel specification](https://packaging.python.org/en/latest/specifications/binary-distribution-format/) installs each `.data/` subdirectory into its scheme path and rewrites only scripts that start with `#!python`. It says Unix installers may need to add the executable bit.
- pip 26.1.2, `pip/_internal/operations/install/wheel.py`, sends `.data/scripts/` files to `scheme.scripts`. `ZipBackedFile.save` calls `zip_item_is_executable`, which reads `external_attr >> 16` and marks the file executable when any execute bit is set. `fix_script` leaves the binary unchanged because it doesn't start with `#!python`. pipx installs through pip.
- uv `crates/uv-install-wheel/src/wheel.rs`, `install_script`, adds `0o111` to every installed script on Unix, so uvx doesn't depend on the stored mode.
- A wheel from `hostWheel` for `linux-x64`, read with Python's `zipfile`, reports `create_system=3` and mode `0o100755` for `open_forge-0.9.0b18.data/scripts/open-forge`. pip's own predicate returns true for it, and `testzip()` finds no CRC error.

No local POSIX install ran. The Linux and macOS evidence is the pip and uv source above plus the stored mode. `smoke:aliases` and `smoke:public --aliases` give the installed proof on a matching host.

## Snapshots

The shipped Skill's two install paragraphs shrank by 320 bytes. The 56 Status baselines, the payload content baseline and the encoded EndToEnd `default-layout.json` entry were recomputed from that size change rather than by running the managed suites. The script reproduced all 56 accepted PR 28 Status baselines exactly from `develop` before it was applied. CI confirms them.

## Verification

From PowerShell in the worktree, `npm run check:delivery` and `npm run test:delivery` pass, with 185 tests, 184 passed and one declared Windows exclusion.
