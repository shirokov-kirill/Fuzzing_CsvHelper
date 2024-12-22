using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer.Generator;

public class RandomGenerator(Random random, MemoryStream stream): InputGeneratorBase
{
	protected override MemoryStream Stream => stream;

	public override IFuzzGeneratorContext Generate()
	{
		var context = new RandomGeneratorContext(Stream);

		var linesCount = random.Next(50);
		for (int i = 0; i < linesCount; i++)
		{
			var stringLength = random.Next(100);
			var line = GeneratorUtils.GetRandomString(random, stringLength);
			context.AddLine(line);
		}

		return context;
	}
}
