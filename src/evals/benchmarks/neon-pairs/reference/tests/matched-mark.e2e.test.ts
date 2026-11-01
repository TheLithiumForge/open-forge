import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { boardCode } from '../src/game/deck.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { loadFixedBoard } from './game-fixture.ts';

// Each layout gives a different card size. The 16x16 deck holds all 128 faces.
const LAYOUTS = [
  { width: 360, height: 800, size: 16 },
  { width: 1280, height: 800, size: 16 },
  { width: 1280, height: 800, size: 8 },
] as const;

for (const { width, height, size } of LAYOUTS)
  test(`check marks leave every matched face uncovered on ${size}x${size} at ${width}x${height}`, async ({
    page,
  }) => {
    await page.setViewportSize({ width, height });
    await loadFixedBoard(page);
    const completed = {
      version: SNAPSHOT_VERSION,
      boardCode: boardCode(size, 1),
      matched: Array.from({ length: size ** 2 }, (_, index) => index),
      selection: { kind: SelectionKind.closed },
      moves: size ** 2 / 2,
      elapsedMs: 0,
      started: true,
    };
    await page.locator('summary').click();
    await page
      .getByLabel(ReproductionControl.snapshot, { exact: true })
      .fill(JSON.stringify(completed));
    await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
    await expect(page.locator('.card.matched')).toHaveCount(size ** 2);
    const covered = await page.locator('.card').evaluateAll((cards) =>
      cards.flatMap((card, index) => {
        const mark = card.querySelector('.matched-mark')?.getBoundingClientRect();
        const face = card.querySelector('svg:not(.matched-mark)');
        const marker = face?.querySelector(':scope > circle');
        const shape = face?.querySelector(':scope > g');
        const matrix = face instanceof SVGSVGElement ? face.getScreenCTM() : null;
        if (
          !mark ||
          !matrix ||
          !(marker instanceof SVGCircleElement) ||
          !(shape instanceof SVGGElement)
        )
          return [`card ${index} lacks its face or mark`];
        const toScreen = (x: number, y: number) => ({
          x: matrix.a * x + matrix.e,
          y: matrix.d * y + matrix.f,
        });
        // The marker circle with half its stroke, and the shape box with half its outline stroke.
        const center = toScreen(marker.cx.baseVal.value, marker.cy.baseVal.value);
        const reach =
          (marker.r.baseVal.value + Number(marker.getAttribute('stroke-width') ?? 0) / 2) *
          matrix.a;
        const gapX = Math.max(mark.left - center.x, 0, center.x - mark.right);
        const gapY = Math.max(mark.top - center.y, 0, center.y - mark.bottom);
        const box = shape.getBBox();
        const half = Number(shape.getAttribute('stroke-width') ?? 0) / 2;
        const start = toScreen(box.x - half, box.y - half);
        const end = toScreen(box.x + box.width + half, box.y + box.height + half);
        const shapeHit =
          start.x < mark.right && end.x > mark.left && start.y < mark.bottom && end.y > mark.top;
        return [
          ...(Math.hypot(gapX, gapY) < reach ? [`card ${index} marker`] : []),
          ...(shapeHit ? [`card ${index} shape`] : []),
        ];
      }),
    );
    expect(covered, 'V15 check marks cover no face artwork').toEqual([]);
  });
