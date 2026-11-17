import type { SupportedRuntime } from "../../package-model.ts";

// One independent oracle for the external wheel tag format and accepted platform floors.
export const ExpectedWheelTag = {
  "win-x64": "py3-none-win_amd64",
  "win-arm64": "py3-none-win_arm64",
  "osx-x64": "py3-none-macosx_15_0_x86_64",
  "osx-arm64": "py3-none-macosx_15_0_arm64",
  "linux-x64": "py3-none-manylinux_2_39_x86_64",
  "linux-arm64": "py3-none-manylinux_2_39_aarch64",
} as const satisfies Record<SupportedRuntime, string>;

export const WheelFixture = {
  version: "0.9.0-beta.18",
  pythonVersion: "0.9.0b18",
  native: Buffer.from("owned inert executable fixture"),
  license: Buffer.from("owned license fixture\n"),
} as const;
