import math
import benchmarks_common as bc
import stats as stat
import subprocess

ITERATIONS = 100

#Performs the list of benchmarks and saves results to ouput csv file
def perform_benchmarks(benchmark_programs, output_file, time_limit_secs):
    stats, csv_output = bc.setup(output_file)
    current_benchmark = 0
    benchmark_count = len(benchmark_programs)
    
    for b in benchmark_programs:
        current_benchmark += 1

        print("Performing benchmark " + str(current_benchmark) + " of " + str(benchmark_count), flush=True)
        print(b.path, flush=True)
        
        subprocess.run(b.get_build_command(), shell=True, check=True, stdout=subprocess.DEVNULL)

        stats.clear()

        #Pilot run
        run_benchmark(b, stats, ITERATIONS, csv_output)
        num_runs = math.ceil(compute_runs(stats))
        
        if not is_enough_runs(num_runs, stats, ITERATIONS):
            print("Performing ",num_runs - ITERATIONS," addtitional runs to achive the desired statistical error", flush=True)
            run_benchmark(b, stats, num_runs - ITERATIONS, csv_output, time_limit_secs)
        bc.save(stats, csv_output, b.path)


def is_enough_runs(num_runs, stats, iterations):
    if iterations >= num_runs:
        return True
    else:
        execute = stats.execution_time.error_percent
        memory = stats.ram.error_percent
        package = stats.package.error_percent
        return all(val <= 0.005 for val in [execute, memory, package])


def run_benchmark(benchmark, stats, iterations, csv_output, time_limit_secs=None):
    time = 0
    for i in range(0, iterations):
        print("\r" + str(i + 1) + " of " + str(iterations), end="", flush=True)
        res = bc.run(benchmark)

        # Break on time limit
        if time_limit_secs and time >= time_limit_secs:
            print("\nEnding benchmark due to time constraints")
            break
    
        # Verify that RAPL values exist
        if all([res.pkg, res.dram]):
            bc.handle_results(res, csv_output, stats)
            time += (res.duration / 1_000_000)
        else:
            bc.error_print("Failure in uptaining results from run: " + str(res) + "\n")
    
    print("", flush=True)


def compute_runs(stats):
    stats.compute()
    num_runs = []
    z_score = 1.96 # For 95% confidence
    for stat in [stats.execution_time, stats.package, stats.ram]:
        top = z_score * stat.sd
        error = stat.mean * 0.005
        num_runs.append((top / error)**2)
    return max(num_runs)
