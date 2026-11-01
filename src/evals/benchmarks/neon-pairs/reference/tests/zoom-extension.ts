import { mkdir, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

declare const chrome: {
  tabs: {
    onUpdated: { addListener(callback: (id: number, change: { status?: string }) => void): void };
    setZoom(id: number, factor: number): void;
  };
};

function zoomOnLoad(): void {
  chrome.tabs.onUpdated.addListener((id, change) => {
    if (change.status === 'complete') chrome.tabs.setZoom(id, 2);
  });
}

export async function zoomExtension(profile: string): Promise<string> {
  const extension = join(profile, 'extension');
  await mkdir(extension);
  // This generated fixture uses Chrome's page zoom API rather than a CSS or device-scale substitute.
  await writeFile(join(extension, 'background.js'), `(${zoomOnLoad.toString()})();`);
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
