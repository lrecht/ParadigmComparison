from Thermal import Thermals
from BoxPlotter import BoxPlotter
from LatexPrinter import LatexPrinter
from CsvParser import CsvParser
from EDPPlotter import EDPPrinter
from typing import *
import argparse

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-f', '--file', required=True, help='The results file to process')
    subparsers = parser.add_subparsers()

    latex = subparsers.add_parser(name='latex')
    latex.set_defaults(which='latex')
    latex.add_argument('-o', '--output', type=str, help='Selects where the latex output should be saved')

    edp = subparsers.add_parser(name='edp')
    edp.set_defaults(which='edp')
    edp.add_argument('-w', '--weight', default=2, type=int, help='Sets the weight used to calculate the edp (1, 2 or 3)')
    edp.add_argument('-o', '--output', type=str, help='Selects where the edp output should be saved')

    plot = subparsers.add_parser(name='plot')
    plot.set_defaults(which='plot')
    plot.add_argument('-b', '--benchmark', nargs='*', help='Used to select which benchmark(s) to include in the plot comparison, leaving it empty means all benchmarks will be compared')
    plot.add_argument('-l', '--language', nargs='*', help='Used to select which language(s) to use when creating comparisons, leaving it empty means showing all languages')
    plot.add_argument('-m', '--metric', nargs='*', help='Used to select which metrics to include, leaving it empty means all metrics will be compared')
    plot.add_argument('-p', '--paradigm', nargs='*', help='Used to select which paradigms to exclude, leaving it empty means no paradigms will be excluded')

    thermal = subparsers.add_parser(name='thermal')
    thermal.set_defaults(which='thermal')
    thermal.add_argument('-t', '--type', choices=['plot', 'correlate'])
    thermal.add_argument('-b', '--benchmark', nargs='*', help='Used to select which benchmark(s) to include in the plot comparison, leaving it empty means all benchmarks will be compared')
    thermal.add_argument('-l', '--language', nargs='*', help='Used to select which language(s) to use when creating comparisons, leaving it empty means showing all languages')
    thermal.add_argument('-m', '--metric', nargs='*', help='Used to select which metrics to include, leaving it empty means all metrics will be compared')
    thermal.add_argument('-p', '--paradigm', nargs='*', help='Used to select which paradigms to exclude, leaving it empty means no paradigms will be excluded')

    args = parser.parse_args()

    # Read and parse CSV file
    results = CsvParser(sep=';').parse(args.file)

    # Print result file as latex table
    if args.which == 'latex':
        LatexPrinter(args.output).output(results)
    elif args.which == 'plot':
        BoxPlotter(results).plot(args)
    elif args.which == 'thermal':
        if args.type == 'plot':
            Thermals(results).plot(args)
        elif args.type == 'correlate':
            Thermals(results).correlate(args)
    elif args.which == 'edp':
        EDPPrinter(args.weight,args.output).output(results)
