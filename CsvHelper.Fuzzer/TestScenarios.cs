using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer;

/// <summary>
/// List of possible read/write scenarios and usages. Scenarios may throw exceptions
/// </summary>
public class TestScenarios
{
	private static readonly List<Func<CsvWriter, CsvReader, IInputGenerator, ScenarioResult>> ScenarioFuncs =
		[Scenario1];

	public static Scenario GetRandomScenario(Random random)
	{
		var index = random.Next(0, ScenarioFuncs.Count);
		if(ScenarioFuncs.Count > index)
			return new Scenario(index, ScenarioFuncs[index]);
		throw new IndexOutOfRangeException("Internal error. There is no scenario with this index.");
	}

	private static ScenarioResult Scenario1(CsvWriter _, CsvReader reader, IInputGenerator generator)
	{
		var context = generator.Generate();
		context.ToCsv();
		try
		{
			var records = reader.GetRecords<dynamic>();
			var values = records.ToList();
			return new ScenarioResult(values, context.GetExpectedResult());
		}
		catch (Exception e)
		{
			return new ScenarioResult(e, context.GetExpectedResult());
		}
	}
}

public record Scenario(int ScenarioKey, Func<CsvWriter, CsvReader, IInputGenerator, ScenarioResult> Func);
public record ScenarioResult(object ActualValue, object ExpectedValue);
