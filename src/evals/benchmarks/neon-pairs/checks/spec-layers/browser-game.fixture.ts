import type { Page } from "playwright";
import { markVisibleCards } from "./browser-cards.ts";
import { ContentMaximum, GameTitle } from "./browser-layout.ts";
import { Labels } from "./browser-session.ts";
import { Rules } from "./rules.ts";
import { Announcement } from "./published-text.ts";

export async function fakeGame(page: Page, options: { readonly mismatchMs?: number; readonly artwork?: boolean } = {}): Promise<void> {
  await page.clock.install();
  await page.setContent(`<style>
    * { box-sizing: border-box } body { margin: 0; font: 16px/24px system-ui }
    p { margin: 0 } article { max-width: ${ContentMaximum}px; margin: 0 auto; padding: 16px }
    .title { font-size: 24px; line-height: 32px } .controls { display: flex; gap: 6px; flex-wrap: wrap }
    .viewport { max-height: min(70dvh, 768px); max-width: 100%; width: fit-content; padding: 6px; border: 1px solid; overflow: auto }
    .board { display: flex; flex-wrap: wrap; gap: 6px }
    .card { flex: 0 0 44px; height: 44px; padding: 0; border: 2px solid; border-radius: 8px; scroll-margin: 6px }
    .card:focus { outline: 3px solid white; outline-offset: 2px }
  </style><main hidden><h1>${GameTitle}</h1></main><article><section>
    <div class="title"><span>${GameTitle}</span></div>
    <div class="controls"><label>${Labels.size}<select><option value="4">4x4</option><option value="16">16x16</option></select></label><button>${Labels.fresh}</button><button>${Labels.restart}</button></div>
    <p id="status"></p><p id="result"></p><div aria-live="polite" aria-atomic="true"></div><div class="viewport"><div class="board"></div></div>
    <input aria-label="Unstyled native control"><p id="secondary" style="font-size:14px;margin:0">Secondary text</p>
  </section></article>`);
  await page.evaluate(({ colors, markers, sizeLabel, miss, mismatchMs, artwork, green }) => {
    const board = document.querySelector<HTMLElement>(".board"), select = document.querySelector("select");
    if (!board || !select) throw new Error("Missing fake controls.");
    select.setAttribute("aria-label", sizeLabel);
    let size = 4, moves = 0, first: number | null = null, blocked = false, started: number | null = null, completedTime: number | null = null;
    const matched = new Set<number>();
    let pending: ReturnType<typeof setTimeout> | undefined, deadline = 0, remaining = 0, closeMiss: (() => void) | undefined;
    const label = (index: number, state: string): string => `Row ${Math.floor(index / size) + 1}, column ${index % size + 1}, ${state}`;
    const face = (index: number): string => `${colors[Math.floor(index / 2) % colors.length]} solid circle, marker ${markers[Math.floor(index / 2) % markers.length]}`;
    const announce = (text: string): void => { const region = document.querySelector('[aria-live="polite"]'); if (region) region.replaceChildren(document.createTextNode(text)); };
    const paint = (button: Element, found: boolean): void => {
      if (!artwork) return;
      button.innerHTML = `<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="5" fill="cyan"/>${found ? `<path d="M2 6 L5 9 L10 3" fill="none" stroke="${green}" stroke-width="2"/>` : ""}</svg>`;
      (button as HTMLElement).style.borderColor = found ? green : "";
    };
    document.addEventListener("visibilitychange", () => {
      if (document.hidden) { clearTimeout(pending); remaining = Math.max(0, deadline - performance.now()); }
      else if (blocked && closeMiss) { deadline = performance.now() + remaining; pending = setTimeout(closeMiss, remaining); }
    });
    const status = (): void => {
      const status = document.querySelector("#status"), result = document.querySelector("#result");
      const seconds = started === null ? 0 : Math.floor(((completedTime ?? performance.now()) - started) / 1000);
      const time = `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, "0")}`;
      if (status) status.textContent = `Moves: ${moves} Pairs: ${matched.size / 2}/${size * size / 2} Time: ${time} Best: none`;
      if (result) result.textContent = matched.size === size * size ? `You won in ${moves} moves. Time: 0:00` : "";
    };
    const start = (): void => {
      clearTimeout(pending); closeMiss = undefined;
      size = Number(select.value); moves = 0; first = null; blocked = false; started = null; completedTime = null; matched.clear(); board.replaceChildren();
      board.style.width = `${size * 50 - 6}px`;
      for (let index = 0; index < size * size; index++) {
        const button = document.createElement("button"); button.className = "card"; button.tabIndex = index === 0 ? 0 : -1;
        button.setAttribute("aria-label", label(index, "closed"));
        button.onfocus = () => { for (const sibling of board.querySelectorAll("button")) sibling.tabIndex = sibling === button ? 0 : -1; button.scrollIntoView({ block: "nearest", inline: "nearest" }); };
        button.onclick = () => {
          if (blocked || matched.has(index) || first === index) return;
          started ??= performance.now();
          button.setAttribute("aria-label", label(index, `${face(index)}, revealed`));
          paint(button, false);
          if (first === null) first = index;
          else {
            moves++;
            if (face(first) === face(index)) {
              for (const position of [first, index]) {
                matched.add(position); board.children[position]?.setAttribute("aria-label", label(position, `${face(position)}, matched`));
                board.children[position]?.setAttribute("aria-disabled", "true");
                if (board.children[position]) paint(board.children[position]!, true);
              }
              const pairs = matched.size / 2;
              if (pairs === size * size / 2) completedTime = performance.now();
              announce(pairs === size * size / 2 ? `Pair found. All pairs found in ${moves} moves and 0:00.` : `Pair found. ${pairs} ${pairs === 1 ? "pair" : "pairs"} matched.`);
            } else {
              const positions = [first, index]; blocked = true; announce(miss);
              closeMiss = () => { for (const position of positions) board.children[position]?.setAttribute("aria-label", label(position, "closed")); blocked = false; };
              remaining = mismatchMs; deadline = performance.now() + remaining; pending = setTimeout(closeMiss, remaining);
            }
            first = null;
          }
          status();
        };
        button.onkeydown = (event) => {
          if (event.ctrlKey || event.altKey || event.metaKey || event.shiftKey) return;
          const next = event.key === "ArrowRight" && index % size < size - 1 ? index + 1 : event.key === "ArrowLeft" && index % size > 0 ? index - 1 : event.key === "ArrowDown" ? index + size : event.key === "ArrowUp" ? index - size : -1;
          if (next >= 0 && next < size * size) { event.preventDefault(); (board.children[next] as HTMLElement).focus(); }
        };
        board.append(button);
      }
      status();
    };
    select.onchange = start;
    setInterval(status, 100);
    for (const button of document.querySelectorAll<HTMLButtonElement>(".controls button")) button.onclick = start;
    start();
  }, { colors: Rules.colors, markers: Rules.markers, sizeLabel: Labels.size, miss: Announcement.miss, mismatchMs: options.mismatchMs ?? Rules.mismatchMs, artwork: options.artwork ?? false, green: Rules.palette[2] });
  await markVisibleCards(page);
  await page.clock.pauseAt(new Date(Date.now() + 1000));
}

