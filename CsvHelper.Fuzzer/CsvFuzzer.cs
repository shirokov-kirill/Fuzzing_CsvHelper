using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Tests;

namespace CsvHelper.Fuzzer;

public class CsvRandomFuzzer(IInputGenerator generator, Func<string, ExecutionResult> target)
{
	public void Fuzz()
	{
		var counter = 0;
		var maxCount = 1000;
		while (counter < maxCount)
		{
			var path = generator.Generate();
			var result = target(path);
			if(result == ExecutionResult.Failed)
				Console.Write($"Failed, exception {result.Exception}");
			counter++;
		}
	}
}
