import pyRAPL

#Setupsies
pyRAPL.setup()
csv_output = pyRAPL.outputs.CSVOutput('result.csv')
experimentIterations = 10


#The test case being run
def test():
    for i in range(1,10000):
        print("hej")


#The measuring equipment
for i in range(0,experimentIterations):
    meter = pyRAPL.Measurement(label="label")
    meter.begin()
    test()
    meter.end()
    csv_output.add(meter.result)


#the output
csv_output.save()
print(meter.result)
