import re
import os
from typing import *
from Result import Observation
from FileParser import FileParser

rules = {
    'label': r'benchmarks/(.+?)/(.+?)_(f#|c#)/',
    'duration': r'.*',
    'pkg': r'.*',
    'ram': r'.*'
}


class CsvParser(FileParser):
    def __init__(self, sep: str = ';'):
        self.header: Sequence[str]
        self.separator = sep


    def apply_rules(self, part: str, index: int) -> List[str]:
        if self.header[index] in rules:
            rule = rules[self.header[index]]
            regex = re.compile(rule)
            match = regex.search(part)
            if match:
                if len(match.regs) == 1:
                    return [match.group()]
                else:
                    return list(match.groups())
        return []
        

    def split(self, line: str) -> Sequence[str]:
        return line.split(self.separator)


    def get_lines(self, file: str) -> Sequence[str]:
        if os.path.exists(file):
            lines = open(file, 'r').readlines()
            self.header = self.split(lines[0])
            return lines[1:]
        else:
            raise FileNotFoundError(f'The file {file} was not found')


    def parse_line(self, line: str) -> Observation:
        line_parts = self.split(line)
        obtained_info: List[str] = []
        for (i, part) in enumerate(line_parts):
            res = self.apply_rules(part, i)
            obtained_info = obtained_info + res
        return Observation(*obtained_info)