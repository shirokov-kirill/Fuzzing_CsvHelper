// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Runtime.CompilerServices;
using CsvHelper.FuzzingLogger;

namespace CsvHelper;

/// <summary>
/// Methods to help with arrays.
/// </summary>
public static class ArrayHelper
{
	/// <summary>
	/// Trims the characters off the start and end of the buffer
	/// by updating the start and length arguments.
	/// </summary>
	/// <param name="buffer">The buffer.</param>
	/// <param name="start">The start.</param>
	/// <param name="length">The length.</param>
	/// <param name="trimChars">The characters to trim.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Trim(char[] buffer, ref int start, ref int length, char[] trimChars)
	{
		FuzzingLogsCollector.Log("ArrayHelper", "Trim", 26);
		// Trim start.
		for (var i = start; i < start + length; i++)
		{
			FuzzingLogsCollector.Log("ArrayHelper", "Trim", 30);
			var c = buffer[i];
			if (!Contains(trimChars, c))
			{
				FuzzingLogsCollector.Log("ArrayHelper", "Trim", 34);
				break;
			}

			FuzzingLogsCollector.Log("ArrayHelper", "Trim", 38);
			start++;
			length--;
		}

		// Trim end.
		for (var i = start + length - 1; i > start; i--)
		{
			FuzzingLogsCollector.Log("ArrayHelper", "Trim", 46);
			var c = buffer[i];
			if (!Contains(trimChars, c))
			{
				FuzzingLogsCollector.Log("ArrayHelper", "Trim", 50);
				break;
			}

			FuzzingLogsCollector.Log("ArrayHelper", "Trim", 54);
			length--;
		}
	}

	/// <summary>
	/// Determines whether this given array contains the given character.
	/// </summary>
	/// <param name="array">The array to search.</param>
	/// <param name="c">The character to look for.</param>
	/// <returns>
	///   <c>true</c> if the array contains the characters, otherwise <c>false</c>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Contains(char[] array, in char c)
	{
		FuzzingLogsCollector.Log("ArrayHelper", "Contains", 70);
		for (var i = 0; i < array.Length; i++)
		{
			FuzzingLogsCollector.Log("ArrayHelper", "Contains", 73);
			if (array[i] == c)
			{
				FuzzingLogsCollector.Log("ArrayHelper", "Contains", 76);
				return true;
			}
		}

		FuzzingLogsCollector.Log("ArrayHelper", "Contains", 81);
		return false;
	}
}
