from .temperature import Temperature_Summary
from pyRAPL import Result


class CsvLine():
    """
    A class representing a single line in a CSV file
    """

    def __init__(self, result: Result, temp: Temperature_Summary):
        self.label = result.label
        self.timestamp = result.timestamp
        self.duration = result.duration
        self.pkg = result.pkg[0]
        self.dram = result.dram[0]
        self.temp = temp

    def print(self):
        """
        Takes all of the values in a single CSV line and joins them with ';' 
        """
        temp_before = self.temp.before if self.temp else None
        temp_after = self.temp.after if self.temp else None
        return "{0};{1};{2};{3};{4};{5};{6}".format(self.label, self.timestamp, self.duration, self.pkg, self.dram, temp_before, temp_after)


class CSV_Output():
    """
    Our version of the CSV output file functionality from the pyRAPL library
    """
    def __print_header__(self):
        with open(self.filepath, "w+") as csvfile:
            csvfile.write("label;timestamp;duration;pkg;ram;temp before;temp after\n")

    def __init__(self, filepath: str):
        self.filepath = filepath
        self.measurements = []
        self.__print_header__()

    def add(self, result: Result, temp_result: Temperature_Summary = None):
        measure = CsvLine(result, temp_result)
        self.measurements.append(measure)

    def save(self):
        """
        Prints all of the stored measurements into a single CSV file
        """
        with open(self.filepath, "a+") as csvfile:
            for measure in self.measurements:
                csvfile.write("{0}\n".format(measure.print()))
            self.measurements = []
