import { Plus, Save } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Isolation, Tool } from "../../config/choices.ts";
import type { EvalConfig } from "../../config/config.types.ts";
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
        <Label>Judge, at least as strong as the agents</Label>
        <ModelChoiceRow choice={config.judge} onChange={(next) => change({ judge: { ...config.judge, ...next } })} />
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          Scores each run
          <Input type="number" min={1} className="w-16" value={config.judge.repeats} onChange={(event) => change({ judge: { ...config.judge, repeats: Number(event.target.value) } })} />
          times
        </div>
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
