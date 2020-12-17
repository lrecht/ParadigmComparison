import itertools
import os
from typing import *
from Result import Key, MetricType, Result


class LatexPrinter():
    def __init__(self, filepath: str):
        self.path = filepath


    def get_comparison(self, paradigms: List[str]) -> Iterator[Tuple[str, str]]:
        paradigms.sort(reverse=True)
        return itertools.combinations(paradigms, 2)


    def save(self, lines: List[str]) -> None:
        with open(self.path, 'w+') as output:
            output.writelines(lines)


    def output(self, result: Result) -> None:
        latex_lines: List[str] = []
        for language in result.languages:
            lines = self.results_per_language(result, language)
            latex_lines = latex_lines + lines
            latex_lines.append("\n")
        self.save(latex_lines)


    def results_per_language(self, result: Result, language: str) -> List[str]:
        """Creates the latex code for all benchmarks given a specific language"""
        lines: List[str] = []
        for benchmark in result.benchmarks:
            line = benchmark.replace('_', ' ')
            for (p1, p2) in self.get_comparison(result.paradigms[benchmark]):
                for metric in MetricType:
                    k1 = Key(benchmark, p1, language, metric)
                    k2 = Key(benchmark, p2, language, metric)
                    stat_res = result.test(k1, k2)
                    line = line + f' & {stat_res.Result}'
                    print(stat_res)
            line = line + " \\\\ \hline \n"
            lines.append(line)
        #lines.sort() # <-- To sort benchmarks alphabetically, uncomment this line
        return lines
