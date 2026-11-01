export const ReplyEvidenceBytes = 64 * 1024;

export function capReply(reply: string): string {
  return new TextDecoder().decode(Buffer.from(reply).subarray(0, ReplyEvidenceBytes), { stream: true });
}

export function replyJson(reply: string): unknown {
  const candidates: string[] = [], stack: string[] = [];
  let start = 0, quoted = false, escaped = false;
  // Locate one outer value without interpreting prose or choosing between competing replies.
  for (let index = 0; index < reply.length; index += 1) {
    const char = reply[index];
    if (!stack.length) {
      if (char === "{" || char === "[") { start = index; stack.push(char); }
      continue;
    }
    if (quoted) {
      if (escaped) escaped = false;
      else if (char === "\\") escaped = true;
      else if (char === '"') quoted = false;
    } else if (char === '"') quoted = true;
    else if (char === "{" || char === "[") stack.push(char);
    else if (char === "}" || char === "]") {
      const opening = stack.pop();
      if (opening !== (char === "}" ? "{" : "[")) throw new Error("The reply has mismatched JSON delimiters.");
      if (!stack.length) candidates.push(reply.slice(start, index + 1));
    }
  }
  const candidate = candidates[0];
  if (stack.length || candidates.length !== 1 || candidate === undefined) throw new Error("The reply needs exactly one top-level JSON object or array.");
  return JSON.parse(candidate);
}
