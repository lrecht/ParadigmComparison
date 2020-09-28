import pyRAPL
import argparse
import os
import fnmatch
import subprocess

parser = argparse.ArgumentParser()
becnhmarks_path = "./benchmarks"
all_paradigms = ["functional", "oop", "procedual"]
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

    for _, dirs, _ in os.walk(path):
        for name in dirs:
            program_path = path + '/' + name

            if fnmatch.fnmatch(name, "*_c#"):
                results.append(C_Sharp_Program(program_path, name.split('_')[0]))

    return results

# Adds function to discover functions dictionary
language_discover_funcs["c#"] = discover_csharp_program


#Discovers f# project folders based on naming convention (functional_f# or oop_f#)
def discover_fsharp_program(path):
    results = []

    for _, dirs, _ in os.walk(path):
        for name in dirs:
            program_path = path + '/' + name

            if fnmatch.fnmatch(name, "*_f#"):
                results.append(F_Sharp_Program(program_path, name.split('_')[0]))

    return results

# Adds function to discover functions dictionary
language_discover_funcs["f#"] = discover_fsharp_program


#Performs the list of benchmarks and saves to results to output csv file
def perform_benchmarks(benchmarks, output_file, skip_build):
    #Setupsies
    experimentIterations = 10
    pyRAPL.setup()
    csv_output = pyRAPL.outputs.CSVOutput(output_file)

    benchmark_count = len(benchmarks)
    current_benchmark = 0

    for b in benchmarks:
        current_benchmark += 1

        print('\r' + "Performing benchmark " + str(current_benchmark) + " of " + str(benchmark_count), end='', flush=True)

        if(not skip_build and b.get_build_command()):
            subprocess.run(b.get_build_command(), shell=True, check=True, stdout=subprocess.DEVNULL)

        #The measuring equipment
        for _ in range(0, experimentIterations):
            meter = pyRAPL.Measurement(label=b.get_run_command())
            meter.begin()
            
            subprocess.run(b.get_run_command(), shell=True, check=True)

            meter.end()
            csv_output.add(meter.result)

        csv_output.save()


if __name__ == '__main__':
    parser.add_argument("-n", "--nobuild", action='store_true', help="Skips build step for benchmarks")
    parser.add_argument("-b", "--benchmarks", action=readable_dir, nargs='+', help="Run only specified benchmarks")
    parser.add_argument("-p", "--paradigm", choices=all_paradigms, help="Run only benchmarks for paradigm")
    parser.add_argument("-l", "--language", choices=all_languages, help="Run only benchmarks for language")
    parser.add_argument("-o", "--output", default="results.csv", help="Output csv file for results. Default is results.csv")

    args = parser.parse_args()

    benchmarks = []
    paradigms = []
    languages = []

    #If no benchmarks are given, all benchmarks are to be run
    if args.benchmarks:
        benchmarks = args.benchmarks
    else:
        benchmarks = [f.path for f in os.scandir(becnhmarks_path) if f.is_dir()]
    
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

    benchmark_programs = get_benchmark_programs(benchmarks, paradigms, languages)

    perform_benchmarks(benchmark_programs, output_file, skip_build)
