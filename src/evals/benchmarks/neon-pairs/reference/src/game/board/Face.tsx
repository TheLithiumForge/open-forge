import { faceDesign } from './face-design.ts';
import type { FaceShape } from './face-design.ts';

function shapeElement(shape: FaceShape) {
  switch (shape) {
    case 'circle':
      return <circle cx="50" cy="50" r="25" />;
    case 'square':
      return <rect x="25" y="25" width="50" height="50" />;
    case 'triangle':
      return <path d="M50 25 75 75 25 75Z" />;
    case 'diamond':
      return <path d="M50 25 75 50 50 75 25 50Z" />;
    case 'star':
      return <path d="M50 25 57 42 75 43 62 56 66 75 50 65 34 75 38 56 25 43 43 42Z" />;
    case 'hexagon':
      return <path d="M37.5 25 62.5 25 75 50 62.5 75 37.5 75 25 50Z" />;
    case 'plus':
      return <path d="M42 25H58V42H75V58H58V75H42V58H25V42H42Z" />;
    case 'chevron':
      return <path d="M25 25 50 46 75 25V53L50 75 25 53Z" />;
    default:
      return unreachable(shape);
  }
}

export function Face({ face }: { face: number }) {
  const design = faceDesign(face);
  return (
    <svg viewBox="0 0 100 100" aria-hidden="true" focusable="false">
      <g
        fill={design.outline ? 'none' : design.color}
        stroke={design.color}
        strokeWidth={design.outline ? 5 : 0}
        strokeLinejoin="round"
      >
        {shapeElement(design.shape)}
      </g>
      <circle
        cx={design.center[0]}
        cy={design.center[1]}
        r="5"
        fill={design.color}
        stroke="var(--face)"
        strokeWidth="3"
      />
    </svg>
  );
}

function unreachable(value: never): never {
  throw new Error(`Unknown shape: ${String(value)}`);
}
