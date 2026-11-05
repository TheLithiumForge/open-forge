# Competition pairing

This page describes entry-order seeding and the opening bracket for shared-device competition.

Pairing connects a group of people to an event structure. It is separate from choosing a board, recording a finished attempt or interpreting a tied result. The game has no participant collection; competition extends the existing solo controller.

## From the entry list to the opening matches

Entry order gives the group a sequence that can be inspected without any information about previous performance. Seeds follow entry order, so the first player entered is seed 1. A bracket of four meets as 1 against 4, then 2 against 3. A bracket of eight meets as 1 against 8, 4 against 5, 3 against 6, then 2 against 7. This fixes the opening arrangement while leaving the existing memory-game engine concerned with cards rather than people.

The ordering of the eight-person case deserves attention. Writing opposite ends of the entry list together produces the right collection of opponents, but a straightforward loop over the first half of the list would display them in a different order. The sequence in the paragraph above is therefore relevant to the shape of the bracket, not just to membership of each opening match.

Here is a worked example with eight entrants.

| Entry position | Entrant | Seed |
| --- | --- | --- |
| First | Ada | 1 |
| Second | Bo | 2 |
| Third | Cora | 3 |
| Fourth | Dev | 4 |
| Fifth | Emi | 5 |
| Sixth | Finn | 6 |
| Seventh | Gia | 7 |
| Eighth | Hal | 8 |

Reading down the opening matches gives Ada with Hal, Dev with Emi, Cora with Finn, then Bo with Gia. This example is useful during a design review because every participant is visibly accounted for, and a swapped middle match is easy to spot. It also avoids relying on the geometry of a bracket drawing to explain who is involved.

## What a seed means here

In this scope, the seed identifies a place derived from entry. It is not an estimate of memory skill. The current game has no account history, participant rating or service from which to obtain such an estimate. A developer looking for an existing ranking calculation in `src/game` will not find one.

This distinction makes the entry list meaningful to the group. If people entered themselves while passing the device around the table, that physical sequence explains the resulting seed values. Alphabetical sorting would tell a different story; sorting by an ordinary local best would tell a different one again. Neither transformation follows from the entry-order definition above.

There is a separate ambiguity between the seed in a bracket and the seed in a board code. `src/game/deck.ts` uses `seed` for the numeric input to deck generation. That number describes an arrangement of cards, not a person. For instance, the last number in `np1-4-73` has no relationship to the entrant in position 73, or to an entrant at all. The shared word is a vocabulary collision between two subjects.

[Competition deals](competition-deals.md) covers board identity. [Competition ties](competition-ties.md) covers result comparison. Neither calculation is present in the current pairing-free source, and neither is needed to explain the opening layout here.

## The small example catches a different mistake

With Ada, Bo, Cora and Dev as the complete entry list, the opening matches are Ada with Dev, followed by Bo with Cora. This is not obtained by taking the first two matches of the eight-person example. It is a bracket constructed from the smaller entry list.

That observation is useful when reviewing code that represents possible opponents by index. A table of entry positions can be applied to the corresponding list length. A table of names copied from another event cannot. The distinction becomes particularly clear when the same people participate in more than one demonstration but join in a different order.

It is also easy for a visual mockup to hide a mismatch between the text and the underlying order. A designer may draw a balanced-looking left and right side while the data is assembled in another traversal. The named example gives reviewers a shared reference independent of whether the visual bracket runs left to right, top to bottom or in a compact list.

## A pairing review on paper

A useful review exercise is to write the entry list once, assign its seed values, and trace each participant into the opening layout. In the eight-person example, every name appears once in the opening matches. Each opponent relationship can be checked in both directions: locating Ada also locates Hal; locating Hal leads back to Ada.

This exercise reveals three different classes of error. A duplicated participant is a membership problem. Correct opponents in the wrong middle positions are an ordering problem. Correct seed positions attached to the wrong names are an entry-mapping problem. They can produce similar screenshots, but they point to different mistakes in construction.

The exercise does not determine who advances. It simply makes the initial structure concrete enough to inspect. Event stages are discussed in [competition rounds](competition-rounds.md), and the relationship between an individual finish and local saved results is discussed in [competition scores](competition-scores.md).

For an engineer considering test coverage, the two named examples offer direct expectations rather than a second copy of the same pairing algorithm. A test that calculates its expected list with the same loop as the implementation is weak evidence: both can make the same ordering mistake. A literal list of expected opponent names exposes that mistake immediately.

## Unresolved pairing decisions

The pairing definition describes the two bracket shapes above. It does not describe a general event for every possible number of entrants. The project has no rule for a vacant place, a late arrival, a withdrawal, replacing a participant or rearranging an event after results exist. These are unresolved product questions rather than branches that can be inferred from a nearest power of two.

These boundaries are deliberate. The opening arrangement is small and explicit. General scheduling systems solve much larger problems, but importing their assumptions would add behavior beyond the current game and competition scope.

