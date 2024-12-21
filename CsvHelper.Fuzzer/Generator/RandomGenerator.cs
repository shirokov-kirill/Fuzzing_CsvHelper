using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace CsvHelper.Fuzzer.Generator;

public class RandomGenerator(Random random): InputGeneratorBase
{
	public override (string, object) Generate()
	{
		var linesCount = random.Next(500);
		var lines = new string[linesCount];
		for (int i = 0; i < linesCount; i++)
		{
			var stringLength = random.Next(10000);
			var line = GeneratorUtils.GetRandomString(random, stringLength);
			lines[i] = line;
		}

		var filePath = CreateEmptyFile();
		using StreamWriter file = new StreamWriter(filePath);
		foreach (var line in lines)
		{
			file.WriteLine(line);
		}

		// TODO improve with check on correct .csv
		var isEmpty = lines.Length == 0;
		return (filePath, isEmpty ? new List<string>() : false);
	}
}
