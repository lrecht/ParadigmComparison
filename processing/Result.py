from numpy.core.fromnumeric import mean
from scipy import stats
from Option import Option
from enum import Enum
from typing import *


class MetricType(Enum):
    """An enum of the different metrics recorded for each observation"""
    DURATION = 'duration'
    DRAM = 'dram'
    PACKAGE = 'pkg'
    TEMP = 'temp'


class Key(NamedTuple):
    benchmark: str
    paradigm: str
    language: str
    metric: MetricType


class Observation(NamedTuple):
    """Represents a single benchmark observation"""
    benchmark: str
    paradigm: str
    language: str
    duration: str
    package: str
    dram: str
    temp_before: str
    temp_after: str


class Metric(NamedTuple):
    key: Key
    results: List[float]


class StatResult(NamedTuple):
    """Represents a single comparison between two samples"""
    FirstParadigm: str
    SecondParadigm: str
    Result: Literal['<', '=', '>']


class Result():
    def __init__(self):
        self.observations: Dict[Key, List[float]] = {}
        self.benchmarks: List[str] = []
        self.paradigms: Dict[str, List[str]] = {}
        self.languages: List[str] = []


    def __get_key(self, obs: Observation, metric: MetricType) -> Key:
        return Key(obs.benchmark, obs.paradigm, obs.language, metric)


    def __add(self, obs: Observation, metric: MetricType, value: float) -> None:
        duration_key = self.__get_key(obs, metric)
        if duration_key not in self.observations:
            self.observations[duration_key] = []
        self.observations[duration_key].append(value)


    def add(self, obs: Observation) -> None:
        if obs.benchmark not in self.benchmarks:
            self.benchmarks.append(obs.benchmark)
            self.paradigms[obs.benchmark] = []

        if obs.paradigm not in self.paradigms[obs.benchmark]:
            self.paradigms[obs.benchmark].append(obs.paradigm)

        if obs.language not in self.languages:
            self.languages.append(obs.language)

        self.__add(obs, MetricType.DURATION, float(obs.duration))
        self.__add(obs, MetricType.PACKAGE, float(obs.package))
        self.__add(obs, MetricType.DRAM, float(obs.dram))
        self.__add(obs, MetricType.TEMP, (float(obs.temp_before) + float(obs.temp_after)) / 2)


    def __get_ordering(self, obs1: List[float], obs2: List[float], p_value: float) -> Literal['<', '=', '>']:
        """
        Compares the p-value from the t-test of the two samples and returns the statistical significant ordering
        The '=' ordering operator is used to signify no statistical significant ordering for the two samples
        """
        P_LIMIT = 0.05
        if p_value < P_LIMIT:
            diff = mean(obs1) - mean(obs2)
            if diff > 0:
                return '>'
            elif diff < 0:
                return '<'
        return '='


    def test(self, k1: Key, k2: Key) -> StatResult:
        """Compares two samples with a t-test and returns the statistically significant ordering if any exists"""
        obs1 = self.observations[k1]
        obs2 = self.observations[k2]
        _, p_value = stats.ttest_ind(obs1, obs2, equal_var=False)
        return StatResult(k1.paradigm, k2.paradigm, self.__get_ordering(obs1, obs2, p_value))
        

    def get_result(self, key: Key) -> Option[Metric]:
        if key in self.observations:
            met = Metric(key, self.observations[key])
            return Option(met)
        else:
            return Option.empty()


    def get_raws(self, key: Key, metrics: List[MetricType]) -> Option[Dict[MetricType, List[float]]]:
        result_map: Dict[MetricType, List[float]] = {}
        for metric in metrics:
            new_key = Key(key.benchmark, key.paradigm, key.language, metric)
            opt_res = self.get_result(new_key)
            if opt_res.has_value:
                result_map[metric] = opt_res.get().results
        if len(result_map.keys()) == len(metrics):
            return Option(result_map)
        else:
            return Option.empty()