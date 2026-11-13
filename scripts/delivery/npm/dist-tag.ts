export const DistTagCommand = { command: "dist-tag", add: "add", remove: "rm" } as const;
export const LatestTag = "latest";

export function distTagArguments(name: string, version: string, tag: string): string[] {
  return [DistTagCommand.command, DistTagCommand.add, `${name}@${version}`, tag];
}

export function removeDistTagArguments(name: string, tag: string): string[] {
  return [DistTagCommand.command, DistTagCommand.remove, name, tag];
}
