import itertools
import os
from typing import *
from Result import Key, MetricType, Result

class EDPPrinter():
    def __init__(self, weight: int,outputfile: str):
        self.weight = weight
        self.outputfile = outputfile

    def computeEDP(self,results: Result) -> Dict[Key, float]:
        edp = {}
        for k in results.observations:
            # Only need one result pr. benchmark, language and paradigm
            if k.metric is not MetricType.DURATION:
                continue

            # Find results for time, dram and package
            time = results.observations[k]
            k = Key(k.benchmark, k.paradigm, k.language, MetricType.PACKAGE)
            cpu = results.observations[k]
            k = Key(k.benchmark, k.paradigm, k.language, MetricType.DRAM)
            ram = results.observations[k]

            length = len(time)
            sumTime = sum(time)
            sumCPU = sum(cpu)
            sumRAM = sum(ram)

            # Summarise the results

            # Find avg's and convert to seconds and joule
            avgTime = sumTime / length * 0.000001
            avgCPU = sumCPU / length * 0.000001
            avgRAM = sumRAM / length * 0.000001

            edp[k] = avgTime ** self.weight * (avgCPU+avgRAM)
        
        return edp

    # Creates output as: |       c#      |      f#       |
    #                    | pp | oop | fp | pp | oop | fp |
    # Output fits under this table header
    def createOutput(self,edp: Dict[Key, float],results: Result) -> List[str]:
        lines = []
        for benchmark in results.benchmarks:
            line = benchmark.replace('_', ' ').capitalize()
            for lang in results.languages:
                for par in ['procedural','oop','functional']:
                    line = line + " &"
                    if par in results.paradigms[benchmark]:
                        res = edp[Key(benchmark,par,lang,MetricType.DRAM)]
                        line = line + " %s" % float('%.5g' % res)
            line = line + " \\\\ \hline \n"
            lines.append(line)
        return lines

    def save(self, lines):
        with open(self.outputfile, 'w+') as output:
            output.writelines(lines)

    def output(self, results):
        edp = self.computeEDP(results)
        out = self.createOutput(edp,results)
        self.save(out)
