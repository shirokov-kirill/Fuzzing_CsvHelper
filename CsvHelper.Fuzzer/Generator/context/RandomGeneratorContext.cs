namespace CsvHelper.Fuzzer.Generator.context;

public class RandomGeneratorContext(string path): IFuzzGeneratorContext
{
	private readonly List<string> myLines = new List<string>();

	public string ToCsv()
	{
		using StreamWriter file = new StreamWriter(path);
		foreach (var line in myLines)
		{
			file.WriteLine(line);
		}
		return path;
	}

	public object GetExpectedResult()
	{
		return myLines.Count == 0 ? new List<IDictionary<string, object>>() : false;
	}

	public void AddLine(string line)
	{
		myLines.Add(line);
	}
}
