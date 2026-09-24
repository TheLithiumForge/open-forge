import { statSync } from "node:fs";
import { dirname, isAbsolute, relative, resolve, sep } from "node:path";
import type { Definition, Link, Root } from "mdast";
import { visit } from "unist-util-visit";

// Rewrites relative Markdown links that leave the published documentation into
// GitHub URLs, so the same source reads correctly on GitHub and on the site.

export interface RepositoryLinkOptions {
  /** Absolute path of the repository root. */
  readonly repositoryRoot: string;
  /** Returns true when the absolute path is a page this site publishes. */
  readonly isPublished: (absolutePath: string) => boolean;
  readonly blobBaseUrl: string;
  readonly treeBaseUrl: string;
}

interface SourceFile {
  readonly path: string;
}

const schemePattern = /^[a-z][a-z\d+.-]*:/i;
const fragmentSeparator = "#";

function isRelativeDestination(url: string): boolean {
  return url !== "" && !schemePattern.test(url) && !url.startsWith(fragmentSeparator) && !url.startsWith("/") && !url.startsWith("@");
}

export function isInside(parent: string, candidate: string): boolean {
  const path = relative(parent, candidate);
  return path !== "" && !path.startsWith("..") && !isAbsolute(path);
}

function repositoryUrlFor(absolutePath: string, fragment: string, options: RepositoryLinkOptions): string {
  const repositoryPath = relative(options.repositoryRoot, absolutePath).split(sep).join("/");
  const isDirectory = statSync(absolutePath, { throwIfNoEntry: false })?.isDirectory() ?? false;
  const baseUrl = isDirectory ? options.treeBaseUrl : options.blobBaseUrl;
  return `${baseUrl}/${repositoryPath}${fragment}`;
}

function rewrite(node: Link | Definition, file: SourceFile, options: RepositoryLinkOptions): void {
  if (!isRelativeDestination(node.url)) {
    return;
  }

  const fragmentIndex = node.url.indexOf(fragmentSeparator);
  const target = fragmentIndex === -1 ? node.url : node.url.slice(0, fragmentIndex);
  const fragment = fragmentIndex === -1 ? "" : node.url.slice(fragmentIndex);
  const absolutePath = resolve(dirname(file.path), decodeURI(target));

  if (options.isPublished(absolutePath) || !isInside(options.repositoryRoot, absolutePath)) {
    return;
  }

  node.url = repositoryUrlFor(absolutePath, fragment, options);
}

export function repositoryLinks(options: RepositoryLinkOptions) {
  return (tree: Root, file: SourceFile): void => {
    visit(tree, "link", (node) => rewrite(node, file, options));
    visit(tree, "definition", (node) => rewrite(node, file, options));
  };
}
