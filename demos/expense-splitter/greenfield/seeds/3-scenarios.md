# Level 3: brief plus scenarios

The level 2 brief with the exact results you expect, including how rounding works.

Give your agent this request in an empty project:

> Build a small command-line tool that helps a group of friends split shared expenses and see who owes whom.
>
> - **Who:** a few friends sharing costs on a trip or in a flat. One of them runs the tool.
> - **It must:** add an expense that one person paid and several people share evenly, list the expenses, and show each person's balance.
> - **Constraints:** Node.js 22.18 or later, TypeScript that Node runs directly, no runtime dependencies, expenses saved to a JSON file, tests with `node:test`.
> - **Not now:** accounts, several currencies, a web interface, uneven splits.
>
> These must hold, each starting from an empty ledger:
>
> | Action                                                   | Expected result                                                      |
> | -------------------------------------------------------- | -------------------------------------------------------------------- |
> | Sam pays 10.00 for dinner, shared by Sam, Priya, and Lee | Sam 3.34, Priya 3.33, Lee 3.33. The payer absorbs the leftover cent. |
> | Then show balances                                       | Sam is owed 6.66, Priya and Lee each owe 3.33                        |
> | Priya pays 0.05 for a taxi, shared with Sam              | Priya 0.03, Sam 0.02                                                 |
> | Add an expense of 1.234                                  | Rejected with a clear message. Nothing is saved.                     |
> | Add an expense shared by nobody                          | Rejected with a clear message. Nothing is saved.                     |
> | Look at the saved file                                   | Every amount is stored as a whole number of cents                    |
