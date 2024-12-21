namespace CsvHelper.Fuzzer.Tests;

public class ExecutionResult
{
	private string myId;
	public Exception? Exception { get; private set; }
	public object? Payload { get; private set; }

	private ExecutionResult(string id)
	{
		myId = id;
	}

	public static ExecutionResult Success = new ("Success");
	public static ExecutionResult Failed = new ("Failed");

	/// <summary>
	/// Adds exception to the Failed member
	/// </summary>
	/// <param name="exception">exception to attach</param>
	public ExecutionResult WithException(Exception exception)
	{
		if(myId == "Failed")
			Exception = exception;
		return this;
	}

	public ExecutionResult WithPayload(object payload)
	{
		if (myId == "Success")
			Payload = payload;
		return this;
	}
}
