import pyRAPL
import argparse
import subprocess
import stats as stat
from program import *
from datetime import datetime
import email_service as es
import sys

parser = argparse.ArgumentParser()
benchmarks_path = "./benchmarks"
all_paradigms = ["functional", "oop", "procedural"]
all_languages = ["c#", "f#"]
language_discover_funcs = {}

#sys.stderr.line_buffering=False
def error_print(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)

#Used to validate benchmark folders
class readable_dir(argparse.Action):
    def __call__(self, parser, namespace, values, option_string=None):
        for prospective_dir in values:
            if not os.path.isdir(prospective_dir):
                raise argparse.ArgumentTypeError("readable_dir:{0} is not a valid path".format(prospective_dir))
            if os.access(prospective_dir, os.R_OK):
                dirs = getattr(namespace, self.dest)
                if dirs: 
                    dirs.append(prospective_dir) 
                else:
                    dirs = [prospective_dir]
                setattr(namespace, self.dest, dirs)
            else:
                raise argparse.ArgumentTypeError("readable_dir:{0} is not a readable dir".format(prospective_dir))


#Fetches programs to run based on arguments
def discover_programs(path, paradigms, languages):
    programs = []
    
    for lang in languages:
        lang_discover_func = language_discover_funcs[lang]
        discovered = lang_discover_func(path)

        for p in discovered:
            if p.paradigm in paradigms:
                programs.append(p)

    return programs


#Goes through benchmark folders to find benchmarks
def get_benchmark_programs(benchmarks, paradigms, languages):
    benchmark_programs = []
    
    for benchmark_path in benchmarks:
        program_paths = discover_programs(benchmark_path, paradigms, languages)

        benchmark_programs = benchmark_programs + program_paths

    return benchmark_programs

# Adds function to discover functions dictionary
language_discover_funcs["c#"] = discover_csharp_program

# Adds function to discover functions dictionary
language_discover_funcs["f#"] = discover_fsharp_program


#Performs the list of benchmarks and saves to results to output csv file
def perform_benchmarks(benchmarks, experiment_iterations, time_limit, output_file, skip_build, skip_runs):
    #Setupsies
    pyRAPL.setup()
    statistics = stat.Aggregator(output_file)
    csv_output = pyRAPL.outputs.CSVOutput(output_file)

    benchmark_count = len(benchmarks)
    current_benchmark = 0

    for b in benchmarks:
        skipped = 0
        statistics.clear()
        current_benchmark += 1

        print('\r' + "Performing benchmark " + str(current_benchmark) + " of " + str(benchmark_count), end='', flush=True)
        print("\n", b.path, flush=True)

        if(not skip_build and b.get_build_command()):
            subprocess.run(b.get_build_command(), shell=True, check=True, stdout=subprocess.DEVNULL)

        #The measuring equipment
        current = 0
        (max_iter, get_next_iter) = get_iter_options(time_limit, experiment_iterations)
        while(current < max_iter):
            res = run(b)
            if all([res.pkg, res.dram]):
                if (skipped != skip_runs): 
                    skipped += 1
                else:
                    handle_results(res, csv_output, statistics)
                    current = get_next_iter(res, current)
            else:
                error_print("Failure in uptaining results from run: " + str(res))

        statistics.compute()
        statistics.save(b.path)
        csv_output.save()
    
    print('\n')


def get_iter_options(max_time, max_iter):
    if max_time is not None:
        return (max_time, lambda x, current: current + (x.duration / 1_000_000))
    else:
        return (max_iter, lambda _, current: current + 1)


def run(benchmark):
    meter = pyRAPL.Measurement(label=benchmark.get_run_command())
    meter.begin()
    
    subprocess.run(benchmark.get_run_command(), shell=True, check=True, stdout=subprocess.DEVNULL)

    meter.end()
    return meter.result


def handle_results(res, raw_results_csv, stats):
    stats.add(res)
    raw_results_csv.add(res)


if __name__ == '__main__':
    parser.add_argument("-n", "--nobuild", action='store_true', help="Skips build step for benchmarks")
    parser.add_argument("-b", "--benchmarks", action=readable_dir, nargs='+', help="Run only specified benchmarks")
    parser.add_argument("-p", "--paradigm", choices=all_paradigms, help="Run only benchmarks for paradigm")
    parser.add_argument("-l", "--language", choices=all_languages, help="Run only benchmarks for language")
    parser.add_argument("-o", "--output", default="results.csv", help="Output csv file for results. Default is results.csv")
    parser.add_argument("-i", "--iterations", default=10, type=int, help="Number of iterations for each benchmark")
    parser.add_argument("-t", "--time-limit", type=int, help="Number of seconds to continousely run each benchmark")
    parser.add_argument("-e", "--send-results-email", type=str, help="Send email containing statistical results")
    parser.add_argument("-s", "--skip-runs", type=int, default=0, help="Skip first n runs of each benchmark to stabilise results")

    args = parser.parse_args()

    benchmarks = []
    paradigms = []
    languages = []

    #If no benchmarks are given, all benchmarks are to be run
    if args.benchmarks:
        benchmarks = args.benchmarks
    else:
        benchmarks = [f.path for f in os.scandir(benchmarks_path) if f.is_dir()]
    
    #If no paradigm is given, then all paradigms are to be run
    if args.paradigm:
        paradigms = [args.paradigm]
    else:
        paradigms = all_paradigms

    #If no language is given, then all languages are to be run
    if args.language:
        languages = [args.language]
    else:
        languages = all_languages


    skip_build      = args.nobuild
    output_file     = "[{0}]{1}".format(datetime.now().isoformat(),args.output)
    iterations      = args.iterations
    time_limit      = args.time_limit
    email           = args.send_results_email
    skip_runs       = args.skip_runs

    benchmark_programs = get_benchmark_programs(benchmarks, paradigms, languages)

    perform_benchmarks(benchmark_programs, iterations, time_limit, output_file, skip_build, skip_runs)
    if(email is not None):
        es.send_results(email, output_file)
