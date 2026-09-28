// The repository README is written for GitHub, where relative links resolve
// inside the repository. The npm package page has no repository context, so its
// copy of the README uses absolute links. Images come from the documentation
// site, because GitHub's raw file host serves SVG as plain text.

export const RepositoryUrl = "https://github.com/TheLithiumForge/open-forge";
export const SiteUrl = "https://thelithiumforge.github.io/open-forge";

const SiteStaticPrefix = "src/docusaurus/static/";
const RepositoryRef = "main";
const Fence = /^\s*(```|~~~)/u;

export function packageReadme(markdown: string): string {
  let insideFence = false;
  return markdown
    .split("\n")
    .map((line) => {
      if (Fence.test(line)) {
        insideFence = !insideFence;
        return line;
      }
      return insideFence ? line : absoluteLine(line);
    })
    .join("\n");
}

function absoluteLine(line: string): string {
  return line
    .replace(/(\]\()([^)\s]+)(\))/gu, (_match, open: string, target: string, close: string) => open + absoluteTarget(target) + close)
    .replace(/((?:src|srcset|href)=")([^"]+)(")/gu, (_match, open: string, target: string, close: string) => open + absoluteTarget(target) + close);
}

function absoluteTarget(target: string): string {
  if (/^[a-z][a-z\d+.-]*:/iu.test(target) || target.startsWith("#") || target.startsWith("/")) return target;
  const path = target.replace(/^\.\//u, "");
  if (path.startsWith(SiteStaticPrefix)) return `${SiteUrl}/${path.slice(SiteStaticPrefix.length)}`;
  const kind = path.split(/[#?]/u)[0]?.endsWith("/") ? "tree" : "blob";
  return `${RepositoryUrl}/${kind}/${RepositoryRef}/${path}`;
}
