namespace CsvHelper.Fuzzer;

public class TestScenarioResultEvaluator
{
	private static readonly List<Func<ScenarioResult, bool>> TestScenarioResultEvaluators =
		[Scenario1Evaluator];

	public static Func<ScenarioResult, bool> GetEvaluator(int scenarioKey)
	{
		if(TestScenarioResultEvaluators.Count > scenarioKey)
			return TestScenarioResultEvaluators[scenarioKey];
		throw new ArgumentException($"Scenario key {scenarioKey} is out of range.");
	}

	private static bool Scenario1Evaluator(ScenarioResult scenarioResult)
	{
		var expectedResult = scenarioResult.ExpectedValue as List<IDictionary<string, object>>;
		var actualResult = scenarioResult.ActualValue as List<object>;

		var isSuccess = expectedResult?.Count == actualResult?.Count;
		if (!isSuccess)
			return false;
		for (int i = 0; i < actualResult?.Count; i++)
		{
			var value = actualResult[i] as IDictionary<string, object>;
			isSuccess &= value?.SequenceEqual(expectedResult[i]) ?? false;
		}
		return isSuccess;
	}
}
