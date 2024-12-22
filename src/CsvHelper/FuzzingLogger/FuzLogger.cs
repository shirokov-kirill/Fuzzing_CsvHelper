namespace CsvHelper.FuzzingLogger;

/// <summary>
/// Logs the trace of the executed program. Use <inheritdoc cref="Log"/> to commit.
/// </summary>
public class FuzzingLogsCollector
{
	private FuzzingLogsCollector()
	{

	}

	public static readonly FuzzingLogsCollector Instance = new();
	private ISet<string> myTrace = new HashSet<string>();

	public static void Log(string fileName, string function, int lineNumber)
	{
		Instance.LogInner(fileName, function, lineNumber);
	}

	private void LogInner(string fileName, string function, int lineNumber)
	{
		myTrace.Add($"file:{fileName}, func:{function}, line:{lineNumber}");
	}

	public ISet<string> GetTrace()
	{
		return myTrace;
	}

	public void Reset()
	{
		myTrace = new HashSet<string>();
	}
}
