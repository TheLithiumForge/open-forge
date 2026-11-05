# ADR 012: Make silence the product behavior

This record explains why Neon Pairs uses visual state and accessible text feedback without game audio.

## Context

Sound was attractive as an easy way to punctuate a reveal or reward a match. It also raised questions that the game did not otherwise need to answer: when to unlock playback, what to do with autoplay restrictions, how to set volume, and whether a device was muted. The core memory task already had observable card states and a textual result for a completed turn.

The decision was to keep this version silent. Revealing, matching, missing, restarting and completing all follow the same audio behavior. Silence is intentional rather than a partially implemented effects system. There are no sound assets, audio API calls or sound toggle in the playable game.

## Feedback still has work to do

A matched card stays face up, receives a green border and displays a check mark. A mismatch remains visible for its active interval before both cards close. The status area shows moves, pair progress and elapsed time. A completed board has a visible summary. None of those observations requires a sound effect to explain what happened.

Turn feedback also reaches browser accessibility APIs as text. That is distinct from the game synthesizing speech. A screen reader can present the page according to the player's own tools and settings; the application does not start a competing spoken track. [Feedback without audio](feedback-without-audio.md) explains the available feedback channels.

## Alternatives

An always-on effects track would have made a local game surprising in shared spaces. A mute toggle would have added a preference and another control to a small interface. A default-muted track would still have required playback and asset paths that were irrelevant to the required journeys. The team accepted that a silent game has a less theatrical win in exchange for consistent behavior across launch contexts.

Generated tones would have avoided audio files but retained audio-context lifecycle and permission issues. Sound samples would have added distribution weight and an asset-review problem. Neither offered a necessary distinction that the card artwork or turn text lacked.

## How the decision is evidenced

The source audit checks authored game files for audio constructs alongside other runtime boundaries. The browser environment test adds failing stubs for audio constructors, media playback and synthesized speech, then plays through completion and restart. It also counts media requests and checks that no audio or video elements appear.

Those checks answer different questions. A static scan can catch an unused API reference that a particular journey never executes. The browser journey can catch unexpected behavior introduced through a rendered path. Counting requests alone would miss synthesized tones, and searching for assets alone would miss programmatic playback. The evidence is stronger because it combines those observations.

## Consequences and limits

A celebratory experience is carried by the banner and its short permitted pulse. The reduced-motion presentation remains understandable because the summary text and matched board stay visible. Removing motion does not reveal an audio dependency that was concealed during ordinary play. See [ADR 025](adr-025-reduced-motion.md) and [ADR 050](adr-050-completion-banner.md).

This choice says nothing about muting a player's assistive technology or browser. The page supplies semantic information for those tools to present. The recorded automated accessibility evidence is tree evidence; spoken output remains part of user acceptance. [Sound](sound.md) is the product definition, and [Audio research notes](audio-research-notes.md) records the surrounding design discussion.

## A browser check that would detect playback

The test replaces playback entry points with failures before the game journey. This checks behavior rather than the presence of a mute icon.

```ts
test('keeps the initial focus and produces no audio during play', async ({ page }) => {
  let audioRequests = 0;
  page.on('request', (request) => {
    if (request.resourceType() === 'media') audioRequests += 1;
  });
  await page.goto(GAME_URL);
  expect(
    await page.locator('button:focus, select:focus, input:focus, textarea:focus').count(),
  ).toBe(0);
  await page.addInitScript(() => {
    for (const key of ['AudioContext', 'webkitAudioContext', 'Audio']) {
      Object.defineProperty(window, key, {
        configurable: true,
        value: class {
          constructor() {
            throw new Error('Unexpected synthesized audio');
          }
        },
      });
    }
    HTMLMediaElement.prototype.play = () => {
      throw new Error('Unexpected media playback');
    };
    speechSynthesis.speak = () => {
      throw new Error('Unexpected synthesized speech');
    };
  });
  await loadFixedBoard(page);
  await finishBoard(page);
  await page.getByRole('button', { name: DealControl.restart, exact: true }).click();
  await expect(page.locator('audio, video')).toHaveCount(0);
  expect(audioRequests).toBe(0);
});
```
