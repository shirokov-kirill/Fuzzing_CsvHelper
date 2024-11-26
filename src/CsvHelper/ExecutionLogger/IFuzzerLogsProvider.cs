namespace CsvHelper.ExecutionLogger;

public interface IFuzzerLogsProvider
{
	public void Clear();

	public List<string> GetLogs();
}
