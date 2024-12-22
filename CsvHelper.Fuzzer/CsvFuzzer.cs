using System.Globalization;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Tracing;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Fuzzer;

public class CsvFuzzer(Random random)
{
	public void Fuzz()
	{
		var traceCollector = new TraceCollector(FuzzingLogsCollector.Instance);
		var errorsLogger = new ErrorsLogger();

		while (traceCollector.ShouldRepeat())
		{
			using var stream = new MemoryStream();
			using var writer = new StreamWriter(stream);
			using var reader = new StreamReader(stream);
			using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
			using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
			{
				var generator = new SpecificationBasedGenerator(stream, writer, random);

				traceCollector.Next();
				var scenario = TestScenarios.GetRandomScenario(random);
				try
				{
					var (scenarioResult, input) = scenario.Func.Invoke(csvWriter, csvReader, generator);
					var resultEvaluator = TestScenarioResultEvaluator.GetEvaluator(scenario.ScenarioKey);
					var isFail = resultEvaluator.Invoke(scenarioResult);
					if (isFail)
					{
						errorsLogger.LogReadError(scenario.ScenarioKey, input);
					}
				}
				catch (Exception e) { }
				traceCollector.Commit();
				Console.WriteLine(traceCollector.GetStatistics());
			}
		}
		Console.WriteLine(traceCollector.GetStatistics());
	}
}
