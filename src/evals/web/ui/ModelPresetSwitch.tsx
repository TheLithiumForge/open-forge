import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { CustomPreset } from "../../config/presets.ts";

interface ModelPresetSwitchProps {
  readonly id: string;
  readonly label: string;
  readonly value: string;
  readonly names: readonly string[];
  readonly disabled: boolean;
  readonly onChange: (name: string) => void;
}

export function ModelPresetSwitch({ id, label, value, names, disabled, onChange }: ModelPresetSwitchProps) {
  return <div className="flex items-center justify-between gap-3">
    <Label htmlFor={id}>{label}</Label>
    <Select value={value} onValueChange={onChange} disabled={disabled}>
      <SelectTrigger id={id} aria-label={`${label} preset`} className="w-36"><SelectValue /></SelectTrigger>
      <SelectContent>
        {value === CustomPreset ? <SelectItem value={CustomPreset} disabled>{CustomPreset}</SelectItem> : null}
        {names.map((name) => <SelectItem key={name} value={name}>{name}</SelectItem>)}
      </SelectContent>
    </Select>
  </div>;
}
