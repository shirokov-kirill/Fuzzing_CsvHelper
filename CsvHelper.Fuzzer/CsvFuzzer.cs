using System.Text;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Generator.context;
using CsvHelper.Fuzzer.Tests;
using CsvHelper.Fuzzer.Tracing;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Fuzzer;

public class CsvFuzzer(Random random, IInputGenerator generator, MemoryStream stream, CsvReader csvReader, CsvWriter csvWriter)
{
	public void Fuzz(StreamWriter streamWriter, StreamReader streamReader)
	{
		var globalCounter = 0;
		var traceCollector = new TraceCollector(FuzzingLogsCollector.Instance);

		while (traceCollector.ShouldRepeat())
		{
			stream.Flush();
			stream.Position = 0;
			traceCollector.Next();
			var scenario = TestScenarios.GetRandomScenario(random);
			var scenarioResult = scenario.Func.Invoke(csvWriter, csvReader, generator);
			var resultEvaluator = TestScenarioResultEvaluator.GetEvaluator(scenario.ScenarioKey);
			var isFail = resultEvaluator.Invoke(scenarioResult);
			if (isFail)
			{
				Console.WriteLine($"Run number {globalCounter} failed.");
				// todo log it
			}
			traceCollector.Commit();
			globalCounter++;
			Console.WriteLine(traceCollector.GetStatistics());
		}
		Console.WriteLine(traceCollector.GetStatistics());
	}
}
