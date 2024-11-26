using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace CsvHelper.Fuzzer.Generator;

public class RandomGenerator(Random random): IInputGenerator
{
	public string Generate()
	{
		var linesCount = random.Next(500);
		var lines = new string[linesCount];
		for (int i = 0; i < linesCount; i++)
		{
			var stringLength = random.Next(10000);
			var line = GeneratorUtils.GetRandomString(random, stringLength);
			lines[i] = line;
		}

		// create storage folder if none exists
		string storagePath =GeneratorUtils.GetPathToStorage();
		bool exists = System.IO.Directory.Exists(storagePath);
		if(!exists)
			System.IO.Directory.CreateDirectory(storagePath);

		var filePath = storagePath + @"\input.csv";
		File.Create(filePath).Close();
		using StreamWriter file = new StreamWriter(filePath);
		foreach (var line in lines)
		{
			file.WriteLine(line);
		}

		return filePath;
	}
}
