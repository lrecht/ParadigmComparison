import math
from . import benchmark_utils as bm_utils
import subprocess

SAMPLE_ITERATIONS = 100

# Executes the list of benchmarks and saves results to ouput csv file
def perform_benchmarks(benchmark_programs, output_file, time_limit_secs):
    stats, csv_output = bm_utils.setup(output_file)
    benchmark_count = len(benchmark_programs)

    for index, current_benchmark in enumerate(benchmark_programs):
        print("Performing benchmark " + str(index + 1) + " of " + str(benchmark_count), flush=True)
        print(current_benchmark.path, flush=True)

        # Build benchmark
        subprocess.run(current_benchmark.get_build_command(),
                       shell=True, check=True, stdout=subprocess.DEVNULL)
        stats.clear()

        # Run random sample
        run_benchmark(current_benchmark, stats, SAMPLE_ITERATIONS, csv_output)
        num_runs = math.ceil(compute_sample_size(stats))

        # Maybe conduct additional runs
        if not is_enough_runs(num_runs, stats, SAMPLE_ITERATIONS):
            print("Performing ", num_runs - SAMPLE_ITERATIONS,
                  " addtitional runs to achive the desired statistical error", flush=True)
            run_benchmark(current_benchmark, stats, num_runs - SAMPLE_ITERATIONS, csv_output, time_limit_secs)
        bm_utils.save(stats, csv_output, current_benchmark.path)


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
    for i in range(iterations):
        print("\r" + str(i + 1) + " of " + str(iterations), end="", flush=True)
        results, temp_res = bm_utils.run(benchmark)

        # Break on time limit
        if time_limit_secs and time >= time_limit_secs:
            print("\nEnding benchmark due to time constraints")
            break

        # Verify that RAPL values exist
        if all([results.pkg, results.dram]):
            bm_utils.handle_results(results, temp_res, csv_output, stats)
            time += (results.duration / 1_000_000)
        else:
            bm_utils.error_print(
                "Failure in obtaining results from run: " + str(results) + "\n")

    print("", flush=True)


# This is the Cochran formula
def compute_sample_size(stats):
    stats.compute()
    num_runs = []
    z_score = 1.96  # For 95% confidence
    for metric in [stats.execution_time, stats.package, stats.ram]:
        top = z_score * metric.sd
        error = metric.mean * 0.005
        num_runs.append((top / error)**2)
    return max(num_runs)
