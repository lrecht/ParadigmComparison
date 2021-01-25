class Result():
    def __init__(self, label: str, result_line: str):
        self.label = label
        
        duration, pkg, dram, temp = result_line.split(';')
        self.duration = float(duration)
        self.pkg = float(pkg)
        self.dram = float(dram)
        self.temp = float(temp)
