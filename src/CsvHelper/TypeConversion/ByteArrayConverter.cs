// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using System.Text;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts a <see cref="T:Byte[]"/> to and from a <see cref="string"/>.
/// </summary>
public class ByteArrayConverter : DefaultTypeConverter
{
	private readonly ByteArrayConverterOptions options;
	private readonly string HexStringPrefix;
	private readonly byte ByteLength;

	/// <summary>
	/// Creates a new ByteArrayConverter using the given <see cref="ByteArrayConverterOptions"/>.
	/// </summary>
	/// <param name="options">The options.</param>
	public ByteArrayConverter(ByteArrayConverterOptions options = ByteArrayConverterOptions.Hexadecimal | ByteArrayConverterOptions.HexInclude0x)
	{
		FuzzingLogsCollector.Log("ByteArrayConverter", "ByteArrayConverter", 26);
		// Defaults to the literal format used by C# for whole numbers, and SQL Server for binary data.
		this.options = options;
		ValidateOptions();

		HexStringPrefix = (options & ByteArrayConverterOptions.HexDashes) == ByteArrayConverterOptions.HexDashes ? "-" : string.Empty;
		ByteLength = (options & ByteArrayConverterOptions.HexDashes) == ByteArrayConverterOptions.HexDashes ? (byte)3 : (byte)2;
	}

	/// <summary>
	/// Converts the object to a string.
	/// </summary>
	/// <param name="value">The object to convert to a string.</param>
	/// <param name="row">The <see cref="IWriterRow"/> for the current record.</param>
	/// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being written.</param>
	/// <returns>The string representation of the object.</returns>
	public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("ByteArrayConverter", "ConvertToString", 44);
		if (value is byte[] byteArray)
		{
			FuzzingLogsCollector.Log("ByteArrayConverter", "ConvertToString", 47);
			return (options & ByteArrayConverterOptions.Base64) == ByteArrayConverterOptions.Base64
				? Convert.ToBase64String(byteArray)
				: ByteArrayToHexString(byteArray);
		}

		FuzzingLogsCollector.Log("ByteArrayConverter", "ConvertToString", 53);
		return base.ConvertToString(value, row, memberMapData);
	}

	/// <summary>
	/// Converts the string to an object.
	/// </summary>
	/// <param name="text">The string to convert to an object.</param>
	/// <param name="row">The <see cref="IReaderRow"/> for the current record.</param>
	/// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being created.</param>
	/// <returns>The object created from the string.</returns>
	public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("ByteArrayConverter", "ConvertFromString", 66);
		if (text != null)
		{
			FuzzingLogsCollector.Log("ByteArrayConverter", "ConvertToString", 69);
			return (options & ByteArrayConverterOptions.Base64) == ByteArrayConverterOptions.Base64
				? Convert.FromBase64String(text)
				: HexStringToByteArray(text);
		}

		FuzzingLogsCollector.Log("ByteArrayConverter", "ConvertToString", 75);
		return base.ConvertFromString(text, row, memberMapData);
	}

	private string ByteArrayToHexString(byte[] byteArray)
	{
		FuzzingLogsCollector.Log("ByteArrayConverter", "ByteArrayToHexString", 81);
		var hexString = new StringBuilder();

		if ((options & ByteArrayConverterOptions.HexInclude0x) == ByteArrayConverterOptions.HexInclude0x)
		{
			FuzzingLogsCollector.Log("ByteArrayConverter", "ByteArrayToHexString", 86);
			hexString.Append("0x");
		}

		if (byteArray.Length >= 1)
		{
			FuzzingLogsCollector.Log("ByteArrayConverter", "ByteArrayToHexString", 92);
			hexString.Append(byteArray[0].ToString("X2"));
		}

		for (var i = 1; i < byteArray.Length; i++)
		{
			FuzzingLogsCollector.Log("ByteArrayConverter", "ByteArrayToHexString", 98);
			hexString.Append(HexStringPrefix + byteArray[i].ToString("X2"));
		}

		FuzzingLogsCollector.Log("ByteArrayConverter", "ByteArrayToHexString", 102);
		return hexString.ToString();
	}

	private byte[] HexStringToByteArray(string hex)
	{
		FuzzingLogsCollector.Log("ByteArrayConverter", "HexStringToByteArray", 108);
		var has0x = hex.StartsWith("0x");

		var length = has0x
			? (hex.Length - 1) / ByteLength
			: hex.Length + 1 / ByteLength;
		var byteArray = new byte[length];
		var has0xOffset = has0x ? 1 : 0;

		for (var stringIndex = has0xOffset * 2; stringIndex < hex.Length; stringIndex += ByteLength)
		{
			FuzzingLogsCollector.Log("ByteArrayConverter", "HexStringToByteArray", 119);
			byteArray[(stringIndex - has0xOffset) / ByteLength] = Convert.ToByte(hex.Substring(stringIndex, 2), 16);
		}

		FuzzingLogsCollector.Log("ByteArrayConverter", "HexStringToByteArray", 123);
		return byteArray;
	}

	private void ValidateOptions()
	{
		FuzzingLogsCollector.Log("ByteArrayConverter", "ValidateOptions", 129);
		if ((options & ByteArrayConverterOptions.Base64) == ByteArrayConverterOptions.Base64)
		{
			FuzzingLogsCollector.Log("ByteArrayConverter", "ValidateOptions", 132);
			if ((options & (ByteArrayConverterOptions.HexInclude0x | ByteArrayConverterOptions.HexDashes | ByteArrayConverterOptions.Hexadecimal)) != ByteArrayConverterOptions.None)
			{
				FuzzingLogsCollector.Log("ByteArrayConverter", "ValidateOptions", 135);
				throw new ConfigurationException($"{nameof(ByteArrayConverter)} must be configured exclusively with HexDecimal options, or exclusively with Base64 options.  Was {options.ToString()}")
				{
					Data = { { "options", options } }
				};
			}
		}
		FuzzingLogsCollector.Log("ByteArrayConverter", "ValidateOptions", 142);
	}
}
