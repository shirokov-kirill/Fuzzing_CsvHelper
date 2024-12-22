using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer.Generator;

public interface IInputGenerator
{
	public IFuzzGeneratorContext Generate();

	public int RandomInteger();
	public string RandomString(int maxLength);

	public IFuzzGeneratorContext Generate(Func<IEnumerable<string>> generateRawHeader, Func<IEnumerable<string>> generateRecord, int numberOfRecords);
}
