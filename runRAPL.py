from math import sqrt
import pyRAPL
import argparse
import os
import fnmatch
import subprocess
import math

parser = argparse.ArgumentParser()
benchmarks_path = "./benchmarks"
all_paradigms = ["functional", "oop", "procedural"]
all_languages = ["c#", "f#"]
language_discover_funcs = {}


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


class Program:
    def __init__(self, path, paradigm):
        self.path = path
        self.paradigm = paradigm

    def get_build_command(self):
        raise NotImplementedError("Please Implement this method")

    def get_run_command(self):
        raise NotImplementedError("Please Implement this method")


class Dotnet_Program(Program):
    def get_build_command(self):
        return "dotnet build --configuration Release --nologo --verbosity quiet " + self.path


class C_Sharp_Program(Dotnet_Program):
    def get_run_command(self):
        command = self.path + '/bin/Release/netcoreapp3.1/'
        return command + self.paradigm + "_c#"


class F_Sharp_Program(Dotnet_Program):
    def get_run_command(self):
        command = self.path + '/bin/Release/netcoreapp3.1/'
        return command + self.paradigm + "_f#"


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


#Discovers c# project folders based on naming convention (functional_c# or oop_c#)
def discover_csharp_program(path):
    results = []

    for name in os.listdir(path):
        if not os.path.isdir(path + '/' + name):
            continue

        program_path = path + '/' + name

        if fnmatch.fnmatch(name, "*_c#"):
            results.append(C_Sharp_Program(program_path, name.split('_')[0]))

    return results

# Adds function to discover functions dictionary
language_discover_funcs["c#"] = discover_csharp_program


#Discovers f# project folders based on naming convention (functional_f# or oop_f#)
def discover_fsharp_program(path):
    results = []

    for name in os.listdir(path):
        if not os.path.isdir(path + '/' + name):
            continue

        program_path = path + '/' + name

        if fnmatch.fnmatch(name, "*_f#"):
            results.append(F_Sharp_Program(program_path, name.split('_')[0]))

    return results

# Adds function to discover functions dictionary
language_discover_funcs["f#"] = discover_fsharp_program


class Stats():
    """A class to simplify the statistical computations"""

    def Clear(self):
        """After each benchmark, the instance values can be cleared using this function"""
        self.measures = []
        self.mean = 0
        self.std = 0
        self.error_margin = 0
        self.error_percent = 0

    def add_measurement(self, measure):
        """A small method to add measurements to the instance"""
        self.measures.append(measure)

    def compute_results(self):
        """Calls all of the statistical calculation functions in the right order (Results will occupy the instance variables)"""
        self.mean = self.get_mean()
        self.std = self.get_deviation() * 2 # The times two is to produce 95% confidence
        self.error_margin = self.get_error_margin()
        self.error_percent = self.get_error_percent()

    def get_mean(self):
        """Will return the mean value, based on the current measurements"""
        return sum(self.measures) / len(self.measures)

    def get_deviation(self):
        """Will return the standard deviation, based on the current measurements (Mean must be calculated first)"""
        sqrt_sum = 0
        for val in self.measures:
            sqrt_sum += (val - self.mean)**2
        
        subres = sqrt_sum
        if len(self.measures) > 1:
            subres = sqrt_sum / (len(self.measures) - 1)
        return math.sqrt(subres)

    def get_error_margin(self):
        """Will return the error margin, based on the current measurements (Standard deviation must be calculated first)"""
        return self.std / math.sqrt(len(self.measures))

    def get_error_percent(self):
        """Will return the error margin in percent of the mean (Error margin must be calculated first)"""
        return (self.error_margin / self.mean)

    def to_pretty_string(self):
        """Will return the 'mean', 'error margin', and 'error percent' in a readable format"""
        mean = "{0:.2f}".format(self.mean)
        error = "{0:.2f}".format(self.error_margin)
        percent = "{0:.2%}".format(self.error_percent)
        return "Results: {0} ± {1} (± {2})\n".format(mean, error, percent)


#Performs the list of benchmarks and saves to results to output csv file
def perform_benchmarks(benchmarks, experiment_iterations, output_file, skip_build):
    #Setupsies
    pyRAPL.setup()
    statistics = Stats()
    csv_output = pyRAPL.outputs.CSVOutput(output_file)

    benchmark_count = len(benchmarks)
    current_benchmark = 0

    for b in benchmarks:
        statistics.Clear()
        current_benchmark += 1

        print('\r' + "Performing benchmark " + str(current_benchmark) + " of " + str(benchmark_count), end='', flush=True)
        print("\n", b.path)

        if(not skip_build and b.get_build_command()):
            subprocess.run(b.get_build_command(), shell=True, check=True, stdout=subprocess.DEVNULL)

        #The measuring equipment
        for _ in range(0, experiment_iterations):
            meter = pyRAPL.Measurement(label=b.get_run_command())
            meter.begin()
            
            subprocess.run(b.get_run_command(), shell=True, check=True, stdout=subprocess.DEVNULL)

            meter.end()
            statistics.add_measurement(meter.result.duration)
            csv_output.add(meter.result)

        statistics.compute_results()
        print(statistics.to_pretty_string())
        csv_output.save()
    
    print('\n')


if __name__ == '__main__':
    parser.add_argument("-n", "--nobuild", action='store_true', help="Skips build step for benchmarks")
    parser.add_argument("-b", "--benchmarks", action=readable_dir, nargs='+', help="Run only specified benchmarks")
    parser.add_argument("-p", "--paradigm", choices=all_paradigms, help="Run only benchmarks for paradigm")
    parser.add_argument("-l", "--language", choices=all_languages, help="Run only benchmarks for language")
    parser.add_argument("-o", "--output", default="results.csv", help="Output csv file for results. Default is results.csv")
    parser.add_argument("-i", "--iterations", default=10, type=int, help="Number of iterations for each benchmark")

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

    skip_build = args.nobuild
    output_file = args.output
    iterations = args.iterations

    benchmark_programs = get_benchmark_programs(benchmarks, paradigms, languages)

    perform_benchmarks(benchmark_programs, iterations, output_file, skip_build)
