---
open-forge:
  description: Reproduce useful CLI scenarios with ordinary files and independently checked state
  tags: [Memory, Document, CLI, Fixture]
---

# Experience Fixtures

Create each independent scenario under an isolated temporary root. Redirect the CLI data home into that root too. Within a flow carry actual resulting state between steps; branch only from an identified checkpoint. Use public commands where their installation or mutation is the behavior being checked.

| Fixture | Required state |
| --- | --- |
| W0 | Empty ordinary workspace plus a distinctive README whose bytes must survive |
| W1 | Verified public Core install from the binary being tested; derive inventory from its actual source, never the older pack count |
| ROUTES | W1 with ordinary unowned guidance and patterns sources, valid parent entrypoints, and known current lists |
| SEARCH | ROUTES with independently chosen tag intersections/unions, headings and excluded decoys |
| LINKS | ROUTES with authored incoming/outgoing local links and independently identified destination spans |
| TEMPLATE | A routed Template with distinctive metadata and body plus a separate destination |
| PACKAGES | External catalogue beside the consumer; custom toolkit/base packages with pinned manifests and real content. The bundled development-toolkit is dependency-only and is not this fixture |
| VERSION-PAIR | Two deliberate source versions with known additions, changes and retirements; a Framework update needs two genuine builds |
| LIB | Real source directory within the scratch workspace, destination route, registration and raw relative file symlinks created by the public CLI |
| PERMISSIONS | Package targeting an exact external path such as docs/team.md; grants for docs/team do not cover that file |
| OWNERSHIP-VARIANTS | Separate known-empty, absent, damaged and inaccessible records; none may silently impersonate another |
| TWO-WORKSPACES | Distinct installed A and damaged B, plus an uninstalled child; verify explicit workspace selection |
| LITERAL-CONTENT | Unicode, code fences, mixed line endings and a file without a final newline; retain exact original bytes |
| REPEATABLE | Actual successful prior state followed by the identical command, without resetting |
| VIEW-VARIANTS | Identical starting copies for mutating presentation variants; unchanged shared state only for read-only calls |
| COLLISION | Two sources with independently verified identical automatic IDs under current source-reference rules; retain their distinct physical paths |
| PACKAGES-INSTALLED | PACKAGES after public installation; independently verify actual ownership claims and destination bytes |
| PACKAGE-SCAFFOLD | Writable external catalogue parent with an absent selected package directory; inspect the actual path returned by creation |
| LIB-UNATTACHED | W1 with a real contained source directory and an existing routed destination; leaves are unoccupied and no registration exists |

## Input Examples

The supplied small authored examples are retained under [docs/cli-experience-fixtures](../../../../../../docs/cli-experience-fixtures/README.md). They are test input candidates, not a new automated test suite. Preserve native Skill metadata and support bytes. Do not copy the supplied old Framework tree into current fixtures.

## Capability-Dependent Cases

A live lock requires an actual writer, not a lock filename. An after-effect failure needs a reproducible external boundary and observed prior effect. A valid recovery bundle must come from a supported operation or a verified fixture, not a fabricated ZIP. Inaccessible-file scenarios need an independently proven access failure. Real terminal approval requires a terminal, not redirected input presented as interactive.

If these conditions cannot be established, report fixture-unavailable or not-run. Neither is a product failure or a passing result. Never kill unrelated processes, change real user settings, or execute these mutations in the development checkout.

| Named fixture | Required proof |
| --- | --- |
| READ-DENIED | Independently demonstrate that the CLI account cannot read the selected required input; malformed or missing input is different |
| LOCK | A test-created live process owns the actual workspace mutation lock; record acquisition, contention and release |
| RECOVERY | A valid supported recovery bundle or draft for this workspace, produced by an observed operation or a reviewed reproducible recipe |
| FAULT-AFTER-EFFECT | One verified real effect followed by a controlled filesystem or process failure at a known boundary |
| PUBLICATION-FAILURE | Verified content effects precede failed ownership publication; record subsequent management behavior separately |
| CANCEL-BEFORE-EFFECT | Real cancellation at an observed pre-effect boundary, with unchanged files and relevant isolated stores |
| RETAINED-AFTER-SUCCESS | All intended effects complete before independently verified recovery cleanup failure; generic permutations remain deferred |
| CATALOGUE-CHANGED | An observed candidate-set change between selection and guarded revalidation without bypassing the real lock |
| UNEXPECTED-FAILURE | A reproducible condition reaching an unexpected failure, rather than relabelled validation failure; no executable seed is supplied |
| SUPPLIED-CAPTURES | Historical text for provenance only; it cannot establish current state or substitute for a fresh execution |

Scenario commands use named bindings such as `$CAT`, `$WS_A`, `$LOCATION`, `$EXPECTED`, and `$TARGET`. Resolve them from the verified fixture, record actual values, and execute argument arrays. Preserve empty strings and spaces. A scenario mentioning ownership or a blocked input does not itself require a live-lock fixture; apply capability requirements only when the selected starting state needs them.
