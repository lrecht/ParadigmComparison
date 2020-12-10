import sys
import pyRAPL
from pyRAPL import Result, Measurement
from . import stats as stat
import subprocess
from .temperature import Temperature_Summary, get_temp_celsius
from . import csv_utils as cu
from typing import Tuple


def error_print(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)


def run(benchmark) -> Tuple[Result, Temperature_Summary]:
    meter = Measurement(label=benchmark.get_run_command())
    temp_before = get_temp_celsius()
    meter.begin()
    subprocess.run(benchmark.get_run_command(), shell=True, check=True, stdout=subprocess.DEVNULL)
    meter.end()
    temp_after = get_temp_celsius()
    return (meter.result, Temperature_Summary(temp_before, temp_after))


def handle_results(res, temp_res, raw_results_csv, stats):
    stats.add(res)
    raw_results_csv.add(res, temp_res)

def setup(output_file):
    pyRAPL.setup()
    stats = stat.Aggregator(output_file)
    csv_output = cu.CSV_Output(output_file)
    return (stats, csv_output)

def save(stats, csv, path):
    stats.compute()
    stats.save(path)
    csv.save()
