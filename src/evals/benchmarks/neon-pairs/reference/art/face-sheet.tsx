import { createRoot } from 'react-dom/client';
import { Face } from '../src/game/board/Face.tsx';
import { faceDesign } from '../src/game/board/face-design.ts';
import './face-sheet.css';

const FACE_COUNT = 128;
const root = document.getElementById('root');
if (!root) throw new Error('Missing face-sheet root.');
createRoot(root).render(
  <main>
    <h1>Neon Pairs face sheet</h1>
    <p>All 128 faces use the same Face component as the game. The first 64 are solid. The next 64 are outlines.</p>
    <div className="face-sheet">
      {Array.from({ length: FACE_COUNT }, (_, face) => (
        <figure key={face}><Face face={face} /><figcaption>{face}: {faceDesign(face).label}</figcaption></figure>
      ))}
    </div>
  </main>,
);
