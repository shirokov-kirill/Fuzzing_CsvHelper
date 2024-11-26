using System.Linq.Expressions;

namespace CsvHelper.ExecutionLogger;

/// <summary>
/// Can be used in functions to log on branching and in code.
/// Used then to determine coverage in fuzzer.
/// </summary>
internal class CsvFuzzingLogger: IFuzzerLogsProvider
{
	/// <summary>
	/// Logger only instance
	/// </summary>
	public static CsvFuzzingLogger Instance { get; } = new();

	private List<string> myStorage = new();

	private CsvFuzzingLogger() { }


	public void Clear()
	{
		myStorage.Clear();
	}

	public List<string> GetLogs()
	{
		return myStorage;
	}

	public void LogString(string name, int lineNumber) {
		myStorage.Add($"{name}: {lineNumber}");
	}
}

public class BinOp(string id)
{
	/// <summary>
	/// Equality operator
	/// </summary>
	public static BinOp Eq = new("Eq");

	/// <summary>
	/// Greater-than operator
	/// </summary>
	public static BinOp Gt = new("Gt");

	/// <summary>
	/// Greater-or-equal operator
	/// </summary>
	public static BinOp GoE = new("GoE");
}
