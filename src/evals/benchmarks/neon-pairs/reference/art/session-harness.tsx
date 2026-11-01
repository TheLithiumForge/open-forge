import { createRoot } from 'react-dom/client';
import { Game } from '../src/game/Game.tsx';

export function mountSessionHarness(element: HTMLElement): void {
  const host = document.createElement('div');
  const unmount = document.createElement('button');
  unmount.textContent = 'Unmount game';
  element.append(unmount, host);
  const root = createRoot(host);
  root.render(<Game />);
  unmount.addEventListener('click', () => root.unmount());
}
