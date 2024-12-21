using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer.Generator;

public interface IInputGenerator
{
	public IFuzzGeneratorContext Generate();
}
