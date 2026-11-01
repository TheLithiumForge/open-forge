import { createRoot } from 'react-dom/client';
import { Card } from '../src/game/board/Card.tsx';
import { FACE_COUNT, faceDesign } from '../src/game/board/face-design.ts';
import '../src/game/game.css';
import '../src/game/board/board.css';
import './face-sheet.css';

const root = document.getElementById('root');
if (!root) throw new Error('Missing face-sheet root.');
function ignore(): void {}
createRoot(root).render(
  <main>
    <h1>Neon Pairs face sheet</h1>
    <p>Each face uses the actual 44px card. Shape, marker and fill define its identity.</p>
    <div className="face-sheet">
      {Array.from({ length: FACE_COUNT }, (_, face) => (
        <figure key={face}>
          <div className="face-swatch">
            <Card
              index={face}
              size={16}
              face={face}
              matched={false}
              blocked={true}
              tabIndex={-1}
              activate={ignore}
              focus={ignore}
              navigate={ignore}
            />
          </div>
          <figcaption>
            {face}: {faceDesign(face).label}
          </figcaption>
        </figure>
      ))}
    </div>
  </main>,
);
