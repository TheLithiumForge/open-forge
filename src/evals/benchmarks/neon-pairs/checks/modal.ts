import { errors, type Page } from "playwright";
import { GameFailure } from "./check-ids.ts";

const ModalSelector = 'dialog:modal, [role="dialog"][aria-modal="true"]';
const ActionTimeoutMs = 2000;
const CloseNames = [/close|dismiss|continue|^ok$/i, /play again|new game|restart/i];

/** Clears a blocking dialog through the same controls a player can use. */
export async function dismissModal(page: Page): Promise<void> {
  for (let attempt = 0; attempt < 4; attempt += 1) {
    const modal = page.locator(ModalSelector).filter({ visible: true }).last();
    if (!(await modal.count())) return;
    await page.keyboard.press("Escape");
    await page.waitForTimeout(100);
    if (!(await modal.isVisible())) continue;
    let closed = false;
    for (const name of CloseNames) {
      const buttons = modal.getByRole("button", { name }).filter({ visible: true });
      for (const button of await buttons.all()) {
        if (!(await button.isEnabled())) continue;
        try { await button.click({ timeout: ActionTimeoutMs }); }
        catch (error) { if (error instanceof errors.TimeoutError) continue; throw error; }
        await page.waitForTimeout(100);
        if (!(await modal.isVisible())) { closed = true; break; }
      }
      if (closed) break;
    }
    await page.waitForTimeout(100);
    if (!closed && await modal.isVisible()) break;
  }
  if (await page.locator(ModalSelector).filter({ visible: true }).count()) throw new GameFailure("An open modal could not be closed with Escape or its close, dismiss or play-again button.");
}
