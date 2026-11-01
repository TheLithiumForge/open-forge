import { Plus, Save } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Isolation, Tool } from "../../config/choices.ts";
import type { EvalConfig, JudgeChoice } from "../../config/config.types.ts";
import { configuredJudges } from "../../config/judges.ts";
import { MatrixBallots } from "../../config/matrix.ts";
import type { StateResponse } from "../server/api.types.ts";
import { api } from "./api.ts";
import { ModelChoiceRow } from "./ModelChoiceRow.tsx";

interface ConfigEditorProps {
  readonly state: StateResponse;
  readonly onChange: (state: StateResponse) => void;
}

const isolations = Object.values(Isolation);

/** Tools, models, setups and limits. Saved to eval.config.local.json, which replaces the defaults. */
export function ConfigEditor({ state, onChange }: ConfigEditorProps) {
  const [config, setConfig] = useState<EvalConfig>(state.config);
  const [message, setMessage] = useState("");
  const change = (next: Partial<EvalConfig>) => {
    setConfig({ ...config, ...next });
    setMessage("Not saved yet.");
  };
  const toggleSetup = (setup: string) => change({ setups: config.setups.includes(setup) ? config.setups.filter((item) => item !== setup) : [...config.setups, setup] });
  const judges = configuredJudges(config);
  const changeJudges = (next: readonly JudgeChoice[]) => {
    const first = next[0];
    if (first !== undefined) change({ judge: first, judges: next });
  };
  const save = () =>
    api.saveConfig(config).then(
      () => api.state().then(onChange),
      (reason: unknown) => setMessage(String(reason)),
    );

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <Label>Agents under test</Label>
        {config.agents.map((agent, index) => (
          <ModelChoiceRow
            key={index}
            choice={agent}
            onChange={(next) => change({ agents: config.agents.map((item, position) => (position === index ? next : item)) })}
            onRemove={() => change({ agents: config.agents.filter((_, position) => position !== index) })}
          />
        ))}
        <Button variant="outline" size="sm" onClick={() => change({ agents: [...config.agents, { tool: Tool.codex, model: null, effort: null }] })}>
          <Plus /> Add an agent
        </Button>
      </div>
      <div className="space-y-2">
        <Label>Judges, in order</Label>
        <p className="text-sm text-muted-foreground">Each attempt is implemented once. The first judge runs as it finishes. Every later judge reads the same saved attempt.</p>
        {judges.map((judge, index) => <div className="space-y-2" key={index}>
          <ModelChoiceRow choice={judge} onChange={(next) => changeJudges(judges.map((item, position) => position === index ? { ...judge, ...next } : item))}
            {...(judges.length > 1 ? { onRemove: () => changeJudges(judges.filter((_, position) => position !== index)) } : {})} />
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            Ballots per question
            <Input type="number" min={1} className="w-16" aria-label={`Judge ${index + 1} ballots`} value={judge.repeats}
              onChange={(event) => changeJudges(judges.map((item, position) => position === index ? { ...judge, repeats: Number(event.target.value) } : item))} />
          </div>
        </div>)}
        <Button variant="outline" size="sm" onClick={() => changeJudges([...judges, { tool: Tool.codex, model: null, effort: null, repeats: MatrixBallots }])}>
          <Plus /> Add a judge
        </Button>
      </div>
      <div className="space-y-2">
        <Label>Simulated user</Label>
        <ModelChoiceRow choice={config.simulatedUser} onChange={(next) => change({ simulatedUser: next })} />
      </div>
      <Separator />
      <div className="space-y-2">
        <Label>Setups</Label>
        <div className="grid grid-cols-2 gap-2">
          {Object.entries(state.suite.setups).map(([id, label]) => (
            <label key={id} className="flex items-center gap-2 text-sm">
              <Checkbox checked={config.setups.includes(id)} onCheckedChange={() => toggleSetup(id)} /> {label}
            </label>
          ))}
        </div>
      </div>
      <div className="grid grid-cols-3 gap-3">
        <div className="space-y-2">
          <Label htmlFor="runs-per-setup">Runs per setup</Label>
          <Input id="runs-per-setup" type="number" min={1} value={config.runsPerSetup} onChange={(event) => change({ runsPerSetup: Number(event.target.value) })} />
        </div>
        <div className="space-y-2">
          <Label htmlFor="parallel-runs">Runs at once</Label>
          <Input id="parallel-runs" type="number" min={1} value={config.parallelRuns} onChange={(event) => change({ parallelRuns: Number(event.target.value) })} />
        </div>
        <div className="space-y-2">
          <Label>Isolation</Label>
          <Select value={config.isolation} onValueChange={(value) => change({ isolation: isolations.find((item) => item === value) ?? config.isolation })}>
            <SelectTrigger className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {isolations.map((isolation) => (
                <SelectItem key={isolation} value={isolation}>
                  {isolation}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>
      <div className="flex items-center gap-3">
        <Button variant="secondary" onClick={save}>
          <Save /> Save
        </Button>
        <span className="text-sm text-muted-foreground">{message}</span>
      </div>
    </div>
  );
}
