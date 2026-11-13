export const DeliveryOption = { version: "version", from: "from", to: "to", registry: "registry" } as const;

export function optionArgument(name: (typeof DeliveryOption)[keyof typeof DeliveryOption]): string {
  return `--${name}`;
}
