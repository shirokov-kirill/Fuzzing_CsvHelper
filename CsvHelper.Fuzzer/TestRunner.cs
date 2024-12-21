using System.Globalization;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Tests;

namespace CsvHelper.Fuzzer;

class TestRunner
{
	private static ExecutionResult Execute(string path)
	{
		try
		{
			using (var reader = new StreamReader(path))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				// TODO implement series of random calls
				var records = csv.GetRecords<string>();
				return ExecutionResult.Success.WithPayload(records);
			}
		}
		catch (Exception e)
		{
			return ExecutionResult.Failed.WithException(e);
		}
	}

	static void Main(string[] args)
	{
		var randomFuzzer = new CsvRandomFuzzer(new RandomGenerator(new Random(41)), Execute);
		randomFuzzer.Fuzz();
	}
}
