TEMP_REG_FILE = '/sys/class/thermal/thermal_zone10/temp'

def get_temp_celsius() -> float:
    with open(TEMP_REG_FILE, 'r') as temperature:
        return int(temperature.readline()) / 1000

class Temperature_Summary():
    def __init__(self, t1, t2):
        self.before = t1
        self.after = t2

    def get_average(self) -> float:
        return (self.before + self.after) / 2
