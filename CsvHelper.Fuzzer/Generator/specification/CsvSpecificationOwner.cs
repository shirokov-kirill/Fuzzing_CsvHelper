using System.Text;

namespace CsvHelper.Fuzzer.Generator.specification;

public class CsvSpecificationOwner(SpecificationType specificationType)
{
	private SpecificationType mySpecificationType = specificationType;

	public string FormatHeader(List<string> formattedFieldNames)
	{
		var sb = new StringBuilder();
		if (formattedFieldNames.Count == 0)
		{
			return sb.ToString();
		}
		if (mySpecificationType == SpecificationType.RFC_4180)
		{
			sb.Append(formattedFieldNames[0]);
			for (int i = 1; i < formattedFieldNames.Count; i++)
			{
				sb.Append(',');
				sb.Append(formattedFieldNames[i]);
			}

			sb.Append('\n');
		}

		return sb.ToString();
	}

	public string FormatRow(List<string> formattedFields)
	{
		var sb = new StringBuilder();
		if (formattedFields.Count == 0)
		{
			return sb.ToString();
		}
		if (mySpecificationType == SpecificationType.RFC_4180)
		{
			sb.Append(formattedFields[0]);
			for (int i = 1; i < formattedFields.Count; i++)
			{
				sb.Append(',');
				sb.Append(formattedFields[i]);
			}

			sb.Append('\n');
		}

		return sb.ToString();
	}

	public string FormatField(string field)
	{
		var sb = new StringBuilder();
		if (mySpecificationType == SpecificationType.RFC_4180)
		{
			var shouldEncloseInQuotes = field.IndexOf('\"') != -1
			                            || field.IndexOf('\n') != -1
			                            || field.IndexOf(',') != -1;
			if (shouldEncloseInQuotes)
				sb.Append("\"");
			for (int i = 0; i < field.Length; i++)
			{
				if (field[i] == '\"')
					sb.Append("\"");
				sb.Append(field[i]);
			}
			if (shouldEncloseInQuotes)
				sb.Append("\"");
		}

		return sb.ToString();
	}

	public string FormatFieldName(string fieldName)
	{
		var sb = new StringBuilder();
		if (mySpecificationType == SpecificationType.RFC_4180)
		{
			var shouldEncloseInQuotes = fieldName.IndexOf('\"') != -1
			                            || fieldName.IndexOf('\n') != -1
			                            || fieldName.IndexOf(',') != -1;
			if (shouldEncloseInQuotes)
				sb.Append("\"");
			for (int i = 0; i < fieldName.Length; i++)
			{
				if (fieldName[i] == '\"')
					sb.Append("\"");
				sb.Append(fieldName[i]);
			}
			if (shouldEncloseInQuotes)
				sb.Append("\"");
		}

		return sb.ToString();
	}
}

public class SpecificationType(string id)
{
	public static SpecificationType RFC_4180 = new SpecificationType("RFC_4180");
}
