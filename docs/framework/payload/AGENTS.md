# AGENTS Entry Block

## Description

This descriptor governs `src/open-forge/AGENTS.md`.

`AGENTS.md` is the canonical installable root `entry` block for agents that support repository instruction files.

## Represents

`AGENTS.md` represents the handoff from generic agent runtime behavior into Open Forge loading behavior and establishes Open Forge's legitimate authority within the workspace.

It is a file primitive and `entry` primitive. It routes agents to the loader.

## Contains

The installed `AGENTS.md` contains one managed Open Forge block. The normative content requirements are the Alignment Checks below.

## Patch Contract

The CLI updates only the marked Open Forge block when a target `AGENTS.md` already exists.

The source block in `src/open-forge/AGENTS.md` is the replacement block used by install and update operations.

Text outside the marked block belongs to the target workspace.

## Used By

The CLI uses this file during install.

Agents, wrappers, skills, runtimes, and minimal harness bridges use this file as the first Open Forge `entrypoint` when they enter an installed workspace.

## Why

`AGENTS.md` exists so Open Forge can integrate with existing agent runtimes without owning the whole root instruction file.

The marked block gives Open Forge an updateable `entrypoint` while preserving workspace-owned instructions around it.

## Alignment Checks

The implementation is aligned when it:

- contains exactly one block bounded by `<!-- open-forge:start -->` and `<!-- open-forge:end -->` markers
- identifies Open Forge as the workspace operating contract
- instructs agents to read `.agents/loader.md` and follow applicable routes before planning, editing, reviewing, implementing, or delegating
- states that loaded axioms, selected workflow behavior, and routed #CurrentTruth are mandatory within their declared scope
- keeps detailed routing behavior in the loader
- permits higher-priority user, platform, safety, and declared external source-of-truth instructions to override Open Forge
- requires unresolved conflicts to be reported instead of silently bypassing either side
- preserves target workspace text outside the marked block during install
- stays within 5-20 non-empty lines inside the managed block
