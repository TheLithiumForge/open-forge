---
open-forge:
  description: Review and admit tool recipes, official downloads, pinned runtimes and lifecycle commands
  tags: [Memory, Document, CurrentTruth, Maintenance, Tool, Supply]
---

# Tool admission
Tool admission is a maintenance process that reviews an exact download recipe before an Open Forge release can install it.

For example, changing a release asset's hash creates a new recipe to review. A local manifest with that hash can't bypass release admission. Open Forge distributes recipes and never ships, mirrors or rehosts tool bytes.

Status: V1 supply model decided.

## Source

The [Tool manifest contract](../cli/contracts/shared/tool-manifest/interface.md#supply-recipes) defines the accepted supply rules. The [external-tools design](../cli/technical-designs/external-tools.md) defines stores, process capabilities and the split between workspace and account records. This checklist defines how maintainers review those recipes before release admission.

## Contract

### Review a recipe

Complete these checks before adding a tool or changing a pin:

1. Confirm the upstream project, source commit, release tag, license and notices. Record any use or distribution conditions that affect the recipe. Downloading from upstream doesn't remove its license conditions.
2. Select the exact official source for every target. Native tools use the project's GitHub release asset. Node uses `https://nodejs.org/dist/v<version>/`. Packages and their complete dependency closure use the npm registry. Reject mirrors, floating tags and a fallback to an unrelated source.
3. Record each exact version, URL and byte size. Native and Node archives pin SHA-256. npm packages keep their lockfile SRI values. Record the source commit and the attestation identity where upstream publishes build attestations.
4. For a native asset with an attestation, verify the asset digest, expected repository, source commit and build identity. Record the verification method and evidence. A published attestation that can't be verified blocks admission or installation according to the recipe's declared check.
5. For Node, verify the signature on `SHASUMS256.txt` against the upstream release keys, then pin the portable archive's listed hash. Record the checksum document, signature and signer evidence. Mark a target unsupported when no official archive exists. This includes musl/Alpine.
6. For a Node package, review the pinned `package.json`, complete lockfile, resolved registry URLs, integrity values and npm options. Run the private runtime's bundled npm through that absolute Node path. Use `npm ci` without dependency resolution, lockfile rewriting or package install scripts.
7. Review every install, uninstall and index-cleanup argument vector, timeout, environment override, output path and process scope. Check that the tool's installer registers its MCP server and supplies the required usage Skill. Open Forge edits no agent configuration.
8. Verify disabled self-update checks and default-off telemetry. Test the visible telemetry opt-in and opt-out. Confirm that watching, automatic indexing and hooks follow the declared process policy.
9. Test each admitted target natively. Cover first install, same-pin reinstall, configuration, update, downgrade, cancellation, command failure, removal and another workspace sharing the same store. Use the declared process consent and final-client shutdown rules.
10. Check safe extraction, exact hashes, account-store containment, reference release, removal exclusions and the managed ignore section. Confirm that preview makes the same pre-effect decisions and starts no network requests or processes.
11. Add the exact manifest hash and adapter revision to release support only after review passes. Keep every previously reviewed pin in the catalogue so an exact older pin can be selected. Keep a blocked pin's reason when it can't be installed.

The [Node verification instructions](https://github.com/nodejs/node#verifying-binaries) define upstream checksum verification. The [npm ci reference](https://docs.npmjs.com/cli/v11/commands/npm-ci/) defines frozen lockfile installation. Those upstream procedures provide evidence for a recipe and don't grant Open Forge release admission.

### Reject a recipe

These conditions block installation:

- An unreviewed manifest tuple or unavailable adapter revision.
- A missing official target, source commit, required hash, size or complete dependency lock.
- A URL outside the recipe's approved source and redirect hosts.
- A size, digest, signature or required attestation mismatch.
- An archive with escaping paths, links, special members or duplicate destinations.
- Missing artifact consent, process consent or destination grants.
- A required output covered by a removal exclusion or another package's receipt.
- Unavailable store identity, references, process scope or required native tests.

Readable blocked definitions stay available to Inspect. Never replace failed verification with the user's Node, a PATH lookup, another asset, a mirror or a self-update.

### Keep evidence

Store the reviewed recipe and test evidence with the tool definition. The workspace settings record one pin per tool and its active roles. The workspace lock records the exact recipe, hashes, files, store references and command results. Account-local state records physical bindings, references and process consent.

## Verification

Use synthetic test-only definitions and local hashed artifacts for core tests. Test strict source selection, hashes, extraction, process arguments and environment, record publication, references, consent, preview parity and exclusions. Production recipes also need the matching native journeys and retained attestation or signature evidence from the checklist. CI supplies the full managed and Native AOT gates. Recipe review never redistributes the tested artifact.

## Not decided yet

- Proposed: `open-forge tool exec <tool> -- <args>` runs a pinned tool and runtime from the current directory. It avoids PATH changes and symlinks. It isn't implemented in this branch.
- The maintainer needs to decide whether Choose also switches agent integrations through tool commands or changes only role metadata.
- Unix Node archive preparation needs a decision on exact unused launcher links. The current extraction rule rejects links.
