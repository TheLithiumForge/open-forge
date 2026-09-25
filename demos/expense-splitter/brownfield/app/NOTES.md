# Notes to self

Scratch notes, not documentation. Mostly things I had to learn the hard way.

- **Cents, always.** v0.1 stored amounts as floats. Three people splitting 10.00 came out as 3.3333333333333335 each, and after a month of dinners the balances drifted by a few cents that nobody could explain. Since v0.2 every amount is a whole number of cents, from parsing to saving. Don't reintroduce floats anywhere near money, including "just for the percentage".
- **Who gets the odd cent?** When 10.00 is split three ways, someone has to pay 3.34. Priya argued it should rotate. Sam said whoever paid should just eat it, because they chose the restaurant. We went with Sam's rule: the payer is listed first and gets the leftover cents. It's predictable, it never needs extra state, and nobody complains about a cent they chose to pay.
- **Shares must add up.** `addExpense` refuses any expense whose shares don't sum to the total. It caught two bugs already. Keep it.
- Percentage splits keep coming up. Not started. Whatever we do there has to follow the two rules above.
