/**
 * Plain docs keep every word of the spec but none of the links between its documents, so an agent finds them by name or
 * by listing the folder. A link keeps its text and loses its target. Links to outside sources and code blocks stay.
 */
const Link = /\[([^\]\n]+)\]\(([^)\s]+)\)/g;
const Fence = /^\s*(```|~~~)/;
const outside = (target: string): boolean => /^[a-z][a-z0-9+.-]*:/i.test(target);

export function withoutLinks(bytes: Uint8Array): Buffer {
  let fenced = false;
  const lines = Buffer.from(bytes).toString("utf8").split("\n").map((line) => {
    if (Fence.test(line)) { fenced = !fenced; return line; }
    return fenced ? line : line.replace(Link, (link: string, text: string, target: string) => outside(target) ? link : text);
  });
  return Buffer.from(lines.join("\n"));
}
