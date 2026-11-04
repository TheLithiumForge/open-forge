import { PriceVersions, type PriceVersion, type TokenPrices } from "./data/prices.ts";

const rate = (value: number) => `$${value.toLocaleString("en", { minimumFractionDigits: 2, maximumFractionDigits: 3 })}`;
const cells = (prices: TokenPrices) => <>
  <td>{rate(prices.input)}</td><td>{rate(prices.cachedInput)}</td><td>{rate(prices.cacheWrite)}</td><td>{rate(prices.output)}</td>
</>;

function PriceTable({ version }: { readonly version: PriceVersion }) {
  return <div className="eval-scroll" tabIndex={0} role="region" aria-label={`Prices from ${version.from}`}>
    <table className="eval-table">
      <thead><tr>{["Model", "Input", "Cached input", "Cache write", "Output"].map((title) => <th key={title} scope="col">{title}</th>)}</tr></thead>
      <tbody>{Object.entries(version.models).flatMap(([id, model]) => [
        <tr key={id}><th scope="row">{model.name} <code>{id}</code></th>{cells(model)}</tr>,
        ...(model.long === undefined ? [] : [<tr key={`${id}-long`}><th scope="row">{model.name}, a request over {model.long.above.toLocaleString("en")} input tokens</th>{cells(model.long)}</tr>]),
      ])}</tbody>
    </table>
  </div>;
}

/** The price list from the data costs are computed with, so the page and the results can't disagree. */
export default function PriceTables() {
  const [current, ...earlier] = PriceVersions;
  if (current === undefined) return null;
  return <>
    <p>In effect from {current.from}, in US dollars per million tokens, read from {current.sources.map((source, index) => <span key={source.url}>{index === 0 ? "" : " and "}<a href={source.url}>{source.provider}'s price list</a></span>)}.</p>
    <PriceTable version={current} />
    {earlier.length === 0 ? null : <>
      <h2>Earlier prices</h2>
      {earlier.map((version) => <section key={version.from}><h3>From {version.from}</h3><PriceTable version={version} /></section>)}
    </>}
  </>;
}
