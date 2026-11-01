import { posix } from "node:path";
import { safeRelative, type FileBytes } from "./files.ts";
import { splitMarkdown } from "./frontmatter.ts";

function prose(bytes: Uint8Array): string {
  return splitMarkdown(bytes).body.toString("utf8").replace(/^(```|~~~)[^\n]*\n[\s\S]*?^\1[^\n]*$/gm, "");
}

function headingIds(bytes: Uint8Array): Set<string> {
  const used = new Map<string, number>();
  const ids = new Set<string>();
  for (const heading of prose(bytes).matchAll(/^#{1,6}\s+(.+?)\s*#*\r?$/gm)) {
    const title = heading[1];
    if (title === undefined) continue;
    const slug = title.toLowerCase().replace(/<[^>]*>/g, "").replace(/[^\p{L}\p{N}\p{M}\s_-]/gu, "").trim().replace(/\s/g, "-");
    const count = used.get(slug) ?? 0;
    ids.add(count ? `${slug}-${count}` : slug);
    used.set(slug, count + 1);
  }
  return ids;
}

export function validateLinks(files: readonly FileBytes[]): void {
  const paths = new Map(files.map((file) => [file.path, file.bytes]));
  for (const file of files) {
    const text = prose(file.bytes);
    const inline = /!?\[[^\]]*\]\(\s*(?:<([^>]+)>|([^\s)]+))(?:\s+"[^"]*")?\s*\)/g;
    const references = /^\s*\[[^\]]+\]:\s*(?:<([^>]+)>|(\S+))/gm;
    for (const match of [...text.matchAll(inline), ...text.matchAll(references)]) {
      const link = match[1] ?? match[2];
      if (!link || /^[a-z][a-z\d+.-]*:/i.test(link)) continue;
      const raw = link.split(/[?#]/)[0] ?? "";
      const decoded = decodeURIComponent(raw);
      if (decoded.startsWith("/")) throw new Error(`Absolute file link in ${file.path}: ${link}`);
      let target = raw ? posix.normalize(posix.join(posix.dirname(file.path), decoded)) : file.path;
      safeRelative(target);
      if (!paths.has(target)) target = `${target.replace(/\/$/, "")}/README.md`;
      const bytes = paths.get(target);
      if (!bytes) throw new Error(`Broken relative link in ${file.path}: ${link}`);
      const fragment = link.includes("#") ? decodeURIComponent(link.slice(link.indexOf("#") + 1)) : "";
      if (fragment && target.endsWith(".md") && !headingIds(bytes).has(fragment)) throw new Error(`Broken heading link in ${file.path}: ${link}`);
    }
  }
}
