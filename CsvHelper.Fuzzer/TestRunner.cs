using System.Globalization;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Generator.context;
using CsvHelper.Fuzzer.Tests;
using Exception = System.Exception;

namespace CsvHelper.Fuzzer;

class TestRunner
{
	static void Main(string[] args)
	{
		var random = new Random();
		using var stream = new MemoryStream();
		using var writer = new StreamWriter(stream);
		using var reader = new StreamReader(stream);
		using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
		using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

		var generator = new SpecificationBasedGenerator(stream, writer, random);

		var randomFuzzer = new CsvFuzzer(random, generator, stream, csvReader, csvWriter);
		randomFuzzer.Fuzz(writer, reader);
	}
}
