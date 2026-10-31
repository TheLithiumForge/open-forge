import { Plus, Save, X } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import type { StateResponse } from "../src/server/api.types.ts";
import { JudgedBy } from "../src/suite/suite-values.ts";
import type { IntentItem } from "../src/suite/suite.types.ts";
import { api } from "./api.ts";

interface IntentEditorProps {
  readonly state: StateResponse;
  readonly onChange: (state: StateResponse) => void;
}

const judges = Object.values(JudgedBy);

/** The wishes only the simulated user and the judges see. Edits change the eval's fingerprint. */
export function IntentEditor({ state, onChange }: IntentEditorProps) {
  const [items, setItems] = useState<IntentItem[]>([...state.suite.intent]);
  const [saved, setSaved] = useState(true);
  const update = (next: IntentItem[]) => {
    setItems(next);
    setSaved(false);
  };
  const edit = (index: number, change: Partial<IntentItem>) => update(items.map((item, position) => (position === index ? { ...item, ...change } : item)));
  const save = () =>
    api.saveIntent(items).then((next) => {
      onChange(next);
      setSaved(true);
    });

  return (
    <div className="space-y-3">
      <p className="text-sm text-muted-foreground">What the user wants but didn't say. The simulated user answers from these, and the checks or the judge score them.</p>
      {items.map((item, index) => (
        <div key={index} className="flex items-center gap-2">
          <Input value={item.text} onChange={(event) => edit(index, { text: event.target.value })} />
          <Select value={item.judgedBy} onValueChange={(value) => edit(index, { judgedBy: judges.find((judge) => judge === value) ?? item.judgedBy })}>
            <SelectTrigger className="w-28">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {judges.map((judge) => (
                <SelectItem key={judge} value={judge}>
                  {judge}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button variant="ghost" size="icon" aria-label="Remove" onClick={() => update(items.filter((_, position) => position !== index))}>
            <X />
          </Button>
        </div>
      ))}
      <div className="flex gap-2">
        <Button variant="outline" size="sm" onClick={() => update([...items, { id: `wish-${items.length + 1}`, text: "", judgedBy: JudgedBy.judge }])}>
          <Plus /> Add a wish
        </Button>
        <Button variant="secondary" size="sm" disabled={saved} onClick={save}>
          <Save /> Save wishes
        </Button>
      </div>
    </div>
  );
}
