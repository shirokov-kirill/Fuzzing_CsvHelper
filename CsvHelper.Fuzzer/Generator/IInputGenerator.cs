namespace CsvHelper.Fuzzer.Generator;

public interface IInputGenerator
{
	public (string, object) Generate();
}
