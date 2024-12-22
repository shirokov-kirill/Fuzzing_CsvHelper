using System.Text;
using CsvHelper.Fuzzer.Generator;
using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer;

public class ErrorsLogger
{
	private StreamWriter errorsStreamWriter = new (CreateLogFile());
	private HashSet<int> myScenariosWithErrors = new HashSet<int>();

	private static string CreateLogFile()
	{
		string storagePath =GeneratorUtils.GetPathToStorage();
		bool exists = System.IO.Directory.Exists(storagePath);
		if(!exists)
			System.IO.Directory.CreateDirectory(storagePath);

		var filePath = storagePath + @"\errors.txt";
		File.Create(filePath).Close();
		return filePath;
	}

	public void LogReadError(int scenarioKey, string input)
	{
		if (!myScenariosWithErrors.Contains(scenarioKey))
		{
			myScenariosWithErrors.Add(scenarioKey);
			var sb = new StringBuilder();
			sb.AppendLine($"\nScenario Key: {scenarioKey}. Error in output.");
			sb.AppendLine($"Input: \n{input}\n\n");
			errorsStreamWriter.WriteLine(sb.ToString());
		}
	}
}
