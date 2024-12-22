using System.Text;

namespace CsvHelper.Fuzzer.Generator.context;

public class RandomGeneratorContext(MemoryStream stream): IFuzzGeneratorContext
{
	private readonly List<string> myLines = new List<string>();

	public void ToCsv()
	{
		using StreamWriter file = new StreamWriter(stream);
		foreach (var line in myLines)
		{
			file.WriteLine(line);
		}
	}

	public object GetExpectedResult()
	{
		return myLines.Count == 0 ? new List<IDictionary<string, object>>() : false;
	}

	public string GetInput()
	{
		var sb = new StringBuilder();
		foreach (var line in myLines)
		{
			sb.AppendLine(line);
		}
		return sb.ToString();
	}

	public void AddLine(string line)
	{
		myLines.Add(line);
	}
}
