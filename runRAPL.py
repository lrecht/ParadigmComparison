import pyRAPL
import argparse
import os
import fnmatch
import subprocess

parser = argparse.ArgumentParser()
becnhmarks_path = "./benchmarks"
all_paradigms = ["functional", "oop"]
all_languages = ["Python"]
language_discover_funcs = {}

#Setupsies
pyRAPL.setup()
csv_output = pyRAPL.outputs.CSVOutput('result.csv')
experimentIterations = 10


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
        return None #Override

    def get_run_command(self):
        return None #Override


class PythonProgram(Program):
    def __init__(self, path, paradigm):
        self.path = path
        self.paradigm = paradigm

    def get_build_command(self):
        return None

    def get_run_command(self):
        return "python3.8 " + self.path



def discover_programs(path, paradigms, languages):
    programs = []
    
    for lang in languages:
        lang_discover_func = language_discover_funcs[lang]
        discovered = lang_discover_func(path)

        for p in discovered:
            if p.paradigm in paradigms:
                programs.append(p)

    return programs


def get_benchmark_programs(benchmarks, paradigms, languages):
    benchmark_programs = []
    
    for benchmark_path in benchmarks:
        sub_dirs = [f.path for f in os.scandir(benchmark_path) if f.is_dir()]
        program_paths = []
        program_paths = program_paths + (discover_programs(benchmark_path, paradigms, languages))
        #program_paths = program_paths + [discover_programs(path, paradigms, languages) for path in sub_dirs]

        benchmark_programs = benchmark_programs + program_paths

    return benchmark_programs


def discover_python_program(path):
    results = []

    for _, _, files in os.walk(path):
        for name in files:
            programPath = path + '/' + name

            if fnmatch.fnmatch(name, "oop_py.py"):
                results.append(PythonProgram(programPath, "oop"))
            elif fnmatch.fnmatch(name, "functional_py.py"):
                results.append(PythonProgram(programPath, "functional"))

    return results

language_discover_funcs["Python"] = discover_python_program


def perform_benchmarks(benchmarks):
    benchmark_count = len(benchmarks)
    current_benchmark = 0

    for b in benchmarks:
        current_benchmark += 1

        print("Performing benchmark " + str(current_benchmark) + " of " + str(benchmark_count))

        if(b.get_build_command()):
            subprocess.run(b.get_build_command(), check=True)

        #The measuring equipment
        for _ in range(0, experimentIterations):
            meter = pyRAPL.Measurement(label=b.get_run_command())
            meter.begin()
            
            subprocess.run(b.get_run_command(), shell=True, check=True)

            meter.end()
            csv_output.add(meter.result)
            csv_output.save()


if __name__ == '__main__':
    parser.add_argument("-n", "--nobuild", help="Skips build step for benchmarks") #TODO: this
    parser.add_argument("-b", "--benchmarks", action=readable_dir, nargs='+', help="Run only specified benchmarks")
    parser.add_argument("-p", "--paradigm", choices=all_paradigms, help="Run only benchmarks for paradigm")
    parser.add_argument("-l", "--language", choices=all_languages, help="Run only benchmarks for language")

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


    benchmark_programs = get_benchmark_programs(benchmarks, paradigms, languages)

    perform_benchmarks(benchmark_programs)
