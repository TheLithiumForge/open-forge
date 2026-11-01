import { expectedDeck } from "./board-code.ts";
import { KnownCode } from "./vectors.ts";
import { LoadNotice } from "./published-text.ts";
import { faceName } from "./rules.ts";
import { CardAttribute } from "../board.ts";

export function reproductionFixture(): string {
  const start = (config: { code: string; deck: number[]; faces: string[]; notices: typeof LoadNotice; attribute: string }): void => {
    const select = document.querySelector("select")!, code = document.querySelector<HTMLInputElement>("#code")!, field = document.querySelector<HTMLTextAreaElement>("textarea")!, board = document.querySelector("#board")!, status = document.querySelector("#status")!, notice = document.querySelector("#notice")!, live = document.querySelector("#live")!;
    const now = performance.now.bind(performance);
    let paused = false, best: number | null = localStorage.getItem("best") === null ? null : Number(localStorage.getItem("best")), previous = now();
    let state = fresh(`np1-${localStorage.getItem("size") ?? "4"}-1`);
    function fresh(value: string) { return { version: 1, boardCode: value, moves: 0, elapsedMs: 0, started: false, matched: [] as number[], selection: { kind: "closed", index: 0, first: 0, second: 0, remainingMs: 0 } }; }
    const won = () => state.matched.length === Number(select.value) ** 2;
    const emit = (text: string): void => { live.replaceChildren(document.createTextNode(text)); notice.textContent = text; };
    function advance(): void {
      const current = now(), elapsed = current - previous; previous = current;
      if (!paused && state.started && !won()) {
        state.elapsedMs += elapsed;
        if (state.selection.kind === "mismatch") {
          state.selection.remainingMs -= elapsed;
          if (state.selection.remainingMs <= 0) state.selection = { kind: "closed", index: 0, first: 0, second: 0, remainingMs: 0 };
        }
      }
    }
    function render(): void {
      status.textContent = `Moves: ${state.moves} Pairs: ${state.matched.length / 2}/${Number(select.value) ** 2 / 2} Time: 0:00 Best: ${best ?? "none"}`;
      document.querySelector("#win")!.textContent = won() ? `Every pair found in ${state.moves} moves and 0:00.` : "";
      board.replaceChildren();
      for (let index = 0; index < Number(select.value) ** 2; index++) {
        const button = document.createElement("button"), matched = state.matched.includes(index);
        const shown = matched || state.selection.kind === "one" && state.selection.index === index || state.selection.kind === "mismatch" && [state.selection.first, state.selection.second].includes(index);
        button.tabIndex = index === 0 ? 0 : -1;
        button.setAttribute(config.attribute, String(index));
        button.setAttribute("aria-label", `Row ${Math.floor(index / Number(select.value)) + 1}, column ${index % Number(select.value) + 1}, ${shown ? `${config.faces[config.deck[index] ?? 0]}, ${matched ? "matched" : "revealed"}` : "closed"}`);
        button.onkeydown = (event) => { if (event.key === "ArrowRight") { event.preventDefault(); (board.children[index + 1] as HTMLElement)?.focus(); } };
        button.onclick = () => {
          advance(); if (paused || matched || state.selection.kind === "mismatch") return;
          state.started = true;
          if (state.selection.kind === "closed") state.selection = { kind: "one", index, first: 0, second: 0, remainingMs: 0 };
          else if (state.selection.index !== index) {
            const first = state.selection.index; state.moves++;
            if (config.deck[first] === config.deck[index]) { state.matched.push(first, index); state.matched.sort((a, b) => a - b); state.selection = { kind: "closed", index: 0, first: 0, second: 0, remainingMs: 0 }; if (won()) { best = best === null ? state.moves : Math.min(best, state.moves); localStorage.setItem("best", String(best)); } }
            else state.selection = { kind: "mismatch", index: 0, first, second: index, remainingMs: 800 };
          }
          render();
        };
        board.append(button);
      }
      document.querySelector("#resume")?.remove();
      if (paused) {
        const button = document.createElement("button"); button.id = "resume"; button.textContent = "Resume";
        button.onclick = () => { paused = false; previous = now(); (board.children[1] as HTMLElement)?.focus(); button.remove(); };
        document.querySelector("details")!.append(button);
      }
    }
    function replace(value: ReturnType<typeof fresh>, imported: boolean): void {
      state = value; code.value = state.boardCode; select.value = state.boardCode.split("-")[1]!; localStorage.setItem("size", select.value); previous = now(); paused = imported && !won(); render();
    }
    document.querySelector<HTMLButtonElement>("#load-board")!.onclick = () => {
      const value = code.value.trim();
      if (!/^np1-(?:4|6|8|16)-[1-9]\d*$/.test(value)) { emit(config.notices.boardError); return; }
      replace(fresh(value), false); emit("New game ready.");
    };
    document.querySelector<HTMLButtonElement>("#load-snapshot")!.onclick = () => {
      try { const value = JSON.parse(field.value); if (value.version !== 1) throw new Error("invalid"); replace(value, true); emit(paused ? config.notices.unfinished : config.notices.completed); }
      catch { emit(config.notices.snapshotError); }
    };
    document.querySelector<HTMLButtonElement>("#export")!.onclick = () => {
      advance(); const selection = state.selection;
      field.value = JSON.stringify({ ...state, elapsedMs: Math.floor(state.elapsedMs), selection: selection.kind === "closed" ? { kind: "closed" } : selection.kind === "one" ? { kind: "one", index: selection.index } : { kind: "mismatch", first: selection.first, second: selection.second, remainingMs: Math.ceil(selection.remainingMs) } }, null, 2);
    };
    document.querySelector<HTMLButtonElement>("#restart")!.onclick = () => replace(fresh(state.boardCode), false);
    document.querySelector<HTMLButtonElement>("#new")!.onclick = () => replace(fresh(state.boardCode), false);
    select.onchange = () => replace(fresh(`np1-${select.value}-1`), false);
    replace(state, false);
  };
  const config = { code: KnownCode, deck: expectedDeck(KnownCode), faces: Array.from({ length: 128 }, (_, id) => faceName(id)), notices: LoadNotice, attribute: CardAttribute };
  return `<!doctype html><button id="new">New Game</button><button id="restart">Restart</button><label>Grid size<select>${[4, 6, 8, 16].map((size) => `<option value="${size}">${size}x${size}</option>`).join("")}</select></label><p id="status"></p><p id="win"></p><div id="live" aria-live="polite" aria-atomic="true"></div><div id="board"></div><details><summary>Reproduce a game</summary><label>Board code<input id="code" maxlength="32"></label><button id="load-board">Load board</button><label>Snapshot JSON<textarea maxlength="65536"></textarea></label><button id="load-snapshot">Load snapshot</button><button id="export">Export snapshot</button><p id="notice"></p></details><script>(${start.toString()})(${JSON.stringify(config)})</script>`;
}
