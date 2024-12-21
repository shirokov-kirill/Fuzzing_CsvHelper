namespace CsvHelper.Fuzzer.Generator.specification;

public interface ISpecificationOwner
{
	public string FormatHeader(List<string> formattedFieldNames);

	public string FormatRow(List<string> formattedFields);

	public string FormatField(string field);

	public string FormatFieldName(string fieldName);
}
