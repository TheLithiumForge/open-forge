export const GameSandbox = "allow-scripts allow-same-origin";

export function gameHeaders(port: number): Readonly<Record<string, string>> {
  const parents = [`http://localhost:${port}`, `http://127.0.0.1:${port}`].join(" ");
  return {
    "content-security-policy": ["default-src 'none'", "script-src 'unsafe-inline' 'unsafe-eval'", "style-src 'unsafe-inline'",
      "img-src data: blob:", "font-src data:", "media-src data: blob:", "connect-src 'none'", "frame-src 'none'",
      "worker-src 'none'", "form-action 'none'", "base-uri 'none'", `frame-ancestors ${parents}`, `sandbox ${GameSandbox}`].join("; "),
    "origin-agent-cluster": "?1", "referrer-policy": "no-referrer", "x-content-type-options": "nosniff",
    "permissions-policy": "camera=(), microphone=(), geolocation=(), payment=(), usb=()",
  };
}
