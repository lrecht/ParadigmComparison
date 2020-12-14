from argparse import Namespace
import math
import itertools
from typing import List
from matplotlib import pyplot as plt
import numpy as np
from Result import Key, MetricType, Result

class Thermals():
    def __init__(self, result: Result):
        self.result = result
        self.colors = ['b','g','r','c','m','y']

    def __select(self, condition: List[str], otherwise: List[str]):
        return condition if condition else otherwise

    def __smooth(self, raw: List[float], rolling: int) -> List[float]:
        avg: List[float] = []
        res: List[float] = []
        for value in raw:
            if len(avg) == rolling:
                avg.pop()

            avg.insert(0, value)
            average = sum(avg) / len(avg)
            res.append(average)
        return res


    def correlate(self, conditions: Namespace) -> None:
        benchmark = self.__select(conditions.benchmark, self.result.benchmarks)
        language = self.__select(conditions.language, self.result.languages)
        metric = [ MetricType(x) for x in conditions.metric] if conditions.metric else [x for x in MetricType]

        for b in benchmark:
            for l in language:
                for paradigm in self.result.paradigms[b]:
                    if conditions.paradigm and paradigm in conditions.paradigm:
                        continue
                        
                    for (m1, m2) in itertools.combinations(metric, 2):
                        k1 = Key(b, paradigm, l, m1)
                        k2 = Key(b, paradigm, l, m2)
                        res1 = self.result.get_result(k1)
                        res2 = self.result.get_result(k2)
                        if res1.has_value and res2.has_value:
                            co = np.corrcoef(res1.get().results, res2.get().results)
                            print("[{0}, {1} - {2}]: {3} vs. {4}".format(b, l, paradigm, m1, m2))
                            print("{0}\n\n".format(co))


    def plot(self, conditions: Namespace) -> None:
        benchmark = self.__select(conditions.benchmark, self.result.benchmarks)
        language = self.__select(conditions.language, self.result.languages)
        metric = [ MetricType(x) for x in conditions.metric] if conditions.metric else [x for x in MetricType]

        for b in benchmark:
            for l in language:
                for paradigm in self.result.paradigms[b]:
                    if conditions.paradigm and paradigm in conditions.paradigm:
                        continue

                    key = Key(b, paradigm, l, MetricType('duration'))
                    res = self.result.get_raws(key, metric)
                    if res.has_value:
                        plot_vars = res.get()
                        fig, ax1 = plt.subplots()
                        for (i, met) in enumerate(plot_vars.keys()):
                            x = np.arange(len(plot_vars[met]))
                            format = '-' + self.colors[i]
                            values = self.__smooth(plot_vars[met], math.ceil(len(x) * 0.05))
                            ax1.plot(x, np.array(values), format)

                            # Ugly, but ensure that at most two scales can be created, more than that is a clusterfuck of scales
                            if i < 1:
                                ax1 = ax1.twinx()

                        fig.legend(plot_vars.keys(), loc='upper right')
                        plt.title("{0}: {1} - [{2}]".format(b, l, paradigm))
                        plt.show()
