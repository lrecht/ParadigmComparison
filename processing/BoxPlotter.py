from argparse import Namespace
import matplotlib.pyplot as plt
from typing import List
from Result import Key, MetricType, Result


class BoxPlotter():
    def __init__(self, result: Result):
        self.result: Result = result

    def __select(self, condition: List[str], otherwise: List[str]):
        return condition if condition else otherwise

    def plot(self, conditions: Namespace) -> None:
        benchmark = self.__select([conditions.benchmark], self.result.benchmarks)
        language = self.__select([conditions.language], self.result.languages)
        metric = [conditions.metric] if conditions.metric else [x for x in MetricType]

        for b in benchmark:
            for l in language:
                for m in metric:
                    plot_vars = {}
                    for paradigm in self.result.paradigms[b]:
                        if conditions.paradigm and paradigm in conditions.paradigm:
                            continue
                        key = Key(b, paradigm, l, m)
                        res = self.result.get_result(key)
                        plot_vars[paradigm] = res.map(lambda x: x.results).or_else([])

                    _, ax = plt.subplots()
                    ax.boxplot(plot_vars.values(), showfliers=False)
                    #ax.set_yscale("log")
                    ax.set_title("{0} ({1}) - {2}".format(b, l, m.name))
                    #ax.set_ylabel("Power in joules")
                    ax.set_xticklabels(plot_vars.keys())
                    plt.show()
