import { readFile } from 'node:fs/promises';
import { createServer } from 'node:http';
import { resolve } from 'node:path';
import { test as base } from '@playwright/test';

export const test = base.extend<{ gameURL: string }, { artifactOrigin: string }>({
  artifactOrigin: [async ({}, use) => {
    const html = await readFile(resolve('dist/index.html'));
    const server = createServer((request, response) => {
      if (request.url !== '/') { response.writeHead(404).end(); return; }
      response.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' }).end(html);
    });
    await new Promise<void>((ready, reject) => {
      server.once('error', reject);
      server.listen(0, '127.0.0.1', ready);
    });
    const address = server.address();
    if (!address || typeof address === 'string') throw new Error('Expected a private test port.');
    try { await use(`http://127.0.0.1:${address.port}/`); }
    finally {
      await new Promise<void>((closed, reject) => server.close((error) => { if (error) reject(error); else closed(); }));
    }
  }, { scope: 'worker' }],
  gameURL: async ({ artifactOrigin }, use) => use(artifactOrigin),
});
