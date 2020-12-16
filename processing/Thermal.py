from argparse import Namespace
import math
from typing import List, Tuple
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


    def __get_color(self, value: float) -> Tuple[int, int, int]:
        default = { "red": 255, "green": 0, "blue": 0 }
        new_range = value * 511
        whole = math.ceil(new_range)
        for _ in range(whole):
            if default['green'] != 255:
                default['green'] = default['green'] + 1
            elif default['green'] == 255:
                default['red'] = default['red'] - 1
        return (default['red'], default['green'], default['blue'])


    def correlate(self, conditions: Namespace) -> None:
        benchmark = self.__select(conditions.benchmark, self.result.benchmarks)
        language = self.__select(conditions.language, self.result.languages)

        for b in benchmark:
            for l in language:
                line = b.replace("_", " ").capitalize()
                for para in ['procedural', 'oop', 'functional']:
                    for (m1, m2) in [(MetricType.TEMP, MetricType.DURATION), (MetricType.TEMP, MetricType.DRAM), (MetricType.TEMP, MetricType.PACKAGE)]:
                        line = line + " & "                        
                        if para in self.result.paradigms[b]:
                            if conditions.paradigm and para in conditions.paradigm:
                                continue

                            k1 = Key(b, para, l, m1)
                            k2 = Key(b, para, l, m2)
                            res1 = self.result.get_result(k1)
                            res2 = self.result.get_result(k2)
                            if res1.has_value and res2.has_value:
                                co = np.corrcoef(res1.get().results, res2.get().results)
                                red, green, blue = self.__get_color(abs(co[1,0]))
                                line = line + '\\cellcolor[RGB]{{{0}, {1}, {2}}}'.format(red, green, blue)
                print(line + "\\\\ \hline")



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
