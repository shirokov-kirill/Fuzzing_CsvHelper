namespace CsvHelper.Fuzzer.Generator.context;

public interface IFuzzGeneratorContext
{
	/// <summary>
	/// Writes all containing data to .csv file
	/// </summary>
	/// <returns>Path which contains .csv file with result</returns>
	public void ToCsv();

	/// <summary>
	/// Gets the result to expect
	/// </summary>
	/// <returns>
	/// false - if not a correct csv file<br/>
	/// list of entries - if csv file should be parsed
	/// </returns>
	public object GetExpectedResult();
	public string GetInput();
}
