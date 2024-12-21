using System.Diagnostics.CodeAnalysis;
using System.Text;
using CsvHelper.Fuzzer.Generator.context;
using CsvHelper.Fuzzer.Generator.specification;

namespace CsvHelper.Fuzzer.Generator;

public class SpecificationBasedGenerator(Random random): InputGeneratorBase
{
	private readonly CsvSpecificationOwner myCsvSpecificationOwner = new CsvSpecificationOwner(SpecificationType.RFC_4180);

	public override IFuzzGeneratorContext Generate()
	{
		// create storage folder if none exists
		var filePath = CreateEmptyFile();
		var context = new CsvGeneratorContext(filePath, myCsvSpecificationOwner);

		var recordsCount = random.Next(MaxLinesCount);
		var fieldsCount = random.Next(1, 300);

		// Generate header
		// Some sources on the internet state both with and without header csv are OK, while others state that header is a mandatory part.
		for (int i = 0; i < fieldsCount; i++)
		{
			context.AddFieldName(GenerateFieldName());
		}

		// Generate rows
		for (int i = 0; i < recordsCount; i++)
		{
			var fields = new List<string>();
			for (int j = 0; j < fieldsCount; j++)
			{
				fields.Add(GenerateField());
			}
			context.AddRecord(fields);
		}

		return context;
	}

	private string GenerateFieldName()
	{
		var length = random.Next(1, 30);
		return GeneratorUtils.GetRandomString(random, length);
	}

	private string GenerateField()
	{
		var length = random.Next(1, 100);
		return GeneratorUtils.GetRandomString(random, length);
	}
}
