import { Face } from "./faces.tsx";

interface CardProps {
  face: number;
  up: boolean;
  matched: boolean;
  onFlip: () => void;
  label: string;
}

const base = {
  aspectRatio: "1",
  borderRadius: "12%",
  border: "1px solid #2a1f4a",
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  padding: 0,
  cursor: "pointer",
  transition: "background 160ms, box-shadow 160ms",
} as const;

export function Card({ face, up, matched, onFlip, label }: CardProps) {
  const look = up
    ? { background: "#120a24", boxShadow: matched ? "0 0 10px #ff2bd6" : "0 0 8px #00f0ff" }
    : { background: "repeating-linear-gradient(45deg, #1b1035 0 6px, #140b28 6px 12px)", boxShadow: "none" };
  return (
    <button type="button" aria-label={up ? `${label}, face up` : `${label}, face down`} onClick={onFlip} style={{ ...base, ...look }}>
      {up ? <Face face={face} /> : null}
    </button>
  );
}
