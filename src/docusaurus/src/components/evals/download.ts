export const JsonType = "application/json";
export const CsvType = "text/csv;charset=utf-8";

/** Saves text as a file in the browser, for downloads too large or too dynamic for a link. */
export function save(name: string, text: string, type: string): void {
  const url = URL.createObjectURL(new Blob([text], { type }));
  const link = Object.assign(document.createElement("a"), { href: url, download: name });
  link.click();
  URL.revokeObjectURL(url);
}
