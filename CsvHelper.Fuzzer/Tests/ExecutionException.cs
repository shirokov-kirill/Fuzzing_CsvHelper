namespace CsvHelper.Fuzzer.Tests;

public class ExecutionException(string message): Exception
{
	public override string Message => message;
}
