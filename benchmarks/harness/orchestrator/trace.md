# Observable Trace Contract

The orchestrator is the trace viewer for each worker arm. Its evidence boundary is the complete interaction surface the runtime actually exposes, not invisible model state.

## Required Layout

```text
record/trace/
  manifest.json
  raw/
    <runtime-native artifacts>
```

`raw/` preserves the runtime-native export when one exists. When a runtime exposes events live but offers no export, retain those events verbatim and in order in the least lossy available format. Record that capture method and its limits in the manifest.

Do not replace raw events with an orchestrator summary. Summaries belong in the orchestrator review and cite the trace they interpret.

## Observable Content

Preserve every event available to the orchestrator during the arm:

- Worker, orchestrator, user, and persona messages.
- Tool calls, arguments, results, failures, retries, and approvals.
- Worker questions and the exact answers returned.
- Runtime status, lifecycle, interruption, and completion events.
- Reasoning summaries or analysis text explicitly exposed by the runtime.
- Model, runtime, settings, worker-context identifier, and actual tool binding when available.

Ordering and failed events are evidence. Keep timestamps or sequence identifiers supplied by the runtime.

Private chain-of-thought, hidden activations, and other model state not exposed by the runtime are not trace. Do not infer or claim them. An exposed reasoning summary is evidence of that summary only, not a guarantee that it is the model's complete internal reasoning.

## Manifest

`manifest.json` declares what the raw artifacts can and cannot support. Keep it small and factual.

```json
{
  "runtime": "runtime and version",
  "model": "actual resolved model",
  "workerContext": "runtime identifier or unknown",
  "captureMethod": "native export or verbatim live capture",
  "startedAt": "timestamp or unknown",
  "endedAt": "timestamp or unknown",
  "observableSurfaces": ["messages", "tool calls", "tool results", "status events"],
  "requestedRuntimeTools": [],
  "observedRuntimeTools": [],
  "knownGaps": ["private chain-of-thought is not exposed"],
  "rawArtifacts": ["raw/events.jsonl"]
}
```

Use actual values and paths; the example is not a claim about a particular runtime. If an event class was visible but could not be retained completely, name that loss under `knownGaps`. If a requested model or tool binding cannot be verified, record it as unknown and narrow the review.

## Local And Runtime Tools

A primitive with `kind: tool` delivers ordinary local-tool files in the workspace and may include a `.agents/workspace/**` route that makes them discoverable. Its files and worker use appear through normal workspace and trace evidence.

A provider- or runtime-native tool exists only when the runtime binds it to that worker. The launch prompt freezes the requested surface, while the manifest records the actual observed binding and evidence. The runner must not present a requested runtime tool as provisioned merely because it was named.

When runtime tool availability differs between comparison arms, declare it as a treatment delta before launch. Do not hide it inside an orchestrator addendum.

## Review Use

Behavior findings cite observable trace artifacts. Outcome findings cite the frozen workspace, delta, and independent checks. Missing trace evidence leaves the related behavior unknown even when the outcome is inspectable.

`finish` requires a completed, parseable trace manifest. Raw artifacts may be absent when the runtime exposed none, but `knownGaps` must make that boundary explicit.

Keep each arm's trace separate. A run-set comparison consumes completed per-arm reviews and may cite their traces, but it does not merge traces into a shared worker context or expose one arm to another.
