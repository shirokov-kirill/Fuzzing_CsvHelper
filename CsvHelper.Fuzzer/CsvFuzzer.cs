using System.Text;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Tests;
using CsvHelper.Fuzzer.Tracing;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Fuzzer;

public class CsvFuzzer<T>(IInputGenerator generator, Func<string, object, ExecutionResult<T>> target)
{
	public void Fuzz()
	{
		var globalCounter = 0;
		var traceCollector = new TraceCollector(FuzzingLogsCollector.Instance);
		while (traceCollector.ShouldRepeat())
		{
			traceCollector.Next();
			var context = generator.Generate();
			var path = context.ToCsv();
			var result = target(path, context.GetExpectedResult());
			if (result == ExecutionResult<T>.Failed)
			{
				Console.WriteLine($"Run number {globalCounter}");
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
			traceCollector.Commit();
			globalCounter++;
			if (globalCounter % 100 == 0)
			{
				Console.WriteLine(traceCollector.GetStatistics());
			}
		}
	}
}
