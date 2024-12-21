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
			var expectedListResult = expectedResult as List<IDictionary<string, object>?>;
			var records = Run<T>(csv).ToList();
			var isEqual = records.Count == (expectedListResult?.Count ?? 0);
			for (int i = 0; i < records.Count; i++)
			{
				var expResult = expectedListResult?.ElementAtOrDefault(i);
				var actResult = records[i] as IDictionary<string, object>;
				if((expResult == null && actResult == null) || (actResult != null && (expResult?.SequenceEqual(actResult) ?? false)))
					continue;
				isEqual = false;
			}
			if (isEqual)
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
		var randomFuzzer = new CsvFuzzer<dynamic>(new SpecificationBasedGenerator(new Random()), Execute<dynamic>);
		randomFuzzer.Fuzz();
	}
}
