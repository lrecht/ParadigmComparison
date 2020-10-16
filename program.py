import os
import fnmatch

class Program:
    def __init__(self, path, paradigm):
        self.path = path
        self.paradigm = paradigm

    def get_build_command(self):
        raise NotImplementedError("Please Implement this method")

    def get_run_command(self):
        raise NotImplementedError("Please Implement this method")


class Dotnet_Program(Program):
    def get_build_command(self):
        return "dotnet build --configuration Release --nologo --verbosity quiet " + self.path


class C_Sharp_Program(Dotnet_Program):
    def get_run_command(self):
        command = self.path + '/bin/Release/netcoreapp3.1/'
        return command + self.paradigm + "_c#"


class F_Sharp_Program(Dotnet_Program):
    def get_run_command(self):
        command = self.path + '/bin/Release/netcoreapp3.1/'
        return command + self.paradigm + "_f#"


#Discovers f# project folders based on naming convention (functional_f# or oop_f#)
def discover_fsharp_program(path):
    results = []

    for name in os.listdir(path):
        if not os.path.isdir(path + '/' + name):
            continue

        program_path = path + '/' + name

        if fnmatch.fnmatch(name, "*_f#"):
            results.append(F_Sharp_Program(program_path, name.split('_')[0]))

    return results

#Discovers c# project folders based on naming convention (functional_c# or oop_c#)
def discover_csharp_program(path):
    results = []

    for name in os.listdir(path):
        if not os.path.isdir(path + '/' + name):
            continue

        program_path = path + '/' + name

        if fnmatch.fnmatch(name, "*_c#"):
            results.append(C_Sharp_Program(program_path, name.split('_')[0]))

    return results
