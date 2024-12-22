namespace CsvHelper.Fuzzer;

class TestRunner
{
	static void Main(string[] args)
	{
		var random = new Random();

		var randomFuzzer = new CsvFuzzer(random);
		randomFuzzer.Fuzz();
	}
}
