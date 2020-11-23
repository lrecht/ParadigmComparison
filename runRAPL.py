import argparse
from program import *
from datetime import datetime
import email_service as es
import sys
import sestoft
import cochran

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
    parser.add_argument("--sestoft-approach", action='store_true', help="Old approach to run specified number of runs or of a specified amount of time")

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

    if args.sestoft_approach:
        sestoft.perform_benchmarks(benchmark_programs, iterations, time_limit, output_file, skip_build, skip_runs)
    else:
        cochran.perform_benchmarks(benchmark_programs, output_file)

    if(email is not None):
        es.send_results(email, output_file)
