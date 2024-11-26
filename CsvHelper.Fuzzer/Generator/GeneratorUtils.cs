using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CsvHelper.Fuzzer.Generator;

public static class GeneratorUtils
{
	public static string GetPathToStorage()
	{
		var pathStringBuilder = new StringBuilder();
		var storageDirectoryPath =
			Environment.CurrentDirectory.Split("\\")
				.TakeWhile(it => !it.StartsWith("bin"))
				.ToList();
		for (int i = 0; i < storageDirectoryPath.Count; i++)
		{
			pathStringBuilder.Append(storageDirectoryPath[i]);
			pathStringBuilder.Append("\\");
		}
		pathStringBuilder.Append("data");

		return pathStringBuilder.ToString();
	}
	public static string GetRandomString(Random random, [NotNull] int length)
	{
		var (chFrom, chTo) = GeneratorSettings.allowedAsciiCharacters;
		var sb = new StringBuilder();
		for (int i = 0; i < length; i++)
		{
			var index = random.Next(chTo);
			sb.Append((char)(chFrom + index));
		}

		return sb.ToString();
	}
}
