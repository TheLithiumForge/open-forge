import { All, type Filters, type SetupSummary } from "../data/aggregate.ts";
import type { Cell } from "../data/cells.ts";
import { controlSetups, summaryRows, tokenClassRows } from "../data/chart-data.ts";
import { NoValue, signed } from "../data/format.ts";
import { Term } from "../info/Term.tsx";
import type { CostPoint } from "./CostQualityChart.tsx";
import { CostQualityChart } from "./CostQualityChart.tsx";
import { DeltaChart } from "./DeltaChart.tsx";
import { RunSpread } from "./RunSpread.tsx";
import { TokenClassChart } from "./TokenClassChart.tsx";

const PointStep = 10;
const PercentStep = 20;
const asPoints = (delta: number | null) => delta === null ? null : delta * 100;
const asPercent = (ratio: number | null) => ratio === null ? null : (ratio - 1) * 100;
const pointText = (value: number | null) => value === null ? NoValue : signed(value, 1);
const percentText = (value: number | null) => value === null ? NoValue : `${signed(value, 0)}%`;
const pointTick = (value: number) => signed(value, 0);
const percentTick = (value: number) => `${signed(value, 0)}%`;

function Legend() {
  return <ul className="eval-legend" aria-label="Colors">
    <li><span className="eval-swatch eval-swatch-open-forge" aria-hidden="true" />Open Forge setups</li>
    <li><span className="eval-swatch eval-swatch-other" aria-hidden="true" />Other setups</li>
  </ul>;
}

/** The averages as charts, then every run when the filters keep one model and one starting point. */
export function ChartsSection({ summaries, cells, filters }: {
  readonly summaries: readonly SetupSummary[];
  readonly cells: readonly Cell[];
  readonly filters: Filters;
}) {
  const controls = controlSetups(cells);
  const coverage = summaryRows(summaries, controls, (summary) => asPoints(summary.coverageDelta));
  const judge = summaryRows(summaries, controls, (summary) => asPoints(summary.judgeDelta));
  const tokens = summaryRows(summaries, controls, (summary) => asPercent(summary.tokenRatio));
  const cost: CostPoint[] = coverage.flatMap((row) => {
    const spent = tokens.find((item) => item.setup === row.setup)?.value ?? null;
    return row.value === null || spent === null ? [] : [{ setup: row.setup, name: row.name, openForge: row.openForge, quality: row.value, tokens: spent }];
  });
  const narrowed = filters.model !== All && filters.startingPoint !== All;
  return <section className="eval-section" aria-labelledby="eval-charts">
    <h2 id="eval-charts">Charts</h2>
    <p>Each chart starts from the same averages as the cards, for every setup. Each starting point has its own control: <Term id="no-setup" /> for an idea and <Term id="plain-docs" /> for the others. The control is the zero line.</p>
    {coverage.length === 0 ? <p>No setup has a comparison with the control in these filters.</p> : <>
      <Legend />
      <div className="eval-charts">
        <DeltaChart rows={coverage} format={pointText} tick={pointTick} step={PointStep}
          title={<><Term id="check-coverage" /> change</>} description="Percentage points above or below the control." />
        <DeltaChart rows={judge} format={pointText} tick={pointTick} step={PointStep}
          title={<><Term id="judge-score" /> change</>} description="Percentage points above or below the control." />
        <DeltaChart rows={tokens} format={percentText} tick={percentTick} step={PercentStep}
          title={<><Term id="tokens" /> change</>} description="Percent more or fewer tokens than the control. Tokens are the main cost measure." />
        <TokenClassChart rows={tokenClassRows(cells)} />
        <CostQualityChart points={cost} />
      </div>
    </>}
    {narrowed ? <RunSpread cells={cells} /> : <p className="eval-muted">Choose one model and one starting point to see every run.</p>}
  </section>;
}
