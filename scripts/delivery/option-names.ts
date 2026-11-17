export const DeliveryOption = { version: "version", from: "from", to: "to" } as const;

export function optionArgument(name: (typeof DeliveryOption)[keyof typeof DeliveryOption]): string {
  return `--${name}`;
}
