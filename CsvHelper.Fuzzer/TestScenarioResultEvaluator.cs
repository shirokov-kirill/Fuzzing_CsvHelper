namespace CsvHelper.Fuzzer;

public class TestScenarioResultEvaluator
{
	private static readonly List<Func<ScenarioResult, bool>> TestScenarioResultEvaluators =
		[
			Scenario0Evaluator,
			Scenario1Evaluator,
			Scenario2Evaluator,
		];

	public static Func<ScenarioResult, bool> GetEvaluator(int scenarioKey)
	{
		if(TestScenarioResultEvaluators.Count > scenarioKey)
			return TestScenarioResultEvaluators[scenarioKey];
		throw new ArgumentException($"Scenario key {scenarioKey} is out of range.");
	}

	private static bool Scenario0Evaluator(ScenarioResult scenarioResult)
	{
		var expectedResult = scenarioResult.ExpectedValue as List<IDictionary<string, object>>;
		var actualResult = scenarioResult.ActualValue as List<object>;

		var isSuccess = true;
		for (int i = 0; i < actualResult?.Count; i++)
		{
			var value = actualResult[i] as IDictionary<string, object>;
			isSuccess &= value?.SequenceEqual(expectedResult[i]) ?? false;
			if(!isSuccess)
				break;
		}
		return isSuccess;
	}

	private static bool Scenario1Evaluator(ScenarioResult scenarioResult)
	{
		var actualResult = scenarioResult.ActualValue as List<object>;

		if (actualResult?.Count != 1)
			return false;

		return actualResult[0].Equals(scenarioResult.ExpectedValue);
	}

	private static bool Scenario2Evaluator(ScenarioResult scenarioResult)
	{
		return scenarioResult.ActualValue.Equals(scenarioResult.ExpectedValue);
	}
}
