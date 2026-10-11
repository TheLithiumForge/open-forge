export const DistTagCommand = { command: "dist-tag", add: "add" } as const;
export const LatestTag = "latest";

export function distTagArguments(name: string, version: string, tag: string): string[] {
  return [DistTagCommand.command, DistTagCommand.add, `${name}@${version}`, tag];
}
