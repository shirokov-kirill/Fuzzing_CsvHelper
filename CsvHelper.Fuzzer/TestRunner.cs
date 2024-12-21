using System.Globalization;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Tests;
using Exception = System.Exception;

namespace CsvHelper.Fuzzer;

class TestRunner
{

	private static IEnumerable<T> Run<T>(CsvReader csvReader)
	{
		var records = csvReader.GetRecords<T>();
		return records.ToList();
	}

	private static ExecutionResult<T> Execute<T>(string path, object expectedResult)
	{
		using var reader = new StreamReader(path);
		using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

		if (expectedResult is false)
		{
			try
			{
				var result = Run<T>(csv);
				return ExecutionResult<T>.Failed.WithPayload(result);
			}
			catch (Exception e)
			{
				return ExecutionResult<T>.Success.WithException(e);
			}
		}

		try
		{
			var expectedListResult = expectedResult as List<T>;
			var records = Run<T>(csv).ToList();
			if (expectedListResult?.SequenceEqual(records) ?? false)
			{
				return ExecutionResult<T>.Success.WithPayload(records);
			}
			else
			{
				return ExecutionResult<T>.Failed.WithPayload(records);
			}
		}
		catch (Exception e)
		{
			return ExecutionResult<T>.Failed.WithException(e);
		}
	}

	static void Main(string[] args)
	{
		var randomFuzzer = new CsvRandomFuzzer<string>(new RandomGenerator(new Random()), Execute<string>);
		randomFuzzer.Fuzz();
	}
}
