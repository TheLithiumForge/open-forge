# Tool maintenance

Tool maintenance is the review procedure that admits an external tool and an exact pin into Open Forge's catalogue.

An archive and a valid signature establish release facts. They don't establish installer scope or complete call evidence. Review those separately before admitting a pin.

## Admission checklist

Complete these steps for every tool or pin:

1. Read current issue decisions and contracts. Keep the latest scope, backup and verified-download rules.
2. Resolve the latest stable tag to its full commit. Keep older reviewed pins when adding a default.
3. Select the exact official asset for all six targets. Record URLs, bytes and published SHA-256. Give missing assets a blocked reason.
4. Download every selected archive without executing it. Compare length and hash with release metadata and published checksums.
5. Verify published attestations or Sigstore bundles with exact signer identity and issuer. Keep commands and source-build limitations. A decoded certificate alone isn't verification.
6. Read pinned source and help. List every output, hook, Skill, profile, MCP name, account effect, transient file and removal effect. Resolve actual paths before consent.
7. Declare reviewed lifecycle argument arrays. Pending arguments stay null in a review draft outside production admission.
8. Prepare mandatory guidance with exact build, refresh and query forms, language notes and process limits. Verify its bytes before activation. Settle tool-owned delivery.
9. Verify telemetry defaults to off, updates are off and Open Forge writes trigger no watcher or automatic indexing. Show supported telemetry settings plainly.
10. Test schema, source identity, hashes, target closure, pending commands, guidance and no accidental admission with local fixtures only.
11. After decisions and the core are ready, run the temporary Windows x64 Git journey with project-level hosts. Inventory download, verification, install, Status, external build and Remove. Name agreed leftovers.
12. Require the other five native journeys in CI. Keep installation qualification separate from the frozen accuracy harness. Promote only tested targets.
13. Admit the exact manifest digest and adapter revision, then update the catalogue default. Check docs, help, both CLI Skill copies and links. Run the .NET whitespace and style checks before committing.

The [manifest contract](../.agents/memory/crystallized/documents/cli/contracts/shared/tool-manifest/interface.md) defines production admission. Review drafts use a separate [schema](../schemas/tools/drafts/v1/definition.schema.json). Their `definition` member keeps the production field shape. Pending lifecycle and evidence stay outside it. The production reader rejects the review wrapper.

## CodeGraph example

The [draft](../src/tools/drafts/codegraph-1.6.2.json) pins `v1.6.2` at `6560052a6f856855d3f71eee838fd66ccfa4285d`. All six archives pass size, checksum and build-attestation checks. The signer is the repository's `release.yml` on `refs/heads/main`. Its attested source commit is `0f0af7bf22a5083a47e34fcb1e1bf67355174ef8`. Keep that separate from the tag commit.

R2 leaves Install arguments pending. Uninstall and index cleanup have source-reviewed forms, with timeouts and native evidence pending. The required Skill is drafted. The [consent inventory](tools/codegraph-consent.md) covers project writes, the prompt hook, legacy cleanup and the bare PATH launcher. `.codegraph/` is the proposed ignore entry. No target is admitted.

The portable archive bundles Node 24.16.0. Admission under the official private Node plus locked npm policy still needs review. The draft records that runtime without approving the supply policy.

## Codebase Memory example

The [draft](../src/tools/drafts/codebase-memory-0.12.1.json) pins `v0.12.1` at `a7158ac2f48ca646b4527341b76f930f2aeff87e`. All six selected archives pass size, checksum and individual Sigstore checks. The checksum bundle also verifies. The signer is the repository's `release.yml` on `refs/heads/main`, with GitHub Actions as issuer.

R1 leaves Install and Uninstall arguments pending. Exact-project cleanup has a source-reviewed form. Timeouts and native evidence are pending. The [consent inventory](tools/codebase-memory-consent.md) covers account client setup, instructions, hooks, Skills, profiles and the wider uninstall sweep. The Open Forge Skill is drafted. No ignore entry or qualified target is declared.

## Passive checks

Proposed. A reviewed adapter gives the native MCP name and resolved destinations. CodeGraph uses `codegraph`. Codebase Memory uses `codebase-memory-mcp`. A difference from `open-forge-<id>` isn't drift. Keep registration identity and native server identity separate.

For JSON, use the standard JSON reader at the exact key. For Codex TOML, keep the no-parser constraint. A literal table header can establish only `present-unverified`. It can't establish command values, environment, duplicate-table validity or effective host configuration. Unsupported spelling or unreadable bytes means unavailable coverage. Neither observation authorizes a write or starts a probe.

Inspect should show native names, resolved paths, consent, pending commands and limits. Status should check only recorded hosts and distinguish missing, present-unverified and unavailable evidence. These are proposed changes. This draft keeps the current core validator and host checks.

## Not decided yet

- R1 and R2 are with the maintainer. Keep both pins blocked until reviewed commands reflect those decisions.
- Decide CodeGraph's private Node policy and tool-owned delivery of required guidance.
- Resolve activation paths and native removal evidence before admitting writers.
