# Screen Reader Journey

Screen Reader Journey is a manual NVDA check that records the spoken result of playing one complete Neon Pairs game with the keyboard.

For example, matching positions 3 and 7 on `np1-4-1` should announce one found pair. Moving focus afterward should leave that result silent.

Status: I17.6 is open for a person. Chromium tree checks cover I17.1 through I17.5. No spoken result is recorded yet.

## Record versions

Before starting, fill in this record with the actual versions. Copy the completed record into the game's README under Accessibility:

| Field                                  | Result  |
| -------------------------------------- | ------- |
| NVDA version from Help, About          | Pending |
| Chromium or Chrome version from About  | Pending |
| Windows version                        | Pending |
| Game source hash from seed-record.json | Pending |
| Journey result                         | Pending |
| Unexpected speech and step             | Pending |

Keep personal names and machine paths out of the record. Record a failed step with its expected and heard speech. Leave I17.6 open until a person completes the journey and saves the versions and result.

## Prepare

Build the game with `npm run build`, then open its `dist/index.html` in the recorded browser. Start NVDA. Keep browser zoom at 100 percent for this journey. Use Tab to reach native controls. Use NVDA+Space to enter focus mode when the arrow keys need to reach the game instead of moving the browse cursor.

Set up the fixed deal with the keyboard:

1. Find "Grid size" by name. Hear its combobox role and the value "4x4". Choose 4x4 if needed.
2. Tab to "New Game". Hear its button role. Press Enter. Focus stays on New Game.
3. Tab through Restart to the first card. Hear "Row 1, column 1, closed" and its button role. Keep the card closed.
4. Tab to "Reproduce a game". Press Space to expand it. Tab to "Board code". Press Ctrl+A and type `np1-4-1`. Tab to "Load board" and press Enter. Focus stays on Load board.
5. Shift+Tab to Board code, then Shift+Tab to Reproduce a game. Press Space to collapse it. Shift+Tab enters the board at its first card. Hear its closed position.

## Finish a game

Use Ctrl+Home before each destination. Then press ArrowDown once for each preceding row and ArrowRight once for each preceding column. Arrow movement should announce the destination's position and closed state without revealing it or repeating a turn result.

For each row below, reveal the first position with Enter. Hear its face name and revealed state. Reveal the second position with Space. Hear that face and one turn result. The matched buttons should expose their matched state and disabled state while still allowing focus:

| Turn | First position | Second position | Face name                       | Expected result                                              |
| ---- | -------------- | --------------- | ------------------------------- | ------------------------------------------------------------ |
| 1    | Row 1 column 3 | Row 2 column 3  | cyan solid circle, marker top   | Pair found. 1 pair matched.                                  |
| 2    | Row 1 column 1 | Row 3 column 4  | cyan solid plus, marker top     | Pair found. 2 pairs matched.                                 |
| 3    | Row 1 column 2 | Row 3 column 2  | cyan solid chevron, marker top  | Pair found. 3 pairs matched.                                 |
| 4    | Row 1 column 4 | Row 2 column 4  | cyan solid hexagon, marker top  | Pair found. 4 pairs matched.                                 |
| 5    | Row 2 column 1 | Row 2 column 2  | cyan solid triangle, marker top | Pair found. 5 pairs matched.                                 |
| 6    | Row 3 column 1 | Row 4 column 4  | cyan solid diamond, marker top  | Pair found. 6 pairs matched.                                 |
| 7    | Row 3 column 3 | Row 4 column 3  | cyan solid square, marker top   | Pair found. 7 pairs matched.                                 |
| 8    | Row 4 column 1 | Row 4 column 2  | cyan solid star, marker top     | Pair found. All pairs found in 8 moves and the elapsed time. |

The final turn has one result that combines the match and win. Its elapsed time depends on your pace. Compare it with the static win summary and the Active time value. Read the Moves value as 8 and Pairs as 8 / 8. Wait and move focus. The clock stays stopped and the turn result stays silent.

## Start again

From the final card, Tab leaves the board and reaches Reproduce a game. Shift+Tab returns to the remembered card. Shift+Tab reaches Restart. Shift+Tab reaches New Game. Press Enter. Hear the new-game status once. Focus stays on New Game. Tab through Restart to the first card and hear its closed position.

Record whether names, roles, states, face identities and each single turn result were heard. Record whether moves and time could be read and whether Tab exit and New Game worked. A tree capture records accessibility data. Only this person-run check records speech.

Back to [Reference Audit](AUDIT.md).
