import * as ts from "typescript";
import { descendants, syntax, type SourceFile } from "./source.ts";
import { Evidence } from "./outcomes.ts";

export function numeric(node: ts.Node, values: ReadonlyMap<string, number>): number | null {
  if (ts.isNumericLiteral(node)) return Number(node.text);
  if (ts.isIdentifier(node)) return values.get(node.text) ?? null;
  if (ts.isParenthesizedExpression(node) || ts.isAsExpression(node)) return numeric(node.expression, values);
  if (ts.isPropertyAccessExpression(node) && node.name.text === "length") return values.get(`${node.expression.getText()}.length`) ?? null;
  if (ts.isCallExpression(node) && node.expression.getText() === "Math.floor" && node.arguments[0]) {
    const value = numeric(node.arguments[0], values); return value === null ? null : Math.floor(value);
  }
  if (!ts.isBinaryExpression(node)) return null;
  const left = numeric(node.left, values), right = numeric(node.right, values);
  if (left === null || right === null) return null;
  switch (node.operatorToken.kind) {
    case ts.SyntaxKind.PercentToken: return left % right;
    case ts.SyntaxKind.SlashToken: return left / right;
    case ts.SyntaxKind.AsteriskToken: return left * right;
    case ts.SyntaxKind.PlusToken: return left + right;
    case ts.SyntaxKind.MinusToken: return left - right;
    default: return null;
  }
}
export function formula(files: readonly SourceFile[], expected: (id: number) => number): string | null {
  const constants = new Map<string, number>();
  const nodes = files.flatMap((file) => descendants(syntax(file)));
  for (const node of nodes) if (ts.isVariableDeclaration(node) && node.initializer && ts.isIdentifier(node.name)) {
    const value = node.initializer;
    const array = ts.isAsExpression(value) ? value.expression : value;
    if (ts.isArrayLiteralExpression(array)) constants.set(`${node.name.text}.length`, array.elements.length);
    if (ts.isNumericLiteral(value)) constants.set(node.name.text, Number(value.text));
  }
  for (const node of nodes.filter((item) => ts.isBinaryExpression(item) || ts.isCallExpression(item))) {
    const variables = new Set(descendants(node).filter(ts.isIdentifier).map((item) => item.text).filter((name) => !["Math", "floor"].includes(name) && !constants.has(name)));
    for (const variable of variables) {
      const matches = Array.from({ length: 128 }, (_, id) => { const values = new Map(constants); values.set(variable, id); return numeric(node, values) === expected(id); }).every(Boolean);
      if (matches) return node.getText();
    }
  }
  return null;
}
export function faceFormulaChecks(files: readonly SourceFile[], evidence: Evidence): void {
  const shapes = formula(files, (id) => id % 8), markers = formula(files, (id) => Math.floor(id / 8) % 8), fills = formula(files, (id) => Math.floor(id / 64));
  for (const [id, expression] of [["V10.6", shapes], ["V10.7", markers], ["V10.8", fills]] as const) evidence.test(id, expression !== null, expression ? `Static arithmetic evaluated for IDs 0..127: ${expression}.` : "No supported arithmetic expression implements the stated mapping.");
  evidence.test("V10.2", shapes !== null && markers !== null && fills !== null && files.some((file) => /(?:face|id).*number|number.*(?:face|id)/i.test(file.text)), "Numeric face declarations and integer mappings span 0..127.");
}
