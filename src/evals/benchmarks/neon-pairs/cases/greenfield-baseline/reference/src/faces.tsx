// Every face is a shape, a color and a fill, so 8 x 8 x 2 = 128 faces cover a 16x16 board.
const shapes = ["circle", "square", "triangle", "diamond", "star", "hexagon", "plus", "chevron"] as const;
const colors = ["#00f0ff", "#ff2bd6", "#39ff14", "#ffe600", "#ff7a00", "#8a5cff", "#ff3b3b", "#ffffff"] as const;
export const faceCount = shapes.length * colors.length * 2;

const paths: Record<(typeof shapes)[number], string> = {
  circle: "M50 14a36 36 0 1 0 0.1 0Z",
  square: "M18 18h64v64h-64Z",
  triangle: "M50 12l40 72h-80Z",
  diamond: "M50 8l38 42-38 42-38-42Z",
  star: "M50 8l11 30h32l-26 19 10 31-27-19-27 19 10-31-26-19h32Z",
  hexagon: "M30 14h40l20 36-20 36h-40l-20-36Z",
  plus: "M38 12h24v26h26v24h-26v26h-24v-26h-26v-24h26Z",
  chevron: "M14 30l36 26 36-26v20l-36 26-36-26Z",
};

export function Face({ face }: { face: number }) {
  const shape = shapes[face % shapes.length];
  const color = colors[Math.floor(face / shapes.length) % colors.length];
  const filled = face < faceCount / 2;
  return (
    <svg viewBox="0 0 100 100" width="70%" height="70%" style={{ filter: `drop-shadow(0 0 4px ${color})` }} aria-hidden="true">
      <path d={paths[shape]} fill={filled ? color : "none"} stroke={color} strokeWidth={filled ? 2 : 7} strokeLinejoin="round" />
    </svg>
  );
}
