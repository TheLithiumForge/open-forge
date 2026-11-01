export const Finding = { name: "name", turned: "turned", best: "best", win: "win", modal: "modal", odd: "odd" } as const;
export const FixtureText = { logo: "neonpairs.", title: "Neon Pairs — Find your next match", best: "PERSONAL BEST", win: "All paired up. Nicely done!", alternateWin: "Ready for another round?", noBest: "—set your first" } as const;
export const FixtureSize = { small: 4, odd: 5, large: 16 } as const;

function documentWith(body: string, title = FixtureText.title): string {
  return `<!doctype html><html><head><meta charset="utf-8"><title>${title}</title><style>
    body{margin:20px;background:#080810;color:white;font:16px sans-serif}
    .board{display:grid;gap:4px;width:max-content}
    .card,.empty{width:60px;height:60px;box-sizing:border-box}
    .card{position:relative;background:#101018;border:1px solid #242430;padding:0}
    .card:focus-visible{outline:2px solid white;outline-offset:1px}
    .card:hover{box-shadow:inset 0 0 16px #888}
    .inner{position:absolute;inset:0;transform-style:preserve-3d;transition:transform .1s}
    .up .inner{transform:rotateY(180deg)}
    .back,.face{position:absolute;inset:0;backface-visibility:hidden}
    .face{transform:rotateY(180deg);display:grid;place-items:center}
    .face svg{width:70%;height:70%}
    dialog::backdrop{background:#0008}
  </style></head><body>${body}</body></html>`;
}

export function oddBoard(size: number, missing = size % 2, columns = size): string {
  const slots = Array.from({ length: size * size }, (_, index) => index >= Math.floor(size * size / 2) && index < Math.floor(size * size / 2) + missing
    ? '<div class="empty"></div>' : '<button class="card" aria-label="Card"></button>').join("");
  return documentWith(`<div class="board" style="grid-template-columns:repeat(${columns},60px)">${slots}</div>`);
}

export function namedPage(broken = false, titleOnly = false): string {
  return oddBoard(FixtureSize.small).replace(`<title>${FixtureText.title}</title>`, `<title>${broken ? "Other game" : FixtureText.title}</title>`)
    .replace("<body>", `<body><h1>${titleOnly || broken ? "Find a match" : FixtureText.logo}</h1>`);
}

export function bestPage(value: string): string {
  return documentWith(`<section><div>${FixtureText.best}</div><strong>${value}<small>moves</small></strong></section>`);
}

export function winPage(won: boolean, broken = false, phrase: string = FixtureText.win): string {
  return documentWith(`<h1>${FixtureText.logo}</h1><p>Moves: ${won ? 14 : 0}</p>${won && !broken ? `<p role="status">${phrase}</p>` : ""}`);
}

export function modalPage(kind: "escape" | "close" | "again" | "fallback" | "stuck"): string {
  const label = kind === "again" || kind === "fallback" ? "Play again" : "Dismiss";
  return documentWith(`<button id="next">Next action</button><dialog><p>${FixtureText.win}</p>${kind === "fallback" ? "<button>Close</button>" : ""}${kind === "stuck" ? "" : `<button id="dismiss">${label}</button>`}</dialog><script>
    const dialog = document.querySelector('dialog');
    ${kind === "escape" ? "" : "dialog.addEventListener('cancel', event => event.preventDefault());"}
    dialog.querySelector('#dismiss')?.addEventListener('click', () => dialog.close());
    dialog.showModal();
  </script>`);
}

export function thinGame(broken = false, modal = false): string {
  const cards = Array.from({ length: FixtureSize.small ** 2 }, (_, index) => `<button class="card" aria-label="Card ${index + 1}"><span class="inner"><span class="back"></span><span class="face"><svg viewBox="0 0 60 60"><path d="M${10 + Math.floor(index / 2) * 3} 10V50H50" fill="none" stroke="#303038" stroke-width="1"/></svg></span></span></button>`).join("");
  return documentWith(`<h1>${FixtureText.logo}</h1><button id="new">New Game</button><select aria-label="Size"><option>4</option><option>16</option></select><p id="moves">Moves: 0</p><p id="win"></p><div class="board" style="grid-template-columns:repeat(4,60px)">${cards}</div><dialog><p>${FixtureText.win}</p><button>Close</button></dialog><script>
    let first = null, moves = 0, matched = 0;
    const cards = [...document.querySelectorAll('.card')];
    const dialog = document.querySelector('dialog');
    dialog.querySelector('button').onclick = () => dialog.close();
    document.querySelector('#new').onclick = () => {
      first = null; moves = 0; matched = 0;
      cards.forEach(card => { card.classList.remove('up'); card.disabled = false; });
      document.querySelector('#moves').textContent = 'Moves: 0';
      document.querySelector('#win').textContent = '';
    };
    cards.forEach((card, index) => card.onclick = () => {
      ${broken ? "return;" : ""}
      if (card.classList.contains('up')) return;
      card.classList.add('up');
      if (first === null) { first = index; return; }
      moves += 1; document.querySelector('#moves').textContent = 'Moves: ' + moves;
      const previous = first; first = null;
      if (Math.floor(previous / 2) === Math.floor(index / 2)) {
        cards[previous].disabled = true; card.disabled = true; matched += 2;
        if (matched === cards.length) { document.querySelector('#win').textContent = ${JSON.stringify(FixtureText.win)}; ${modal ? "dialog.showModal();" : ""} }
      } else setTimeout(() => { cards[previous].classList.remove('up'); card.classList.remove('up'); }, 700);
    });
    document.querySelector('select').onchange = () => {
      const size = Number(document.querySelector('select').value);
      document.querySelector('.board').style.gridTemplateColumns = 'repeat(' + size + ',60px)';
      if (size === 16) document.querySelector('.board').innerHTML = '<button class="card" aria-label="Card"></button>'.repeat(256);
    };
  </script>`);
}
