---
open-forge:
  description: Diagnose Windows terminal and lock helper deadlines under CPU contention and retain reproducible validation
  tags: [Memory, Observation, Contextual, Candidate, CLI, Testing, Performance]
---

# Windows terminal test deadlines

## Scope and applicability

The 2026-10-12 request authorizes diagnosis and repair of intermittent Windows
EndToEnd deadlines from base `dcf76bfad`, repeated affected classes alone and
under CPU contention, and the full managed EndToEnd suite. Leave edits
uncommitted for integration. No push, other-worktree mutation, dependencies or
product behavior changes are authorized by this repair.

This is a reversible test-harness change for a local development tool. Fixtures
own their workspaces and storage. Standard BCL marker tasks, cancellation and
bounded process waits suffice. Reuse the existing terminal transport and lock
protocol. Exceptional machinery: none added. EndToEnd proves the delivered
conversations and live-lock journeys. Test-only changes do not trigger Native
AOT qualification, and Windows Native AOT requires the unsandboxed host under
the [Architecture](../../crystallized/documents/cli/architecture.md).

## Causes and measurements

The terminal harness charged all prompts, redraws, input writes and final exit
to one 30-second deadline. A healthy conversation took approximately 1.4 seconds
alone, and approximately 24 seconds under 48 CPU-burning threads at both sizes.
With 72 threads, the 80-column application reached confirmation at 28.55 seconds
and then failed the original deadline. This reproduces the failure in a wide
layout. Its longest individual prompt wait was 10.34 seconds.

The lock helper waited for the correct `LOCKED` event, but its 10-second startup
guard was too short under contention. Startup rose from approximately 190 ms
alone to 9.25 seconds with 72 threads. With 96 threads, healthy startup exceeded
15 seconds before the same exclusive-lock and mutation assertions passed.
The reported changed-input failure occurred before product retry revalidation.

A temporary probe rendered a 25-row selection with 120-character summaries
1,000 times at each size after 100 warmup frames. 80x24 took 257.59 ms, worst
single frame 0.78 ms. 40x12 took 74.82 ms, worst 0.16 ms. Narrow rendering was
cheaper, and complete published conversations showed no narrow-layout penalty.
No product optimization is justified by these measurements.

The contention fixtures consumed approximately 11.3 to 11.6 logical CPUs on a
12-CPU host. Timing observations prove scheduling sensitivity, not a general
performance guarantee for every terminal or machine.

## Repair

Each expected terminal prompt, input write and final process exit now receives
its own 60-second hang guard. A preceding successful prompt no longer consumes
the next prompt's budget. Partial output does not reset a guard.

The lock helper retains explicit `LOCKED` and `RELEASED` protocol waits, with a
60-second bound and startup cancellation from the test. Sixty seconds gives
several times the measured worst individual waits. Exit checks, cleanup,
exclusive-lock checks and all journey assertions remain present.

Terminal event timings, conversation duration, lock startup timing and timeout
stage plus inner failure remain in test diagnostics. No retries or skips were
introduced, and no assertion was weakened.

## Reproduction

Commands run from the task worktree with .NET SDK 10.0.101, Release managed tests
and the managed `open-forge-dev` publication. Initialize the directories first.
The worktree-local NuGet folder must exist before restore.

```powershell
$env:OPENFORGE_DATA_HOME = Join-Path $PWD 'artifacts/data-home'
$env:APPDATA = Join-Path $PWD 'artifacts/appdata'
$env:MSBUILDDISABLENODEREUSE = '1'
New-Item -ItemType Directory -Force -Path $env:OPENFORGE_DATA_HOME, "$env:APPDATA/NuGet", 'artifacts/empty-feed' | Out-Null
Copy-Item -LiteralPath NuGet.Config -Destination "$env:APPDATA/NuGet/NuGet.Config"
dotnet restore OpenForge.Cli.slnx --source (Join-Path $PWD 'artifacts/empty-feed') -p:NuGetAudit=false
dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --no-restore -c Release -p:NuGetAudit=false -p:UseSharedCompilation=false -nr:false -m:1
```

The default build failed writing worktree outputs. Disabling build-node reuse
and shared compilation produced warning-free builds. This was an environment
setup issue, separate from the test deadlines.

Exact isolated repetition command:

```powershell
foreach ($run in 1..3) {
    $report = "artifacts/final-alone-$run"
    & artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests.exe --filter-class '*PublishedSelectionViewportProcessTests' --filter-class '*F20WorkspaceLockJourneyTests' --parallel none --minimum-expected-tests 10 --fail-warns on --fail-skips on --output Detailed --show-stdout All --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory $report > "$report.log" 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Failed repetition $run" }
}
```

The contention repetitions use the same command with `--parallel collections`
and `artifacts/final-contention-$run` report names. Run this owned CPU fixture in
a separate shell alongside the tests. Its arithmetic is deliberately unrelated
to the product. The first two repetitions used 96 workers. The third began with
96 and continued with 48 after the first fixture's bounded lifetime expired.
The continuation waited for the owned first process to exit before starting.

```powershell
Add-Type -TypeDefinition @'
using System;
using System.Diagnostics;
using System.Threading;
public static class OwnedCpuContention {
    public static void Run(int workers, int seconds) {
        long end = Stopwatch.GetTimestamp() + (long)(seconds * (double)Stopwatch.Frequency);
        var threads = new Thread[workers];
        for (int i = 0; i < workers; i++) {
            threads[i] = new Thread(() => {
                double value = 1;
                while (Stopwatch.GetTimestamp() < end) {
                    for (int n = 0; n < 10000; n++) value = Math.Sqrt(value + n);
                }
                GC.KeepAlive(value);
            });
            threads[i].IsBackground = true;
            threads[i].Start();
        }
        Console.WriteLine("CPU contention started: PID=" + Process.GetCurrentProcess().Id + ", " + workers + " workers, " + seconds + " seconds");
        var watch = Stopwatch.StartNew();
        using (var process = Process.GetCurrentProcess()) {
            while (Stopwatch.GetTimestamp() < end) {
                Thread.Sleep(10000);
                Console.WriteLine("Contention: wall=" + watch.Elapsed.TotalSeconds.ToString("F1") + "s CPU=" + process.TotalProcessorTime.TotalSeconds.ToString("F1") + "s");
            }
        }
        foreach (var thread in threads) thread.Join();
    }
}
'@
[OwnedCpuContention]::Run(96, 900)
```

Baseline contention runs used `(48, 240)` and `(72, 240)`. The final continuation
used `(48, 600)`. Stop only the owned fixture after testing.

Exact first full managed EndToEnd command:

```powershell
& artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests.exe --parallel collections --minimum-expected-tests 1 --fail-warns on --fail-skips off --no-ansi --progress off --output Detailed --show-stdout All --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/final-full > artifacts/final-full.log 2>&1
```

The final full run used the identical invocation with results directory
`artifacts/final-full-current` and log `artifacts/final-full-current.log`.

## Execution receipt

Unchanged baseline: 10/10 alone, 10/10 with 48 workers, 9/10 with 72 workers.
The failure was the 80-column application at the original process deadline.

Final isolated runs: 10/10 each, 30/30 total. Durations 30.389, 42.196 and
30.857 seconds. Zero failures, skips or warnings. Each run includes all seven
viewport cases and all three F20 lock journeys.

Final stress runs: 10/10 each, 30/30 total, in 6m10.075s, 6m00.656s and
3m31.581s. Zero failures, skips or warnings. Healthy terminal conversations
reached 51.224 seconds, the longest prompt wait was 14.074 seconds, and `LOCKED`
reached 15.223 seconds. The 60-second guards provide approximately four times
the measured worst individual event. Across isolated and
stress runs, the viewport class passed 42/42 and the lock class passed 18/18.

The full managed EndToEnd suite passed 330/330 in 3m34.033s. A subsequent
comment-only correction changed "below 10" to "around 10" seconds. The
refreshed build passed without warnings, all three stress repetitions consumed
that build, and its final full suite passed 330/330 in 5m52.006s. Both full runs
had zero failures, skips or warnings. Formatting verification and diff checks
passed. There were no further code or configuration changes.

Full-suite test assembly SHA-256:
`79C40B239469E406485FF6C9E286766DFCE2030BBC9010EA5651B973E26ABEE2`.
Refreshed test assembly SHA-256:
`698A68E495F14F959C9F203C0D5826454A61B36B163A093752EC44B0FA1B0082`.
Managed CLI assembly SHA-256:
`9BD4DA81828D65FF366312167281F1FBAE251BA5D4F4C5BB60F7BFD7831FD206`.

Final `PublishedWindowsTerminal.cs` SHA-256:
`1EF706F32EA4A617C3DB10EEE6B6933405CC66213CA6D01D0903F1F6E37B180D`.
Final `PublishedLockHolder.cs` SHA-256:
`049D714205A402E195F90BAEAF34086736CDBD080DBBAE0E24A1A572426463F7`.
Unchanged `Directory.Build.props` SHA-256:
`4C636ECF1BAA554A10D88497EA93B6EA3E8C086263CE84A4E616B19FBCF228A6`.
Unchanged `Directory.Packages.props` SHA-256:
`01AE27218AC887C281D279A57D552E823F74544464A583DC95998AE23807FCA3`.
Unchanged `global.json` SHA-256:
`15FC2962D00BBB3FEBAE9DDB01301F0723EBB743E1CFCD57AF3D1FDD3FB640BB`.

Raw logs and CTRF reports remain in the named ignored `artifacts/` locations.
This record retains reproduction and conclusions independently of those logs.
The combined owned-fixture stop, recursive probe cleanup and final-suite command
was rejected by automatic approval review with `blocked by policy`. None of that
command executed. The load fixture finished its bounded lifetime normally.
The temporary probe remains under ignored `artifacts/layout-probe`,
`artifacts/bin/Probe` and `artifacts/obj/Probe`. Do not treat its source as required
test support. The final full-suite run was then launched separately and passed.

## Limits

The repair is qualified on Windows managed execution. Native AOT was not run.
A machine that cannot reach an event for 60 seconds still fails intentionally.
The measurements do not identify a product layout hot path or justify caching
or replacing the existing terminal transport.
