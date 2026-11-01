export const Tool = { claude: "claude", codex: "codex", mock: "mock" } as const;
export type Tool = (typeof Tool)[keyof typeof Tool];

export const Isolation = { docker: "docker", local: "local" } as const;
export type Isolation = (typeof Isolation)[keyof typeof Isolation];
