using System.Text;
using CsvHelper.Fuzzer.Generator.specification;

namespace CsvHelper.Fuzzer.Generator.context;

/// <summary>
/// Main class to perform mutations on generated data.<br />
/// Use <inheritdoc cref="ToCsv"/> method to write all data into the .csv file and finalize production.
/// </summary>
public class CsvGeneratorContext(
	StreamWriter writer,
	MemoryStream stream,
	CsvSpecificationOwner formatter): IFuzzGeneratorContext
{
	private readonly CsvSpecificationOwner myFormatter = formatter;

	private bool isCompiled = false;
	private readonly List<string> myFieldNames = new List<string>();
	private readonly List<List<string>> myRecords = new List<List<string>>();

	private List<string> MyLines = new List<string>();

	public void ToCsv()
	{
		if (!isCompiled)
			Compile();
		foreach (var line in MyLines)
		{
			writer.WriteLine(line);
		}
		writer.Flush();
		stream.Position = 0;
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

	public string GetInput()
	{
		if(!isCompiled)
			Compile();
		var sb = new StringBuilder();
		foreach (var line in MyLines)
		{
			sb.AppendLine(line);
		}
		return sb.ToString();
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

	private void Compile()
	{
		isCompiled = true;

		// Generate header
		// Some sources on the internet state both with and without header csv are OK, while others state that header is a mandatory part.
		var fieldNames  = new List<string>();
		for (int i = 0; i < myFieldNames.Count; i++)
		{
			fieldNames.Add(myFormatter.FormatFieldName(myFieldNames[i]));
		}
		var headerLine = myFormatter.FormatHeader(fieldNames);
		MyLines.Add(headerLine);

		// Generate rows
		for (int i = 0; i < myRecords.Count; i++)
		{
			var fields = new List<string>();
			for (int j = 0; j < myRecords[i].Count; j++)
			{
				fields.Add(myFormatter.FormatField(myRecords[i][j]));
			}
			MyLines.Add(myFormatter.FormatRow(fields));
		}
	}
}
