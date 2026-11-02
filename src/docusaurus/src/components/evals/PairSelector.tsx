import { useId } from "react";
import type { PairOption } from "./data/views.ts";
export function PairSelector({ pairs, value, onChange }: {
  readonly pairs: readonly PairOption[];
  readonly value: string;
  readonly onChange: (key: string) => void;
}) {
  const id = useId();
  return <div className="eval-field"><label htmlFor={id}>Implementer and judge</label>
    <select id={id} value={value} onChange={(event) => onChange(event.target.value)}>
      {pairs.map((pair) => <option key={pair.key} value={pair.key}>{pair.label}</option>)}
    </select>
  </div>;
}
