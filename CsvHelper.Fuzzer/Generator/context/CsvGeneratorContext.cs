using System.Text;
using CsvHelper.Fuzzer.Generator.specification;

namespace CsvHelper.Fuzzer.Generator;

/// <summary>
/// Main class to perform mutations on generated data.<br />
/// Use <inheritdoc cref="ToCsv"/> method to write all data into the .csv file and finalize production.
/// </summary>
public class CsvGeneratorContext(string path, CsvSpecificationOwner formatter): IFuzzGeneratorContext
{
	private readonly CsvSpecificationOwner myFormatter = formatter;

	private bool isCompiled = false;
	private readonly List<string> myFieldNames = new List<string>();
	private readonly List<List<string>> myRecords = new List<List<string>>();

	private List<string> MyLines => Compile();

	public string ToCsv()
	{
		if (!isCompiled)
		{

			using StreamWriter file = new StreamWriter(path);
			foreach (var line in MyLines)
			{
				file.WriteLine(line);
			}
		}
		return path;
	}

	public object GetExpectedResult()
	{
		if (!isCompiled)
			return false;
		var resultsList = new List<IDictionary<string, object>>();
		foreach (var line in myRecords)
		{
			var dict = new Dictionary<string, object>();
			for (int i = 0; i < line.Count; i++)
			{
				dict[myFieldNames[i]] = line[i];
			}
			resultsList.Add(dict);
		}

		return resultsList;
	}

	public void AddFieldName(string generatedName)
	{
		if(!isCompiled)
			myFieldNames.Add(generatedName);
	}

	public void AddRecord(List<string> record)
	{
		if(!isCompiled)
			myRecords.Add(record);
	}

	private List<string> Compile()
	{
		isCompiled = true;
		var lines = new List<string>();

		// Generate header
		// Some sources on the internet state both with and without header csv are OK, while others state that header is a mandatory part.
		var fieldNames  = new List<string>();
		for (int i = 0; i < myFieldNames.Count; i++)
		{
			fieldNames.Add(myFormatter.FormatFieldName(myFieldNames[i]));
		}
		var headerLine = myFormatter.FormatHeader(fieldNames);
		lines.Add(headerLine);

		// Generate rows
		var records  = new List<List<string>>();
		for (int i = 0; i < myRecords.Count; i++)
		{
			var fields = new List<string>();
			for (int j = 0; j < myRecords[i].Count; j++)
			{
				fields.Add(myFormatter.FormatField(myRecords[i][j]));
			}
			lines.Add(myFormatter.FormatRow(fields));
		}

		return lines;
	}
}
