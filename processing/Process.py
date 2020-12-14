from BoxPlotter import BoxPlotter
from LatexPrinter import LatexPrinter
from CsvParser import CsvParser
from typing import *
import argparse

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-f', '--file', required=True, help='The results file to process')
    subparsers = parser.add_subparsers()

    latex = subparsers.add_parser(name='latex')
    latex.set_defaults(which='latex')
    latex.add_argument('-o', '--output', type=str, help='Selects where the latex output should be saved')

    plot = subparsers.add_parser(name='plot')
    plot.set_defaults(which='plot')
    plot.add_argument('-b', '--benchmark', help='Used to select which benchmark(s) to include in the plot comparison, leaving it empty means all benchmarks will be compared')
    plot.add_argument('-l', '--language', help='Used to select which language(s) to use when creating comparisons, leaving it empty means showing all languages')
    plot.add_argument('-m', '--metric', help='Used to select which metrics to include, leaving it empty means all metrics will be compared')
    plot.add_argument('-p', '--paradigm', help='Used to select which paradigms to exclude, leaving it empty means no paradigms will be excluded')

    args = parser.parse_args()

    # Read and parse CSV file
    results = CsvParser(sep=';').parse(args.file)

    # Print result file as latex table
    if args.which == 'latex':
        LatexPrinter(args.output).output(results)
    elif args.which == 'plot':
        BoxPlotter(results).plot(args)