import { All, type Comparison, type Filters, type SetupSummary } from "../data/aggregate.ts";
import { controlSetups, summaryRows, tokenClassRows } from "../data/chart-data.ts";
import { NoValue, signed } from "../data/format.ts";
import { Term } from "../info/Term.tsx";
import { DeltaChart } from "./DeltaChart.tsx";
import { RunSpread } from "./RunSpread.tsx";
import { TokenClassChart } from "./TokenClassChart.tsx";
import { SuperChart } from "./SuperChart.tsx";

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
export function ChartsSection({ summaries, items, filters }: {
  readonly summaries: readonly SetupSummary[];
  readonly items: readonly Comparison[];
  readonly filters: Filters;
}) {
  const cells = items.map((item) => item.cell);
  const controls = controlSetups(cells);
  const coverage = summaryRows(summaries, controls, (summary) => asPoints(summary.coverageDelta));
  const judge = summaryRows(summaries, controls, (summary) => asPoints(summary.judgeDelta));
  const tokens = summaryRows(summaries, controls, (summary) => asPercent(summary.tokenRatio));
  const narrowed = filters.model !== All && filters.startingPoint !== All;
  return <section className="eval-section" aria-labelledby="eval-charts">
    <h2 id="eval-charts">Charts</h2>
    <p>Each chart starts from the same averages as the cards, for every scenario. Each starting point has its own control: <Term id="no-setup" /> for an idea and <Term id="plain-docs" /> for the others. The control is the zero line.</p>
    {coverage.length === 0 ? <p>No setup has a comparison with the control in these filters.</p> : <>
      <Legend />
      <div className="eval-charts">
        <SuperChart items={items} canShowRuns={narrowed} />
        <DeltaChart rows={coverage} format={pointText} tick={pointTick} step={PointStep}
          title={<><Term id="check-coverage" /> change</>} description="Percentage points above or below the control." />
        <DeltaChart rows={judge} format={pointText} tick={pointTick} step={PointStep}
          title={<><Term id="judge-score" /> change</>} description="Percentage points above or below the control." />
        <DeltaChart rows={tokens} format={percentText} tick={percentTick} step={PercentStep}
          title={<><Term id="tokens" /> change</>} description="Percent more or fewer tokens than the control. Tokens are the main cost measure." />
        <TokenClassChart rows={tokenClassRows(cells)} />
      </div>
    </>}
    {narrowed ? <RunSpread cells={cells} /> : <p className="eval-muted">Choose one model and one starting point to see every run.</p>}
  </section>;
}
