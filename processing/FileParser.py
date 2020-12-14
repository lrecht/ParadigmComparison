from abc import ABC, abstractmethod
from typing import Sequence
from Result import Observation, Result

class FileParser(ABC):
    @abstractmethod
    def parse_line(self, line: str) -> Observation:
        raise NotImplementedError()


    @abstractmethod
    def get_lines(self, file_path: str) -> Sequence[str]:
        raise NotImplementedError()


    def parse(self, file: str) -> Result:
        res = Result()
        lines = self.get_lines(file)
        for line in lines:
            temp_res = self.parse_line(line)
            res.add(temp_res)
        return res
