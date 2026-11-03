# Large Board Performance

Analysis from the 16 by 16 profiling session.

The first build rendered all 256 faces up front, and the first paint took about 900 ms on the test laptop and over 2 s on the old Android phone. Most of the time went to SVG layout for faces nobody could see yet.

Mounting a face's SVG only while its card is revealed or matched brought the first paint under 200 ms on the laptop. Memoizing cards so a clock tick doesn't render every card again removed the stutter during play.

Open question: the phone still drops frames when a whole row is matched at once. It's within budget, so we left it.
