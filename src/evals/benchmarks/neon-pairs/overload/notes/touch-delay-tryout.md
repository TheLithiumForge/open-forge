# Tryout: Touch Response on Phones

What we tried after testers said taps felt slow.

Some phones waited for a possible double tap before sending the click, which made fast play feel sticky, and a double tap on a card sometimes zoomed the page. Setting `touch-action: manipulation` on the board removed the delay and the zoom without blocking pinch zoom on the rest of the page.

Worked: `touch-action: manipulation` on the board. Didn't work: listening to touch events directly, which double-fired activations next to the click.
