import { Bar, BarChart, CartesianGrid, Scatter, ScatterChart, XAxis, YAxis, ZAxis } from "recharts";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ChartContainer, ChartLegend, ChartLegendContent, ChartTooltip, ChartTooltipContent, type ChartConfig } from "@/components/ui/chart";
import type { RunRecord } from "../../history/results.types.ts";
import { IntentCheckArea, type SetupSummary } from "../../history/summary.ts";

const qualityConfig = {
  checks: { label: "Checks", color: "var(--chart-1)" },
  wishes: { label: "Wishes", color: "var(--chart-2)" },
  rules: { label: "Rules", color: "var(--chart-3)" },
  code: { label: "Code", color: "var(--chart-4)" },
  picked: { label: "Picked", color: "var(--chart-5)" },
} satisfies ChartConfig;

const tokensConfig = { run: { label: "Run", color: "var(--chart-1)" } } satisfies ChartConfig;

const asPercent = (value: number | null): number => Math.round((value ?? 0) * 100);

/** Shares from 0 to 100% per setup, one bar per measure. */
export function QualityChart({ rows }: { rows: readonly SetupSummary[] }) {
  const data = rows.map((row) => ({
    setup: row.setup,
    checks: asPercent(row.coverage),
    wishes: asPercent(row.intent),
    rules: asPercent(row.rules),
    code: asPercent(row.code),
    picked: asPercent(row.preference),
  }));
  return (
    <Card>
      <CardHeader>
        <CardTitle>Quality per setup</CardTitle>
        <CardDescription>Percent of checks, wishes, judge questions and blind picks</CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={qualityConfig} className="h-72 w-full">
          <BarChart data={data}>
            <CartesianGrid vertical={false} />
            <XAxis dataKey="setup" tickLine={false} axisLine={false} />
            <YAxis domain={[0, 100]} tickLine={false} axisLine={false} width={32} />
            <ChartTooltip content={<ChartTooltipContent />} />
            <ChartLegend content={<ChartLegendContent />} />
            {Object.keys(qualityConfig).map((key) => (
              <Bar key={key} dataKey={key} fill={`var(--color-${key})`} radius={3} />
            ))}
          </BarChart>
        </ChartContainer>
      </CardContent>
    </Card>
  );
}

/** Tokens against the share of checks passed, one dot per run, so cost and quality show together. */
export function TokensChart({ runs }: { runs: readonly RunRecord[] }) {
  const data = runs.flatMap((run) => {
    const tokens = run.telemetry?.tokens;
    const stated = run.checks.filter((check) => check.area !== IntentCheckArea && check.passed !== null);
    if (tokens === null || tokens === undefined || stated.length === 0) return [];
    const total = tokens.input + tokens.cachedInput + tokens.cacheWrite + tokens.output;
    return [{ setup: run.setup, tokens: total, checks: Math.round((stated.filter((check) => check.passed === true).length / stated.length) * 100) }];
  });
  return (
    <Card>
      <CardHeader>
        <CardTitle>Tokens and checks</CardTitle>
        <CardDescription>One dot per run</CardDescription>
      </CardHeader>
      <CardContent>
        {data.length === 0 ? (
          <p className="text-sm text-muted-foreground">No token counts yet. Mock runs report none.</p>
        ) : (
          <ChartContainer config={tokensConfig} className="h-72 w-full">
            <ScatterChart>
              <CartesianGrid />
              <XAxis type="number" dataKey="tokens" name="Tokens" tickFormatter={(value: number) => `${Math.round(value / 1000)}k`} />
              <YAxis type="number" dataKey="checks" name="Checks" unit="%" domain={[0, 100]} width={40} />
              <ZAxis type="category" dataKey="setup" name="Setup" />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Scatter data={data} fill="var(--color-run)" />
            </ScatterChart>
          </ChartContainer>
        )}
      </CardContent>
    </Card>
  );
}
