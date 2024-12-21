namespace CsvHelper.Fuzzer.Generator;

public class RandomGenerator(Random random): InputGeneratorBase
{
	public override IFuzzGeneratorContext Generate()
	{
		var filePath = CreateEmptyFile();
		var context = new RandomGeneratorContext(filePath);

		var linesCount = random.Next(500);
		for (int i = 0; i < linesCount; i++)
		{
			var stringLength = random.Next(10000);
			var line = GeneratorUtils.GetRandomString(random, stringLength);
			context.AddLine(line);
		}

		return context;
	}
}
