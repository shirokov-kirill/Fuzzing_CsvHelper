using System.Diagnostics;
using System.Globalization;
using CsvHelper.Configuration;
using Xunit;

namespace CsvHelper.Fuzzing;

public class MainFuzzingTask
{
	[Fact]
	public void FuzzingTask()
	{
		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = false,
		};
		using (var reader = new StreamReader("path\\to\\file.csv"))
		using (var csv = new CsvReader(reader, config))
		{

			var records = csv.GetRecords<Foo>();
		}

		using (var writer = new StreamWriter("path\\to\\file.csv"))
		using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
		{
			csv.WriteHeader<Foo>();
			csv.NextRecord();
			foreach (var record in records)
			{
				csv.WriteRecord(record);
				csv.NextRecord();
			}
		}
	}
}
