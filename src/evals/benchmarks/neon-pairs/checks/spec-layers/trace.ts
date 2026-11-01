import * as ts from "typescript";

export function fillTrace(text: string, slots: ReadonlyMap<string, readonly string[]>): string {
  const parsed = ts.parseJsonText("trace.json", text);
  const edits: { start: number; end: number; text: string }[] = [];
  const seen = new Set<string>();
  const visit = (node: ts.Node): void => {
    if (ts.isObjectLiteralExpression(node)) {
      const properties = node.properties.filter(ts.isPropertyAssignment);
      const field = (name: string) => properties.find((item) => ts.isStringLiteral(item.name) && item.name.text === name)?.initializer;
      const id = field("clauseId"), checks = field("checks");
      if (id && ts.isStringLiteral(id) && checks && ts.isArrayLiteralExpression(checks)) {
        const replacement = slots.get(id.text);
        if (replacement) { edits.push({ start: checks.getStart(parsed), end: checks.end, text: JSON.stringify(replacement) }); seen.add(id.text); }
      }
    }
    node.forEachChild(visit);
  };
  visit(parsed);
  if ([...slots.keys()].some((id) => !seen.has(id))) throw new Error("Trace has no checks slot for a declared clause.");
  let output = text;
  for (const edit of edits.sort((a, b) => b.start - a.start)) output = output.slice(0, edit.start) + edit.text + output.slice(edit.end);
  return output;
}
