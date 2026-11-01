import { createRoot } from 'react-dom/client';
import { Game } from '../../src/game/Game.tsx';

const element = document.getElementById('root');
if (!element) throw new Error('Missing session harness root.');
const host = document.createElement('div');
const unmount = document.createElement('button');
unmount.textContent = 'Unmount game';
element.append(unmount, host);
const root = createRoot(host);
root.render(<Game />);
unmount.addEventListener('click', () => root.unmount());
