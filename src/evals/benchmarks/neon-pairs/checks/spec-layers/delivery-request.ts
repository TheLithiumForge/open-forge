export function allowedRequest(url: string, entry: string): boolean {
  return url === entry || /^(?:data|blob):/.test(url);
}
