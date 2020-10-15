import math

class Stats():
    """A class to simplify the statistical computations"""
    def __init__(self, output_file):
        name = output_file.split(".csv")[0]
        self.file_name = name + "_stats.csv"
        with open(self.file_name, "w") as stats_file:
            stats_file.write("Name,Mean,Error Margin,Error Margin (%),Runs\n")
        

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

    def save(self, benchmark_name):
        with open(self.file_name, "a+") as stats:
            stats.write("{0},{1:.2f},{2:.2f},{3:.2%},{4}\n".format(benchmark_name, self.mean, self.error_margin, self.error_percent, len(self.measures)))


    def to_pretty_string(self):
        """Will return the 'mean', 'error margin', and 'error percent' in a readable format"""
        mean = "{0:.2f}".format(self.mean)
        error = "{0:.2f}".format(self.error_margin)
        percent = "{0:.2%}".format(self.error_percent)
        return "Results: {0} ± {1} (± {2}) - [{3} Runs]\n".format(mean, error, percent, len(self.measures))