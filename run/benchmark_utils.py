import sys
import pyRAPL
import stats as stat
import subprocess

def error_print(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)


def run(benchmark):
    meter = pyRAPL.Measurement(label=benchmark.get_run_command())
    meter.begin()
    subprocess.run(benchmark.get_run_command(), shell=True, check=True, stdout=subprocess.DEVNULL)
    meter.end()
    return meter.result


def handle_results(res, raw_results_csv, stats):
    stats.add(res)
    raw_results_csv.add(res)

def setup(output_file):
    pyRAPL.setup()
    stats = stat.Aggregator(output_file)
    csv_output = pyRAPL.outputs.CSVOutput(output_file)
    return (stats, csv_output)

def save(stats, csv, path):
    stats.compute()
    stats.save(path)
    csv.save()
