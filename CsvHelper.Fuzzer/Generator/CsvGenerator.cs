using System.Diagnostics.CodeAnalysis;
using System.Text;
using CsvHelper.Fuzzer.Generator.context;
using CsvHelper.Fuzzer.Generator.specification;

namespace CsvHelper.Fuzzer.Generator;

public class SpecificationBasedGenerator(MemoryStream stream, StreamWriter writer, Random random): InputGeneratorBase(random)
{
	protected override MemoryStream Stream => stream;
	private readonly CsvSpecificationOwner myCsvSpecificationOwner = new CsvSpecificationOwner(SpecificationType.RFC_4180);

	public override IFuzzGeneratorContext Generate()
	{
		var context = new CsvGeneratorContext(writer, Stream, myCsvSpecificationOwner);

		var recordsCount = random.Next(MaxLinesCount);
		var fieldsCount = random.Next(1, 30);

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
		var length = random.Next(1, 20);
		return GeneratorUtils.GetRandomString(random, length);
	}

	private string GenerateField()
	{
		var length = random.Next(0, 50);
		return GeneratorUtils.GetRandomString(random, length);
	}

	public override IFuzzGeneratorContext Generate(Func<IEnumerable<string>> generateRawHeader, Func<IEnumerable<string>> generateRecord, int numberOfRecords)
	{
		var context = new CsvGeneratorContext(writer, Stream, myCsvSpecificationOwner);
		foreach (var name in generateRawHeader())
		{
			context.AddFieldName(name);
		}

		for (int i = 0; i < numberOfRecords; i++)
		{
			context.AddRecord(generateRecord().ToList());
		}

		return context;
	}
}
