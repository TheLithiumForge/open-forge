function normalized(text: string): { text: string; starts: number[]; ends: number[] } {
  let value = "", offset = 0;
  const starts: number[] = [], ends: number[] = [];
  for (const char of text) {
    const end = offset + char.length;
    const mapped = /\s/u.test(char) ? " " : /[“”„‟«»]/u.test(char) ? '"' : /[‘’‚‛ʼ]/u.test(char) ? "'" : /[‐‑‒–—―−﹘﹣－]/u.test(char) ? "-" : char;
    if (mapped === " " && (!value || value.endsWith(" "))) {
      if (value) ends[ends.length - 1] = end;
    } else {
      value += mapped;
      for (let unit = 0; unit < mapped.length; unit += 1) { starts.push(offset); ends.push(end); }
    }
    offset = end;
  }
  if (value.endsWith(" ")) { value = value.slice(0, -1); starts.pop(); ends.pop(); }
  return { text: value, starts, ends };
}

export function originalQuote(message: string, quote: string): string | null {
  const source = normalized(message), target = normalized(quote).text;
  if (!target) return null;
  const index = source.text.indexOf(target);
  const start = source.starts[index], end = source.ends[index + target.length - 1];
  return index < 0 || start === undefined || end === undefined ? null : message.slice(start, end);
}
