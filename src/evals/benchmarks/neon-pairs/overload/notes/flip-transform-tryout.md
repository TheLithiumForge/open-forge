# Tryout: Card Flip Animation

What we tried for revealing cards.

A 3D `rotateY` flip looked nice on the laptop but stuttered on the old Android phone with the 16 by 16 board, and the back face sometimes flashed through. A quick scale and opacity change was smooth everywhere and kept the face readable during the change.

Worked: transform and opacity only, no layout properties. Didn't work: 3D flips with `backface-visibility` on large boards.
