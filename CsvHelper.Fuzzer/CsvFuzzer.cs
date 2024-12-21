using System.Text;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Tests;

namespace CsvHelper.Fuzzer;

public class CsvFuzzer<T>(IInputGenerator generator, Func<string, object, ExecutionResult<T>> target)
{
	public void Fuzz()
	{
		var counter = 0;
		var maxCount = 1000;
		while (counter < maxCount)
		{
			var (path, expectedResult) = generator.Generate();
			var result = target(path, expectedResult);
			if (result == ExecutionResult<T>.Failed)
			{
				Console.WriteLine($"Run number {counter}");
				if (result.Payload != null)
				{
					Console.WriteLine($"Failed, actual result: [");
					foreach (var record in result.Payload)
					{
						var dynamicRecord = record as IDictionary<string, object?>;
						var sb = new StringBuilder();
						sb.Append("{ ");
						foreach (var key in dynamicRecord.Keys)
						{
							sb.Append($"{key}: {dynamicRecord[key]}, ");
						}
						sb.Append(" }");
						Console.WriteLine($"{sb}");
					}
					Console.WriteLine($"]");
				}

				if (result.Exception != null)
					Console.WriteLine($"Failed, exception {result.Exception}");
			}
			counter++;
		}
	}
}
