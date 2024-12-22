using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer;

/// <summary>
/// List of possible read/write scenarios and usages. Scenarios may throw exceptions
/// </summary>
public class TestScenarios
{
	private static readonly List<Func<CsvWriter, CsvReader, IInputGenerator, (ScenarioResult, string)>> ScenarioFuncs =
		[
			Scenario0,
			Scenario1,
			Scenario2,
		];

	public static Scenario GetRandomScenario(Random random)
	{
		var index = random.Next(0, ScenarioFuncs.Count);
		if(ScenarioFuncs.Count > index)
			return new Scenario(index, ScenarioFuncs[index]);
		throw new IndexOutOfRangeException("Internal error. There is no scenario with this index.");
	}

	private static (ScenarioResult, string) Scenario0(CsvWriter _, CsvReader reader, IInputGenerator generator)
	{
		var context = generator.Generate();
		context.ToCsv();
		try
		{
			var records = reader.GetRecords<dynamic>();
			var values = records.ToList();
			return (new ScenarioResult(values, context.GetExpectedResult()), context.GetInput());
		}
		catch (Exception e)
		{
			return (new ScenarioResult(false, context.GetExpectedResult()), context.GetInput());
		}
	}

	private static (ScenarioResult, string) Scenario1(CsvWriter _, CsvReader reader, IInputGenerator generator)
	{
		var definition = new
		{
			Reference = new Child()
		};

		var generatedData = new List<string>();
		var context = generator.Generate(() => [
				"ParentId",
				"ChildId",
				"ParentName",
				"ChildName"
			], () =>
		{
			var pId = generator.RandomInteger().ToString();
			var childId = generator.RandomInteger().ToString();
			var pVal = generator.RandomString(300);
			var chVal = generator.RandomString(300);
			generatedData.Add(pId);
			generatedData.Add(childId);
			generatedData.Add(pVal);
			generatedData.Add(chVal);
			return
			[
				pId,
				childId,
				pVal,
				chVal
			];
		}, 1);
		var input = context.GetInput();
		context.ToCsv();
		var result = reader.GetRecords(definition).ToList();
		var expectedValue = new {
			Reference = new Child()
			{
				ParentId = int.Parse(generatedData[0]),
				ChildId = int.Parse(generatedData[1]),
				ParentName = generatedData[2],
				ChildName = generatedData[3],
			}
		};
		return (new ScenarioResult(result, expectedValue), input);
	}

	private static (ScenarioResult, string) Scenario2(CsvWriter _, CsvReader reader, IInputGenerator generator)
	{
		var definition = new
		{
			A = 0,
			AnonymousReference = new
			{
				Reference = new Test()
			}
		};

		var generatedData = new List<string>();
		var context = generator.Generate(() => [
			"Id",
			"Name",
			"A",
		], () =>
		{
			var id = generator.RandomInteger().ToString();
			var name = generator.RandomString(300);
			var a = generator.RandomInteger().ToString();
			generatedData.Add(id);
			generatedData.Add(name);
			generatedData.Add(a);
			return
			[
				id,
				name,
				a,
			];
		}, 1);
		var input = context.GetInput();
		context.ToCsv();
		reader.Read();
		reader.ReadHeader();
		reader.Read();
		var result = reader.GetRecord(definition);
		var expectedValue = new {
			A = int.Parse(generatedData[2]),
			AnonymousReference = new Test()
			{
				Id = int.Parse(generatedData[0]),
				Name = generatedData[1],
			}
		};
		return (new ScenarioResult(result, expectedValue), input);
	}

	public class Test
	{
		public int Id { get; set; }

		public string? Name { get; set; }
	}

	public class Parent
	{
		public int ParentId { get; set; }

		public string? ParentName { get; set; }
	}

	public class Child : Parent
	{
		public int ChildId { get; set; }

		public string? ChildName { get; set; }
	}
}

public record Scenario(int ScenarioKey, Func<CsvWriter, CsvReader, IInputGenerator, (ScenarioResult, string)> Func);
public record ScenarioResult(object ActualValue, object ExpectedValue);
