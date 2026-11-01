import { Play } from "lucide-react";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import type { StateResponse } from "../server/api.types.ts";
import { ConfigEditor } from "./ConfigEditor.tsx";
import { IntentEditor } from "./IntentEditor.tsx";

interface SetupPanelProps {
  readonly state: StateResponse;
  readonly onChange: (state: StateResponse) => void;
  readonly onRun: () => void;
}

const Sections = { wishes: "wishes", runs: "runs" } as const;

/** What every run gets: the prompt, the hidden wishes, and which tools, models and setups run. */
export function SetupPanel({ state, onChange, onRun }: SetupPanelProps) {
  const { suite, config } = state;
  const runCount = config.agents.length * config.setups.length * config.runsPerSetup;
  return (
    <Card>
      <CardHeader>
        <CardTitle>Setup</CardTitle>
        <CardDescription className="flex flex-wrap items-center gap-2">
          Eval <code className="text-foreground">{suite.fingerprint}</code>
          {suite.intentEdited ? <Badge variant="secondary">edited wishes</Badge> : null}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <p className="text-sm font-medium">The prompt every run gets</p>
          <p className="rounded-md border bg-muted/40 p-3 text-sm leading-relaxed text-muted-foreground">{suite.prompt}</p>
        </div>
        <Accordion type="multiple" defaultValue={[Sections.runs]}>
          <AccordionItem value={Sections.wishes}>
            <AccordionTrigger>Hidden wishes ({suite.intent.length})</AccordionTrigger>
            <AccordionContent>
              <IntentEditor state={state} onChange={onChange} />
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value={Sections.runs}>
            <AccordionTrigger>Runs</AccordionTrigger>
            <AccordionContent>
              <ConfigEditor key={JSON.stringify(config)} state={state} onChange={onChange} />
            </AccordionContent>
          </AccordionItem>
        </Accordion>
      </CardContent>
      <CardFooter className="flex-col items-stretch gap-2">
        <Button size="lg" disabled={state.running !== null} onClick={onRun}>
          <Play /> {state.running === null ? `Run ${runCount} ${runCount === 1 ? "run" : "runs"}` : "Running…"}
        </Button>
        {suite.intentEdited ? <p className="text-xs text-muted-foreground">Edited wishes change the fingerprint, so these results won't mix with the standard eval.</p> : null}
      </CardFooter>
    </Card>
  );
}
