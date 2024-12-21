namespace CsvHelper.Fuzzer.Tests;

public class ExecutionResult<T>
{
	private string myId;
	public Exception? Exception { get; private set; }
	public IEnumerable<T>? Payload { get; private set; }

	private ExecutionResult(string id)
	{
		myId = id;
	}

	public static ExecutionResult<T> Success = new ("Success");
	public static ExecutionResult<T> Failed = new ("Failed");

	/// <summary>
	/// Adds exception to the Failed member
	/// </summary>
	/// <param name="exception">exception to attach</param>
	public ExecutionResult<T> WithException(Exception exception)
	{
		Exception = exception;
		return this;
	}

	public ExecutionResult<T> WithPayload(IEnumerable<T> payload)
	{
		Payload = payload;
		return this;
	}
}
