import { faceDesign } from './face-design.ts';
import type { FaceShape } from './face-design.ts';

function shapeElement(shape: FaceShape) {
  switch (shape) {
    case 'circle': return <circle cx="50" cy="50" r="30" />;
    case 'square': return <rect x="20" y="20" width="60" height="60" />;
    case 'triangle': return <path d="M50 20 80 80 20 80Z" />;
    case 'diamond': return <path d="M50 20 80 50 50 80 20 50Z" />;
    case 'star': return <path d="M50 20 58 40 80 42 63 56 68 80 50 67 32 80 37 56 20 42 42 40Z" />;
    case 'hexagon': return <path d="M35 20 65 20 80 50 65 80 35 80 20 50Z" />;
    case 'plus': return <path d="M40 20H60V40H80V60H60V80H40V60H20V40H40Z" />;
    case 'chevron': return <path d="M20 20 50 45 80 20V55L50 80 20 55Z" />;
    default: return unreachable(shape);
  }
}

export function Face({ face }: { face: number }) {
  const design = faceDesign(face);
  return (
    <svg viewBox="0 0 100 100" aria-hidden="true" focusable="false">
      <g fill={design.outline ? 'none' : design.color} stroke={design.color} strokeWidth={design.outline ? 5 : 0} strokeLinejoin="round">
        {shapeElement(design.shape)}
      </g>
      <circle cx={design.center[0]} cy={design.center[1]} r="4" fill={design.color} />
    </svg>
  );
}

function unreachable(value: never): never { throw new Error(`Unknown shape: ${String(value)}`); }
