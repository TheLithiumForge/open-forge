export const PipelineCommand = {
  verify: "verify",
  check: "check",
  fast: "check:fast",
  delivery: "check:delivery",
  dotnet: "check:dotnet",
  checks: "ci:checks",
  job: "ci:job",
  docs: "docs:build",
} as const;
export type PipelineCommand = (typeof PipelineCommand)[keyof typeof PipelineCommand];

export const CiCommand = {
  reuse: "ci:reuse",
  prepare: "ci:prepare",
  bundle: "ci:bundle",
  watch: "ci:watch",
  timings: "ci:timings",
  smoke: "smoke:public",
  wsl: "gate:wsl",
} as const;

export const LeafCommand = {
  setup: "setup",
  dist: "dist",
  native: "build:native",
  testBuilt: "test:built",
  pack: "pack",
  format: "format:check",
  typecheck: "typecheck",
  lint: "lint",
  deliveryTypes: "typecheck:delivery",
  deliveryLint: "lint:delivery",
  deliveryFormat: "format:delivery:check",
  deliveryTests: "test:delivery",
  packageLayout: "test:package-layout",
  dotnetWhitespace: "check:dotnet:whitespace",
  dotnetStyle: "check:dotnet:style",
  docs: "check:docs",
} as const;

export const PipelineFlag = {
  rid: "--rid",
  offline: "--offline",
  noRestore: "--no-restore",
  plan: "--plan",
  log: "--log",
} as const;
export const NpmCommand = { run: "run", install: "ci", build: "build", typecheck: "typecheck", separator: "--" } as const;
