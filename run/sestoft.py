from . import benchmark_utils as bc
import subprocess

#Performs the list of benchmarks and saves to results to output csv file
def perform_benchmarks(benchmarks, experiment_iterations, time_limit, output_file, skip_build, skip_runs):
    statistics, csv_output = bc.setup(output_file)
    benchmark_count = len(benchmarks)

    for index, b in enumerate(benchmarks):
        skipped = 0
        statistics.clear()
        print('\r' + "Performing benchmark " + str(index + 1) + " of " + str(benchmark_count), end='', flush=True)
        print("\n", b.path, flush=True)

        if(not skip_build and b.get_build_command()):
            subprocess.run(b.get_build_command(), shell=True, check=True, stdout=subprocess.DEVNULL)

        #The measuring equipment
        current = 0
        (max_iter, get_next_iter) = get_iter_options(time_limit, experiment_iterations)
        while(current < max_iter):
            res = bc.run(b)
            if all([res.pkg, res.dram]):
                if (skipped != skip_runs): 
                    skipped += 1
                else:
                    bc.handle_results(res, csv_output, statistics)
                    current = get_next_iter(res, current)
            else:
                bc.error_print("Failure in uptaining results from run: " + str(res))
        bc.save(statistics, csv_output, b.path)
    print('\n', flush=True)


def get_iter_options(max_time, max_iter):
    if max_time is not None:
        return (max_time, lambda x, current: current + (x.duration / 1_000_000))
    else:
        return (max_iter, lambda _, current: current + 1)
