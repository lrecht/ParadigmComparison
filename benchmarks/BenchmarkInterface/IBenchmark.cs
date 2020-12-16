using System;

namespace BenchmarkInterface
{
    public interface IBenchmark
    {
        void Preprocess();
        int Run();
    }
}
