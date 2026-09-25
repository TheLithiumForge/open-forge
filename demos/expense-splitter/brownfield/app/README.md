# Expense Splitter

Split shared expenses between friends from the command line. Runs on Node.js 22.18 or later with no runtime dependencies and no build step. Node runs the TypeScript directly.

```sh
node src/cli.ts add "Dinner" 10.00 --paid-by sam --with priya,lee
node src/cli.ts list
node src/cli.ts balances
npm test
```

Type checking is optional: `npm install`, then `npm run typecheck`.

Expenses are saved to `expenses.json` in the current folder. Set `EXPENSES_FILE` to use another file.

## Not done yet

- Uneven splits, for example by percentage or by shares
- Settle-up suggestions: the fewest payments that clear every balance
- Editing or deleting an expense
