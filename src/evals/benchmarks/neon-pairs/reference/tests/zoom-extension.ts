import { mkdir, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

declare const chrome: {
  tabs: {
    query(query: { url: string }, callback: (tabs: { id?: number }[]) => void): void;
    setZoom(id: number, factor: number): void;
  };
};

export async function zoomExtension(profile: string): Promise<string> {
  const extension = join(profile, 'extension');
  await mkdir(extension);
  // This generated fixture uses Chrome's page zoom API rather than a CSS or device-scale substitute.
  await writeFile(join(extension, 'background.js'), 'globalThis.ready = true;');
  await writeFile(
    join(extension, 'manifest.json'),
    JSON.stringify({
      manifest_version: 3,
      name: 'Browser zoom fixture',
      version: '1.0',
      permissions: ['tabs'],
      background: { service_worker: 'background.js' },
    }),
  );
  return extension;
}

export function zoomGameTab(url: string): Promise<void> {
  return new Promise((resolve, reject) =>
    chrome.tabs.query({ url }, (tabs) => {
      const tab = tabs[0];
      if (tab?.id === undefined) {
        reject(new Error('Game tab missing.'));
        return;
      }
      chrome.tabs.setZoom(tab.id, 2);
      resolve();
    }),
  );
}
