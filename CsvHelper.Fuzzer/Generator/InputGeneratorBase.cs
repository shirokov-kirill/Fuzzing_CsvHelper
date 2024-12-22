using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer.Generator;

public abstract class InputGeneratorBase(Random random): IInputGenerator
{
	protected abstract MemoryStream Stream { get; }
	protected int MaxLinesCount = 2000;

	public virtual IFuzzGeneratorContext Generate()
	{
		return new RandomGeneratorContext(Stream);
	}

	public int RandomInteger()
	{
		return random.Next();
	}

	public string RandomString(int maxLength)
	{
		var length = random.Next(maxLength);
		return GeneratorUtils.GetRandomString(random, length);
	}

	public virtual IFuzzGeneratorContext Generate(Func<IEnumerable<string>> generateRawHeader, Func<IEnumerable<string>> generateRecord, int numberOfRecords)
	{
		var context = new RandomGeneratorContext(Stream);
		return context;
	}
}
