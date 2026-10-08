import assert from "node:assert/strict";
import { test } from "node:test";
import { qualifyReport } from "../test-report.ts";

function report(status = "passed") {
  return {
    reportFormat: "CTRF",
    results: { summary: { tests: 1, passed: 1, failed: 0, skipped: 0, pending: 0, other: 0 }, tests: [{ status }], extra: { suites: [{ errors: [] }] } },
  };
}

test("standard reports qualify positive complete results without a global inventory", () => {
  assert.deepEqual(qualifyReport(report()), { passed: 1, skipped: 0 });
});

test("empty, incomplete, skipped and error-bearing reports cannot qualify", () => {
  const empty = report();
  empty.results.summary.tests = 0;
  assert.throws(() => qualifyReport(empty));
  const incomplete = report();
  incomplete.results.tests = [];
  assert.throws(() => qualifyReport(incomplete));
  assert.throws(() => qualifyReport(report("skipped")));
  assert.throws(() => qualifyReport({}));
  const failed = report();
  failed.results.summary.failed = 1;
  assert.throws(() => qualifyReport(failed));
});

function platformReport(message: string, type = "OpenForge.Cli.IntegrationTests.Commands.Install.InstallBeforeOutputSnapshotTests") {
  return {
    reportFormat: "CTRF",
    results: {
      summary: { tests: 2, passed: 1, failed: 0, skipped: 1, pending: 0, other: 0 },
      tests: [{ status: "passed" }, { status: "skipped", name: "OS boundary", message, extra: { type } }],
      extra: { suites: [{ errors: [] }] },
    },
  };
}

test("OS exclusions qualify only outside their evidence platform and stay counted separately", () => {
  const windows = platformReport("This deterministic replacement failure requires Windows file sharing.");
  assert.deepEqual(qualifyReport(windows, "linux"), { passed: 1, skipped: 1 });
  assert.deepEqual(qualifyReport(windows, "darwin"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(windows, "win32"));
  assert.throws(() => qualifyReport(windows));
  const unix = platformReport("This evidence requires Unix file permissions.");
  assert.deepEqual(qualifyReport(unix, "win32"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(unix, "linux"));
  assert.throws(() => qualifyReport(unix, "darwin"));
  const linux = platformReport("Required permission evidence targets Linux.");
  assert.deepEqual(qualifyReport(linux, "darwin"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(linux, "linux"));
});

test("unknown, capability, inconsistent and entirely skipped reports still fail", () => {
  assert.throws(() => qualifyReport(platformReport("An unexpected dependency is unavailable."), "linux"));
  assert.throws(() => qualifyReport(platformReport("Creating a file symbolic link requires privileges this host does not grant."), "win32"));
  const inconsistent = platformReport("This deterministic replacement failure requires Windows file sharing.");
  inconsistent.results.summary.skipped = 0;
  inconsistent.results.summary.passed = 2;
  assert.throws(() => qualifyReport(inconsistent, "linux"));
  const allSkipped = platformReport("This deterministic replacement failure requires Windows file sharing.");
  allSkipped.results.tests.shift();
  allSkipped.results.summary.tests = 1;
  allSkipped.results.summary.passed = 0;
  assert.throws(() => qualifyReport(allSkipped, "linux"));
  const wrongSuite = platformReport("This deterministic replacement failure requires Windows file sharing.");
  const skipped = wrongSuite.results.tests[1];
  assert.ok(skipped?.extra);
  skipped.extra.type = "OpenForge.Cli.EndToEndTests.Journeys";
  assert.throws(() => qualifyReport(wrongSuite, "linux"));
});

test("public Windows journeys are exclusions only on Unix, never lost required evidence", () => {
  const report = platformReport("F03 X05 read-denial evidence requires Windows file-sharing semantics.");
  const skipped = report.results.tests[1];
  assert.ok(skipped?.extra);
  skipped.extra.type = "OpenForge.Cli.EndToEndTests.Journeys.F03PlainNoteJourneyTests";
  assert.deepEqual(qualifyReport(report, "linux"), { passed: 1, skipped: 1 });
  assert.deepEqual(qualifyReport(report, "darwin"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(report, "win32"));
  skipped.message = "ConPTY failed to initialize on this host.";
  assert.throws(() => qualifyReport(report, "win32"));
  assert.throws(() => qualifyReport(report, "linux"));
});

test("Update confirmation journeys require Windows ConPTY evidence and exclude only Unix", () => {
  const report = platformReport("This confirmation journey requires the repository's Windows ConPTY harness.");
  const skipped = report.results.tests[1];
  assert.ok(skipped?.extra);
  skipped.extra.type = "OpenForge.Cli.EndToEndTests.PublishedUpdateAdoptionProcessTests";
  assert.deepEqual(qualifyReport(report, "linux"), { passed: 1, skipped: 1 });
  assert.deepEqual(qualifyReport(report, "darwin"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(report, "win32"));
  assert.throws(() => qualifyReport(report));
});

const producerTypes = {
  installSharing: "OpenForge.Cli.IntegrationTests.Commands.Install.InstallRouteSharingIntegrationTests",
  lifecycle: "OpenForge.Cli.IntegrationTests.Commands.Library.Shared.GitIgnore.LibraryGitIgnoreLifecycleIntegrationTests",
  application: "OpenForge.Cli.IntegrationTests.Commands.Library.Shared.GitIgnore.LibraryGitIgnoreApplicationIntegrationTests",
  libraryChoice: "OpenForge.Cli.EndToEndTests.PublishedLibraryGitIgnoreProcessTests",
  selectionViewport: "OpenForge.Cli.EndToEndTests.PublishedSelectionViewportProcessTests",
  installFrontmatterQuestion: "OpenForge.Cli.EndToEndTests.Commands.Install.PublishedInstallFrontmatterProcessTests",
} as const;

const declaredWindowsProducers = [
  {
    type: producerTypes.installSharing,
    reason: "This write refusal requires Windows file sharing enforcement.",
    wrongSuite: producerTypes.libraryChoice,
  },
  {
    type: producerTypes.lifecycle,
    reason: "This owned-file read denial requires Windows file sharing.",
    wrongSuite: producerTypes.libraryChoice,
  },
  {
    type: producerTypes.application,
    reason: "This deterministic replacement denial requires Windows file sharing.",
    wrongSuite: producerTypes.libraryChoice,
  },
  {
    type: producerTypes.libraryChoice,
    reason: "Library terminal choice evidence requires Windows ConPTY.",
    wrongSuite: producerTypes.lifecycle,
  },
  {
    type: producerTypes.selectionViewport,
    reason: "Selection viewport terminal evidence requires Windows ConPTY.",
    wrongSuite: producerTypes.lifecycle,
  },
  {
    type: producerTypes.installFrontmatterQuestion,
    reason: "This selection journey requires the repository's Windows ConPTY harness.",
    wrongSuite: producerTypes.lifecycle,
  },
] as const;

const unixPlatforms = ["linux", "darwin"] as const;
const rejectionPlatforms = [...unixPlatforms, "win32", undefined] as const;
const capabilityFailures = ["ConPTY failed to initialize on this host.", "Windows file sharing could not be established on this host."];
const unitSuite = "OpenForge.Cli.UnitTests.ReleaseQualificationTests";

for (const producer of declaredWindowsProducers) {
  test(`${producer.type} declares a Windows exclusion that qualifies on Linux and macOS`, () => {
    for (const platform of unixPlatforms) {
      assert.deepEqual(qualifyReport(platformReport(producer.reason, producer.type), platform), { passed: 1, skipped: 1 });
    }
  });

  test(`${producer.type} keeps owning-platform, suite and exact-reason gates strict`, () => {
    const report = platformReport(producer.reason, producer.type);
    assert.throws(() => qualifyReport(report, "win32"));
    assert.throws(() => qualifyReport(report));
    for (const platform of rejectionPlatforms) {
      assert.throws(() => qualifyReport(platformReport(producer.reason, producer.wrongSuite), platform));
      assert.throws(() => qualifyReport(platformReport(producer.reason, unitSuite), platform));
      for (const reason of [`${producer.reason} Extra qualifier.`, ...capabilityFailures]) {
        assert.throws(() => qualifyReport(platformReport(reason, producer.type), platform));
      }
    }
  });
}
