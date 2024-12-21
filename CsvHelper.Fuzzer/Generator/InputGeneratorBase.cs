using CsvHelper.Fuzzer.Generator.context;

namespace CsvHelper.Fuzzer.Generator;

public class InputGeneratorBase: IInputGenerator
{
	protected int MaxLinesCount = 2000;

	protected string CreateEmptyFile()
	{
		// create storage folder if none exists
		string storagePath =GeneratorUtils.GetPathToStorage();
		bool exists = System.IO.Directory.Exists(storagePath);
		if(!exists)
			System.IO.Directory.CreateDirectory(storagePath);

		var filePath = storagePath + @"\input.csv";
		File.Create(filePath).Close();
		return filePath;
	}

	public virtual IFuzzGeneratorContext Generate()
	{
		var filePath = CreateEmptyFile();
		return new RandomGeneratorContext(filePath);
	}
}
