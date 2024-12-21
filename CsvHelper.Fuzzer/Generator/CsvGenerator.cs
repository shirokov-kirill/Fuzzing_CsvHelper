using System.Diagnostics.CodeAnalysis;
using System.Text;
using CsvHelper.Fuzzer.Generator.specification;

namespace CsvHelper.Fuzzer.Generator;

public class SpecificationBasedGenerator(Random random): InputGeneratorBase
{
	private readonly CsvSpecificationOwner myCsvSpecificationOwner = new CsvSpecificationOwner(SpecificationType.RFC_4180);

	public override (string, object) Generate()
	{
		var recordsCount = random.Next(MaxLinesCount);
		var fieldsCount = random.Next(300);
		var lines = new string[recordsCount + 1];

		// Generate header
		var fieldNames  = new List<string>();
		for (int i = 0; i < fieldsCount; i++)
		{
			fieldNames.Add(myCsvSpecificationOwner.FormatFieldName(GenerateFieldName()));
		}
		var headerLine = myCsvSpecificationOwner.FormatHeader(fieldNames);
		lines[0] = headerLine;

		// Generate rows
		var records  = new List<List<string>>();
		for (int i = 0; i < recordsCount; i++)
		{
			var fields = new List<string>();
			for (int j = 0; j < fieldsCount; j++)
			{
				fields.Add(myCsvSpecificationOwner.FormatField(GenerateField()));
			}
			records.Add(fields);
			lines[i + 1] = myCsvSpecificationOwner.FormatRow(fields);
		}

		// create storage folder if none exists
		var filePath = CreateEmptyFile();
		using StreamWriter file = new StreamWriter(filePath);
		foreach (var line in lines)
		{
			file.WriteLine(line);
		}

		// TODO improve with check on correct .csv
		var isEmpty = lines.Length == 0;
		return (filePath, records);
	}

	private string GenerateFieldName()
	{
		var length = random.Next(30);
		return GeneratorUtils.GetRandomString(random, length);
	}

	private string GenerateField()
	{
		var length = random.Next(100);
		return GeneratorUtils.GetRandomString(random, length);
	}
}
