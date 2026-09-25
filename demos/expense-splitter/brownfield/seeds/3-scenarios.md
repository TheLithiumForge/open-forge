# Level 3: brief plus scenarios

The level 2 brief with the exact results you expect, including the rounding cases.

Give your agent this request:

> Add percentage splits to the expense splitter.
>
> - `add` accepts `--percent sam=50,priya=30,lee=20` as an alternative to `--with`.
> - Percentages are whole numbers and must add up to 100.
> - Only the people listed pay a share. The payer doesn't have to be one of them.
> - Show the resulting split the same way `add` does today, and update the README.
>
> These must hold, each starting from an empty ledger:
>
> | Command                                                                     | Expected result                                                 |
> | --------------------------------------------------------------------------- | --------------------------------------------------------------- |
> | `add "Groceries" 100.00 --paid-by sam --percent sam=33,priya=33,lee=34`     | sam 33.00, priya 33.00, lee 34.00                               |
> | `add "Lunch" 10.01 --paid-by priya --percent priya=50,lee=50`               | priya 5.01, lee 5.00                                            |
> | `add "Lunch" 10.01 --paid-by lee --percent priya=50,lee=50`                 | priya 5.00, lee 5.01                                            |
> | `add "Gift" 20.00 --paid-by sam --percent priya=50,lee=50`, then `balances` | sam is owed 20.00, priya and lee each owe 10.00                 |
> | `add "Oops" 10.00 --paid-by sam --percent sam=50,priya=40`                  | Error: the percentages add up to 90, not 100. Nothing is saved. |
> | `add "Oops" 10.00 --paid-by sam --with lee --percent sam=50,lee=50`         | Error: use either `--with` or `--percent`. Nothing is saved.    |
> | `add "Dinner" 10.00 --paid-by sam --with priya,lee`                         | Unchanged: sam 3.34, priya 3.33, lee 3.33                       |
