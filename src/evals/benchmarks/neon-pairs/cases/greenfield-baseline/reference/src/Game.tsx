import { useEffect, useRef, useState } from "react";
import { Card } from "./Card.tsx";
import { faceCount } from "./faces.tsx";
import { loadBest, loadSize, saveBest, saveSize } from "./scores.ts";

const sizes = [4, 6, 8, 10, 12, 14, 16] as const;
const flipBackMs = 800;

interface CardState {
  face: number;
  matched: boolean;
}

function shuffle<T>(items: T[]): T[] {
  const copy = [...items];
  for (let index = copy.length - 1; index > 0; index -= 1) {
    const other = Math.floor(Math.random() * (index + 1));
    [copy[index], copy[other]] = [copy[other]!, copy[index]!];
  }
  return copy;
}

function deal(size: number): CardState[] {
  const faces = shuffle(Array.from({ length: faceCount }, (_, face) => face)).slice(0, (size * size) / 2);
  return shuffle([...faces, ...faces]).map((face) => ({ face, matched: false }));
}

const styles = {
  page: { minHeight: "100vh", background: "#07040f", color: "#e8e6ff", fontFamily: "system-ui, sans-serif", display: "flex", flexDirection: "column", alignItems: "center", gap: 16, padding: 16, boxSizing: "border-box" },
  title: { margin: 0, color: "#00f0ff", textShadow: "0 0 12px #00f0ff, 0 0 24px #ff2bd6", letterSpacing: 2 },
  bar: { display: "flex", gap: 12, alignItems: "center", flexWrap: "wrap", justifyContent: "center" },
  control: { background: "#120a24", color: "#e8e6ff", border: "1px solid #ff2bd6", borderRadius: 8, padding: "6px 12px", font: "inherit" },
  won: { color: "#ff2bd6", textShadow: "0 0 10px #ff2bd6", animation: "pulse 1s ease-in-out 3" },
} as const;

export function Game() {
  const [size, setSize] = useState(() => loadSize(sizes));
  const [cards, setCards] = useState(() => deal(size));
  const [open, setOpen] = useState<number[]>([]);
  const [moves, setMoves] = useState(0);
  const [best, setBest] = useState(loadBest);
  const timer = useRef<number | undefined>(undefined);
  const won = cards.every((card) => card.matched);

  useEffect(() => () => window.clearTimeout(timer.current), []);

  function newGame(nextSize: number) {
    window.clearTimeout(timer.current);
    timer.current = undefined;
    saveSize(nextSize);
    setSize(nextSize);
    setCards(deal(nextSize));
    setOpen([]);
    setMoves(0);
  }

  function flip(index: number) {
    if (timer.current !== undefined || open.includes(index) || cards[index]?.matched) return;
    if (open.length === 0) return setOpen([index]);
    const first = open[0]!;
    setMoves(moves + 1);
    if (cards[first]?.face !== cards[index]?.face) {
      setOpen([first, index]);
      timer.current = window.setTimeout(() => {
        timer.current = undefined;
        setOpen([]);
      }, flipBackMs);
      return;
    }
    const next = cards.map((card, position) => (position === first || position === index ? { ...card, matched: true } : card));
    setCards(next);
    setOpen([]);
    if (next.every((card) => card.matched)) setBest(saveBest(size, moves + 1));
  }

  const boardWidth = `min(92vw, ${size * 56}px, 720px)`;
  return (
    <main style={styles.page}>
      <style>{"@keyframes pulse { 50% { transform: scale(1.15); } }"}</style>
      <h1 style={styles.title}>Neon Pairs</h1>
      <div style={styles.bar}>
        <label>
          Grid{" "}
          <select style={styles.control} value={size} onChange={(event) => newGame(Number(event.target.value))}>
            {sizes.map((option) => (
              <option key={option} value={option}>{`${option}x${option}`}</option>
            ))}
          </select>
        </label>
        <span>Moves: {moves}</span>
        <span>Best: {best[size] ?? "—"}</span>
        <button type="button" style={styles.control} onClick={() => newGame(size)}>
          New Game
        </button>
      </div>
      {won ? <strong style={styles.won}>You won in {moves} moves!</strong> : null}
      <div style={{ display: "grid", gridTemplateColumns: `repeat(${size}, 1fr)`, gap: size > 10 ? 2 : 6, width: boardWidth }}>
        {cards.map((card, index) => (
          <Card key={`${size}-${index}`} face={card.face} up={card.matched || open.includes(index)} matched={card.matched} onFlip={() => flip(index)} label={`Card ${index + 1}`} />
        ))}
      </div>
    </main>
  );
}
