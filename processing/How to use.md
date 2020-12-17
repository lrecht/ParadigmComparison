# How to use

The processing scripts are split into several different responsible classes, where the file `Process.py` is the main file to access the functionality.

The file `Process.py` currently has two modes:

1. Latex-mode
   * This mode is used to generate latex tables, with the statical results of the processing of observations
2. Plot-mode
   * This mode is used to verify latex results, in case some of the results look off or simply want to see boxplots of the different samples

## To use the files

To use the processing scripts simply write `python Process.py -f file.csv`
This is the simplest command, it does nothing but process the input file specified using the `-f` or `--file`. *This does not however produce any output*.

To get output simply append either `latex` or `plot` like so:

* `python Process.py -f file.csv latex`
* `python Process.py -f file.csv plot`

Further help and commands can be found by using the `-h` after either of the two commands above. Note that the simple command seen futher above, does not contain much help information using the `-h`.
