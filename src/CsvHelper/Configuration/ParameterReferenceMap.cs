// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Configuration;

/// <summary>
/// Mapping info for a reference parameter mapping to a class.
/// </summary>
public class ParameterReferenceMap
{
	private readonly ParameterReferenceMapData data;

	/// <summary>
	/// Gets the parameter reference map data.
	/// </summary>
	public ParameterReferenceMapData Data => data;

	/// <summary>
	/// Initializes a new instance of the <see cref="ParameterReferenceMap"/> class.
	/// </summary>
	/// <param name="parameter">The parameter.</param>
	/// <param name="mapping">The <see cref="ClassMap"/> to use for the reference map.</param>
	public ParameterReferenceMap(ParameterInfo parameter, ClassMap mapping)
	{
		FuzzingLogsCollector.Log("ParameterReferenceMap", "ParameterReferenceMap", 29);
		if (mapping == null)
		{
			FuzzingLogsCollector.Log("ParameterReferenceMap", "ParameterReferenceMap", 32);
			throw new ArgumentNullException(nameof(mapping));
		}

		data = new ParameterReferenceMapData(parameter, mapping);
	}

	/// <summary>
	/// Appends a prefix to the header of each field of the reference parameter.
	/// </summary>
	/// <param name="prefix">The prefix to be prepended to headers of each reference parameter.</param>
	/// <param name="inherit">Inherit parent prefixes.</param>
	/// <returns>The current <see cref="ParameterReferenceMap" /></returns>
	public ParameterReferenceMap Prefix(string? prefix = null, bool inherit = false)
	{
		FuzzingLogsCollector.Log("ParameterReferenceMap", "Prefix", 47);
		if (string.IsNullOrEmpty(prefix))
		{
			FuzzingLogsCollector.Log("ParameterReferenceMap", "Prefix", 50);
			prefix = data.Parameter.Name + ".";
		}

		data.Inherit = inherit;
		data.Prefix = prefix!;

		FuzzingLogsCollector.Log("ParameterReferenceMap", "Prefix", 57);
		return this;
	}

	/// <summary>
	/// Get the largest index for the
	/// members and references.
	/// </summary>
	/// <returns>The max index.</returns>
	internal int GetMaxIndex()
	{
		FuzzingLogsCollector.Log("ParameterReferenceMap", "GetMaxIndex", 68);
		return data.Mapping.GetMaxIndex();
	}
}
