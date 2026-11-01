import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tool } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";

const tools = Object.values(Tool);

interface ModelChoiceRowProps {
  readonly choice: ModelChoice;
  readonly onChange: (choice: ModelChoice) => void;
  readonly onRemove?: () => void;
}

/** A tool, a model and an effort. Empty fields use the tool's own default. */
export function ModelChoiceRow({ choice, onChange, onRemove }: ModelChoiceRowProps) {
  return (
    <div className="flex items-center gap-2">
      <Select value={choice.tool} onValueChange={(value) => onChange({ ...choice, tool: tools.find((tool) => tool === value) ?? choice.tool })}>
        <SelectTrigger className="w-28">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {tools.map((tool) => (
            <SelectItem key={tool} value={tool}>
              {tool}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <Input placeholder="default model" value={choice.model ?? ""} onChange={(event) => onChange({ ...choice, model: event.target.value || null })} />
      <Input className="w-28" placeholder="effort" value={choice.effort ?? ""} onChange={(event) => onChange({ ...choice, effort: event.target.value || null })} />
      {onRemove === undefined ? null : (
        <Button variant="ghost" size="icon" aria-label="Remove" onClick={onRemove}>
          <X />
        </Button>
      )}
    </div>
  );
}
