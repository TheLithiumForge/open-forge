import { readFile } from 'node:fs/promises';
import { createServer } from 'node:http';
import { fileURLToPath } from 'node:url';

export async function serveArtifact() {
  const html = await readFile(new URL('../dist/index.html', import.meta.url));
  const server = createServer((request, response) => {
    if (request.url !== '/') {
      response.writeHead(404).end();
      return;
    }
    response.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' }).end(html);
  });
  await new Promise<void>((ready, reject) => {
    server.once('error', reject);
    server.listen(0, '127.0.0.1', ready);
  });
  const address = server.address();
  if (!address || typeof address === 'string') throw new Error('Expected a private test port.');
  return {
    url: `http://127.0.0.1:${address.port}/`,
    close: () =>
      new Promise<void>((closed, reject) => {
        server.close((error) => {
          if (error) reject(error);
          else closed();
        });
      }),
  };
}

export const REFERENCE_ROOT = fileURLToPath(new URL('../', import.meta.url));
