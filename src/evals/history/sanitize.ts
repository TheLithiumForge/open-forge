export const Redacted = { path: "[machine-path]", user: "[user]", account: "[account]" } as const;
const privateKeys = new Set(["username", "user", "sessionid", "hostname", "homedir", "cwd", "machinepath", "sourcepath", "credential", "credentials", "apikey", "token", "authorization"]);

export interface SanitizingOptions {
  readonly userNames?: readonly string[];
}

function findUserNames(value: unknown, names: Set<string>): void {
  if (typeof value === "string") {
    for (const match of value.matchAll(/(?:[\\/]Users[\\/]|[\\/]home[\\/])([^\\/\s"']+)/gi)) if (match[1] !== undefined) names.add(match[1]);
  } else if (Array.isArray(value)) {
    for (const child of value) findUserNames(child, names);
  } else if (value !== null && typeof value === "object") {
    for (const [key, child] of Object.entries(value)) {
      if (/^(?:user|username)$/i.test(key) && typeof child === "string") names.add(child);
      findUserNames(child, names);
    }
  }
}

export function sanitizeRecords(value: unknown, options: SanitizingOptions = {}): unknown {
  const names = new Set(options.userNames ?? []);
  findUserNames(value, names);
  const scrub = (input: unknown): unknown => {
    if (typeof input === "string") {
      let result = input.replace(/(?:[a-z]:[\\/]|\\\\)[^\s"'<>|)\]}]+/gi, Redacted.path)
        .replace(/(?<![\w:/])\/(?!\/)[^\s"'<>|)\]}]+/g, Redacted.path);
      for (const name of names) {
        if (name.length === 0) continue;
        const escaped = name.replaceAll(/[.*+?^${}()|[\]\\]/g, "\\$&");
        result = result.replace(new RegExp(`(?<![\\w-])${escaped}(?![\\w-])`, "gi"), Redacted.user);
      }
      return result;
    }
    if (Array.isArray(input)) return input.map(scrub);
    if (input !== null && typeof input === "object") {
      return Object.fromEntries(Object.entries(input).filter(([key]) => !privateKeys.has(key.replaceAll(/[_-]/g, "").toLowerCase()))
        .map(([key, child]) => [key, scrub(child)]));
    }
    return input;
  };
  return scrub(value);
}
