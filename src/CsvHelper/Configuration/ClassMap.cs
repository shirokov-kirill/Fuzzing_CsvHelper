// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Configuration;

///<summary>
/// Maps class members to CSV fields.
///</summary>
public abstract class ClassMap
{
	private static readonly List<Type> enumerableConverters = new List<Type>
	{
		typeof(ArrayConverter),
		typeof(CollectionGenericConverter),
		typeof(EnumerableConverter),
		typeof(IDictionaryConverter),
		typeof(IDictionaryGenericConverter),
		typeof(IEnumerableConverter),
		typeof(IEnumerableGenericConverter)
	};

	/// <summary>
	/// The type of the class this map is for.
	/// </summary>
	public virtual Type ClassType { get; private set; }

	/// <summary>
	/// The class constructor parameter mappings.
	/// </summary>
	public virtual List<ParameterMap> ParameterMaps { get; } = new List<ParameterMap>();

	/// <summary>
	/// The class member mappings.
	/// </summary>
	public virtual MemberMapCollection MemberMaps { get; } = new MemberMapCollection();

	/// <summary>
	/// The class member reference mappings.
	/// </summary>
	public virtual MemberReferenceMapCollection ReferenceMaps { get; } = new MemberReferenceMapCollection();

	/// <summary>
	/// Allow only internal creation of CsvClassMap.
	/// </summary>
	/// <param name="classType">The type of the class this map is for.</param>
	internal ClassMap(Type classType)
	{
		FuzzingLogsCollector.Log("ClassMap", "ClassMap", 57);
		ClassType = classType;
	}

	/// <summary>
	/// Maps a member to a CSV field.
	/// </summary>
	/// <param name="classType">The type of the class this map is for. This may not be the same type
	/// as the member.DeclaringType or the current ClassType due to nested member mappings.</param>
	/// <param name="member">The member to map.</param>
	/// <param name="useExistingMap">If true, an existing map will be used if available.
	/// If false, a new map is created for the same member.</param>
	/// <returns>The member mapping.</returns>
	public MemberMap Map(Type classType, MemberInfo member, bool useExistingMap = true)
	{
		FuzzingLogsCollector.Log("ClassMap", "Map", 72);
		if (useExistingMap)
		{
			FuzzingLogsCollector.Log("ClassMap", "Map", 75);
			var existingMap = MemberMaps.Find(member);
			if (existingMap != null)
			{
				FuzzingLogsCollector.Log("ClassMap", "Map", 79);
				return existingMap;
			}
		}
		FuzzingLogsCollector.Log("ClassMap", "Map", 83);
		var memberMap = MemberMap.CreateGeneric(classType, member);
		memberMap.Data.Index = GetMaxIndex() + 1;
		MemberMaps.Add(memberMap);

		return memberMap;
	}

	/// <summary>
	/// Maps a non-member to a CSV field. This allows for writing
	/// data that isn't mapped to a class member.
	/// </summary>
	/// <returns>The member mapping.</returns>
	public virtual MemberMap<object, object> Map()
	{
		FuzzingLogsCollector.Log("ClassMap", "Map", 98);
		var memberMap = new MemberMap<object, object>(null);
		memberMap.Data.Index = GetMaxIndex() + 1;
		MemberMaps.Add(memberMap);

		return memberMap;
	}

	/// <summary>
	/// Maps a member to another class map.
	/// </summary>
	/// <param name="classMapType">The type of the class map.</param>
	/// <param name="member">The member.</param>
	/// <param name="constructorArgs">Constructor arguments used to create the reference map.</param>
	/// <returns>The reference mapping for the member.</returns>
	public virtual MemberReferenceMap References(Type classMapType, MemberInfo member, params object[] constructorArgs)
	{
		FuzzingLogsCollector.Log("ClassMap", "References", 115);
		if (!typeof(ClassMap).IsAssignableFrom(classMapType))
		{
			FuzzingLogsCollector.Log("ClassMap", "References", 118);
			throw new InvalidOperationException($"Argument {nameof(classMapType)} is not a CsvClassMap.");
		}

		FuzzingLogsCollector.Log("ClassMap", "References", 122);
		var existingMap = ReferenceMaps.Find(member);

		if (existingMap != null)
		{
			FuzzingLogsCollector.Log("ClassMap", "References", 127);
			return existingMap;
		}

		FuzzingLogsCollector.Log("ClassMap", "References", 131);
		var map = (ClassMap)ObjectResolver.Current.Resolve(classMapType, constructorArgs);
		map.ReIndex(GetMaxIndex() + 1);
		var reference = new MemberReferenceMap(member, map);
		ReferenceMaps.Add(reference);

		return reference;
	}

	/// <summary>
	/// Maps a constructor parameter to a CSV field.
	/// </summary>
	/// <param name="name">The name of the constructor parameter.</param>
	public virtual ParameterMap Parameter(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			FuzzingLogsCollector.Log("ClassMap", "Parameter", 148);
			throw new ArgumentNullException(nameof(name));
		}
		FuzzingLogsCollector.Log("ClassMap", "Parameter", 151);
		var args = new GetConstructorArgs(ClassType);

		return Parameter(() => ConfigurationFunctions.GetConstructor(args), name);
	}

	/// <summary>
	/// Maps a constructor parameter to a CSV field.
	/// </summary>
	/// <param name="getConstructor">A function that returns the <see cref="ConstructorInfo"/> for the constructor.</param>
	/// <param name="name">The name of the constructor parameter.</param>
	public virtual ParameterMap Parameter(Func<ConstructorInfo> getConstructor, string name)
	{
		if (getConstructor == null)
		{
			FuzzingLogsCollector.Log("ClassMap", "Parameter", 166);
			throw new ArgumentNullException(nameof(getConstructor));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			FuzzingLogsCollector.Log("ClassMap", "Parameter", 172);
			throw new ArgumentNullException(nameof(name));
		}

		FuzzingLogsCollector.Log("ClassMap", "Parameter", 176);
		var constructor = getConstructor();
		var parameters = constructor.GetParameters();
		var parameter = parameters.SingleOrDefault(p => p.Name == name);
		if (parameter == null)
		{
			FuzzingLogsCollector.Log("ClassMap", "Parameter", 182);
			throw new ConfigurationException($"Constructor {constructor.GetDefinition()} doesn't contain a paramter with name '{name}'.");
		}

		FuzzingLogsCollector.Log("ClassMap", "Parameter", 186);
		return Parameter(constructor, parameter);
	}

	/// <summary>
	/// Maps a constructor parameter to a CSV field.
	/// </summary>
	/// <param name="constructor">The <see cref="ConstructorInfo"/> for the constructor.</param>
	/// <param name="parameter">The <see cref="ParameterInfo"/> for the constructor parameter.</param>
	public virtual ParameterMap Parameter(ConstructorInfo constructor, ParameterInfo parameter)
	{
		if (constructor == null)
		{
			FuzzingLogsCollector.Log("ClassMap", "Parameter", 199);
			throw new ArgumentNullException(nameof(constructor));
		}

		if (parameter == null)
		{
			FuzzingLogsCollector.Log("ClassMap", "Parameter", 205);
			throw new ArgumentNullException(nameof(parameter));
		}

		if (!constructor.GetParameters().Contains(parameter))
		{
			FuzzingLogsCollector.Log("ClassMap", "Parameter", 211);
			throw new ConfigurationException($"Constructor {constructor.GetDefinition()} doesn't contain parameter '{parameter.GetDefinition()}'.");
		}

		FuzzingLogsCollector.Log("ClassMap", "Parameter", 215);
		var parameterMap = new ParameterMap(parameter);
		parameterMap.Data.Index = GetMaxIndex(isParameter: true) + 1;
		ParameterMaps.Add(parameterMap);

		return parameterMap;
	}

	/// <summary>
	/// Auto maps all members for the given type. If a member
	/// is mapped again it will override the existing map.
	/// </summary>
	/// <param name="culture">The culture.</param>
	public virtual void AutoMap(CultureInfo culture)
	{
		AutoMap(new CsvConfiguration(culture));
	}

	/// <summary>
	/// Auto maps all members for the given type. If a member
	/// is mapped again it will override the existing map.
	/// </summary>
	/// <param name="configuration">The configuration.</param>
	public virtual void AutoMap(CsvConfiguration configuration)
	{
		AutoMap(new CsvContext(configuration));
	}

	/// <summary>
	/// Auto maps all members for the given type. If a member
	/// is mapped again it will override the existing map.
	/// </summary>
	/// <param name="context">The context.</param>
	public virtual void AutoMap(CsvContext context)
	{
		FuzzingLogsCollector.Log("ClassMap", "AutoMap", 250);
		var type = GetGenericType();
		if (typeof(IEnumerable).IsAssignableFrom(type))
		{
			FuzzingLogsCollector.Log("ClassMap", "AutoMap", 254);
			throw new ConfigurationException("Types that inherit IEnumerable cannot be auto mapped. " +
											 "Did you accidentally call GetRecord or WriteRecord which " +
											 "acts on a single record instead of calling GetRecords or " +
											 "WriteRecords which acts on a list of records?");
		}

		FuzzingLogsCollector.Log("ClassMap", "AutoMap", 261);
		var mapParents = new LinkedList<Type>();
		var args = new ShouldUseConstructorParametersArgs(type);
		if (context.Configuration.ShouldUseConstructorParameters(args))
		{
			FuzzingLogsCollector.Log("ClassMap", "AutoMap", 266);
			// This type doesn't have a parameterless constructor so we can't create an
			// instance and set it's member. Constructor parameters need to be created
			// instead. Writing only uses getters, so members will also be mapped
			// for writing purposes.
			AutoMapConstructorParameters(this, context, mapParents);
		}
		FuzzingLogsCollector.Log("ClassMap", "AutoMap", 273);
		AutoMapMembers(this, context, mapParents);
	}

	/// <summary>
	/// Get the largest index for the
	/// members and references.
	/// </summary>
	/// <returns>The max index.</returns>
	public virtual int GetMaxIndex(bool isParameter = false)
	{
		if (isParameter)
		{
			FuzzingLogsCollector.Log("ClassMap", "GetMaxIndex", 286);
			return ParameterMaps.Select(parameterMap => parameterMap.GetMaxIndex()).DefaultIfEmpty(-1).Max();
		}

		if (MemberMaps.Count == 0 && ReferenceMaps.Count == 0)
		{
			FuzzingLogsCollector.Log("ClassMap", "GetMaxIndex", 292);
			return -1;
		}

		FuzzingLogsCollector.Log("ClassMap", "GetMaxIndex", 296);
		var indexes = new List<int>();
		if (MemberMaps.Count > 0)
		{
			FuzzingLogsCollector.Log("ClassMap", "GetMaxIndex", 300);
			indexes.Add(MemberMaps.Max(pm => pm.Data.Index));
		}

		if (ReferenceMaps.Count > 0)
		{
			FuzzingLogsCollector.Log("ClassMap", "GetMaxIndex", 306);
			indexes.AddRange(ReferenceMaps.Select(referenceMap => referenceMap.GetMaxIndex()));
		}

		FuzzingLogsCollector.Log("ClassMap", "GetMaxIndex", 310);
		return indexes.Max();
	}

	/// <summary>
	/// Resets the indexes based on the given start index.
	/// </summary>
	/// <param name="indexStart">The index start.</param>
	/// <returns>The last index + 1.</returns>
	public virtual int ReIndex(int indexStart = 0)
	{
		FuzzingLogsCollector.Log("ClassMap", "ReIndex", 321);
		foreach (var parameterMap in ParameterMaps)
		{
			FuzzingLogsCollector.Log("ClassMap", "ReIndex", 324);
			parameterMap.Data.Index = indexStart + parameterMap.Data.Index;
		}

		foreach (var memberMap in MemberMaps)
		{
			FuzzingLogsCollector.Log("ClassMap", "ReIndex", 330);
			if (!memberMap.Data.IsIndexSet)
			{
				FuzzingLogsCollector.Log("ClassMap", "ReIndex", 333);
				memberMap.Data.Index = indexStart + memberMap.Data.Index;
			}
		}

		foreach (var referenceMap in ReferenceMaps)
		{
			FuzzingLogsCollector.Log("ClassMap", "ReIndex", 340);
			indexStart = referenceMap.Data.Mapping.ReIndex(indexStart);
		}

		return indexStart;
	}

	/// <summary>
	/// Auto maps the given map and checks for circular references as it goes.
	/// </summary>
	/// <param name="map">The map to auto map.</param>
	/// <param name="context">The context.</param>
	/// <param name="mapParents">The list of parents for the map.</param>
	/// <param name="indexStart">The index starting point.</param>
	protected virtual void AutoMapMembers(ClassMap map, CsvContext context, LinkedList<Type> mapParents, int indexStart = 0)
	{
		FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 356);
		var type = map.GetGenericType();

		var flags = BindingFlags.Instance | BindingFlags.Public;
		if (context.Configuration.IncludePrivateMembers)
		{
			FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 362);
			flags = flags | BindingFlags.NonPublic;
		}

		FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 366);
		var members = new List<MemberInfo>();
		if ((context.Configuration.MemberTypes & MemberTypes.Properties) == MemberTypes.Properties)
		{
			FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 370);
			// We need to go up the declaration tree and find the actual type the property
			// exists on and use that PropertyInfo instead. This is so we can get the private
			// set method for the property.
			var properties = new List<PropertyInfo>();
			foreach (var property in ReflectionHelper.GetUniqueProperties(type, flags))
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 377);
				if (properties.Any(p => p.Name == property.Name))
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 380);
					// Multiple properties could have the same name if a child class property
					// is hiding a parent class property by using `new`. It's possible that
					// the order of the properties returned
					continue;
				}
				FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 386);
				properties.Add(ReflectionHelper.GetDeclaringProperty(type, property, flags));
			}
			FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 389);
			members.AddRange(properties);
		}

		if ((context.Configuration.MemberTypes & MemberTypes.Fields) == MemberTypes.Fields)
		{
			FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 395);
			// We need to go up the declaration tree and find the actual type the field
			// exists on and use that FieldInfo instead.
			var fields = new List<MemberInfo>();
			foreach (var field in ReflectionHelper.GetUniqueFields(type, flags))
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 401);
				if (fields.Any(p => p.Name == field.Name))
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 404);
					// Multiple fields could have the same name if a child class field
					// is hiding a parent class field by using `new`. It's possible that
					// the order of the fields returned
					continue;
				}

				if (!field.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any())
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 413);
					fields.Add(ReflectionHelper.GetDeclaringField(type, field, flags));
				}
			}
			members.AddRange(fields);
		}

		foreach (var member in members)
		{
			FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 422);
			if (member.GetCustomAttribute<IgnoreAttribute>() != null)
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 425);
				// Ignore this member including its tree if it's a reference.
				continue;
			}

			var typeConverterType = context.TypeConverterCache.GetConverter(member).GetType();

			if (context.Configuration.HasHeaderRecord && enumerableConverters.Contains(typeConverterType))
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 434);
				// Enumerable converters can't write the header properly, so skip it.
				continue;
			}

			var memberTypeInfo = member.MemberType().GetTypeInfo();
			var isDefaultConverter = typeConverterType == typeof(DefaultTypeConverter);
			if (isDefaultConverter)
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 443);
				// If the type is not one covered by our type converters
				// and it has a parameterless constructor, create a
				// reference map for it.

				if (context.Configuration.IgnoreReferences)
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 450);
					continue;
				}

				if (CheckForCircularReference(member.MemberType(), mapParents))
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 456);
					continue;
				}

				mapParents.AddLast(type);
				var refMapType = typeof(DefaultClassMap<>).MakeGenericType(member.MemberType());
				var refMap = (ClassMap)ObjectResolver.Current.Resolve(refMapType);

				if (memberTypeInfo.HasConstructor() && !memberTypeInfo.HasParameterlessConstructor() && !memberTypeInfo.IsUserDefinedStruct())
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 466);
					AutoMapConstructorParameters(refMap, context, mapParents, Math.Max(map.GetMaxIndex() + 1, indexStart));
				}

				// Need to use Max here for nested types.
				AutoMapMembers(refMap, context, mapParents, Math.Max(map.GetMaxIndex() + 1, indexStart));
				mapParents.Drop(mapParents.Find(type));

				if (refMap.MemberMaps.Count > 0 || refMap.ReferenceMaps.Count > 0)
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 476);
					var referenceMap = new MemberReferenceMap(member, refMap);
					if (context.Configuration.ReferenceHeaderPrefix != null)
					{
						FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 480);
						var args = new ReferenceHeaderPrefixArgs(member.MemberType(), member.Name);
						referenceMap.Data.Prefix = context.Configuration.ReferenceHeaderPrefix(args);
					}

					ApplyAttributes(referenceMap);

					map.ReferenceMaps.Add(referenceMap);
				}
			}
			else
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 492);
				// Only add the member map if it can be converted later on.
				// If the member will use the default converter, don't add it because
				// we don't want the .ToString() value to be used when auto mapping.

				// Use the top of the map tree. This will maps that have been auto mapped
				// to later on get a reference to a map by doing map.Map( m => m.A.B.C.Id )
				// and it will return the correct parent map type of A instead of C.
				var classType = mapParents.First?.Value ?? map.ClassType;
				var memberMap = MemberMap.CreateGeneric(classType, member);

				// Use global values as the starting point.
				memberMap.Data.TypeConverterOptions = TypeConverterOptions.Merge(new TypeConverterOptions(), context.TypeConverterOptionsCache.GetOptions(member.MemberType()), memberMap.Data.TypeConverterOptions);
				memberMap.Data.Index = map.GetMaxIndex() + 1;

				ApplyAttributes(memberMap);

				map.MemberMaps.Add(memberMap);
			}
		}
		FuzzingLogsCollector.Log("ClassMap", "AutoMapMembers", 512);
		map.ReIndex(indexStart);
	}

	/// <summary>
	/// Auto maps the given map using constructor parameters.
	/// </summary>
	/// <param name="map">The map.</param>
	/// <param name="context">The context.</param>
	/// <param name="mapParents">The list of parents for the map.</param>
	/// <param name="indexStart">The index starting point.</param>
	protected virtual void AutoMapConstructorParameters(ClassMap map, CsvContext context, LinkedList<Type> mapParents, int indexStart = 0)
	{
		FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 525);
		var type = map.GetGenericType();
		var args = new GetConstructorArgs(map.ClassType);
		var constructor = context.Configuration.GetConstructor(args);
		var parameters = constructor.GetParameters();

		foreach (var parameter in parameters)
		{
			FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 533);
			var parameterMap = new ParameterMap(parameter);

			if (parameter.GetCustomAttributes<IgnoreAttribute>(true).Any() || parameter.GetCustomAttributes<ConstantAttribute>(true).Any())
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 538);
				// If there is an IgnoreAttribute or ConstantAttribute, we still need to add a map because a constructor requires
				// all parameters to be present. A default value will be used later on.

				ApplyAttributes(parameterMap);
				map.ParameterMaps.Add(parameterMap);
				continue;
			}

			var typeConverterType = context.TypeConverterCache.GetConverter(parameter.ParameterType).GetType();
			var memberTypeInfo = parameter.ParameterType.GetTypeInfo();
			var isDefaultConverter = typeConverterType == typeof(DefaultTypeConverter);
			if (isDefaultConverter && (memberTypeInfo.HasParameterlessConstructor() || memberTypeInfo.IsUserDefinedStruct()))
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 552);
				// If the type is not one covered by our type converters
				// and it has a parameterless constructor, create a
				// reference map for it.

				if (context.Configuration.IgnoreReferences)
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 559);
					throw new InvalidOperationException($"Configuration '{nameof(CsvConfiguration.IgnoreReferences)}' can't be true " +
														  "when using types without a default constructor. Constructor parameters " +
														  "are used and all members including references must be used.");
				}

				if (CheckForCircularReference(parameter.ParameterType, mapParents))
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 567);
					throw new InvalidOperationException($"A circular reference was detected in constructor paramter '{parameter.Name}'." +
														  "Since all parameters must be supplied for a constructor, this parameter can't be skipped.");
				}

				mapParents.AddLast(type);
				var refMapType = typeof(DefaultClassMap<>).MakeGenericType(parameter.ParameterType);
				var refMap = (ClassMap)ObjectResolver.Current.Resolve(refMapType);
				AutoMapMembers(refMap, context, mapParents, Math.Max(map.GetMaxIndex(isParameter: true) + 1, indexStart));
				mapParents.Drop(mapParents.Find(type));

				var referenceMap = new ParameterReferenceMap(parameter, refMap);
				if (context.Configuration.ReferenceHeaderPrefix != null)
				{
					FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 581);
					var referenceHeaderPrefix = new ReferenceHeaderPrefixArgs(memberTypeInfo.MemberType(), memberTypeInfo.Name);
					referenceMap.Data.Prefix = context.Configuration.ReferenceHeaderPrefix(referenceHeaderPrefix);
				}

				ApplyAttributes(referenceMap);

				parameterMap.ReferenceMap = referenceMap;
			}
			else if (isDefaultConverter && context.Configuration.ShouldUseConstructorParameters(new ShouldUseConstructorParametersArgs(parameter.ParameterType)))
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 592);
				// If the type is not one covered by our type converters
				// and it should use contructor parameters, create a
				// constructor map for it.

				mapParents.AddLast(type);
				var constructorMapType = typeof(DefaultClassMap<>).MakeGenericType(parameter.ParameterType);
				var constructorMap = (ClassMap)ObjectResolver.Current.Resolve(constructorMapType);
				// Need to use Max here for nested types.
				AutoMapConstructorParameters(constructorMap, context, mapParents, Math.Max(map.GetMaxIndex(isParameter: true) + 1, indexStart));
				mapParents.Drop(mapParents.Find(type));

				parameterMap.ConstructorTypeMap = constructorMap;
			}
			else
			{
				FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 608);
				parameterMap.Data.TypeConverterOptions = TypeConverterOptions.Merge(new TypeConverterOptions(), context.TypeConverterOptionsCache.GetOptions(parameter.ParameterType), parameterMap.Data.TypeConverterOptions);
				parameterMap.Data.Index = map.GetMaxIndex(isParameter: true) + 1;

				ApplyAttributes(parameterMap);
			}

			FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 615);
			map.ParameterMaps.Add(parameterMap);
		}
		FuzzingLogsCollector.Log("ClassMap", "AutoMapConstructorParameters", 618);
		map.ReIndex(indexStart);
	}

	/// <summary>
	/// Checks for circular references.
	/// </summary>
	/// <param name="type">The type to check for.</param>
	/// <param name="mapParents">The list of parents to check against.</param>
	/// <returns>A value indicating if a circular reference was found.
	/// True if a circular reference was found, otherwise false.</returns>
	protected virtual bool CheckForCircularReference(Type type, LinkedList<Type> mapParents)
	{
		FuzzingLogsCollector.Log("ClassMap", "CheckForCircularReference", 631);
		if (mapParents.Count == 0)
		{
			FuzzingLogsCollector.Log("ClassMap", "CheckForCircularReference", 634);
			return false;
		}

		var node = mapParents.Last;
		while (true)
		{
			FuzzingLogsCollector.Log("ClassMap", "CheckForCircularReference", 641);
			if (node?.Value == type)
			{
				FuzzingLogsCollector.Log("ClassMap", "CheckForCircularReference", 644);
				return true;
			}

			node = node?.Previous;
			if (node == null)
			{
				FuzzingLogsCollector.Log("ClassMap", "CheckForCircularReference", 651);
				break;
			}
		}

		return false;
	}

	/// <summary>
	/// Gets the generic type for this class map.
	/// </summary>
	protected virtual Type GetGenericType()
	{
		FuzzingLogsCollector.Log("ClassMap", "GetGenericType", 664);
		return GetType().GetTypeInfo().BaseType?.GetGenericArguments()[0] ?? throw new ConfigurationException();
	}

	/// <summary>
	/// Applies attribute configurations to the map.
	/// </summary>
	/// <param name="parameterMap">The parameter map.</param>
	protected virtual void ApplyAttributes(ParameterMap parameterMap)
	{
		FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 674);
		var parameter = parameterMap.Data.Parameter;
		var attributes = parameter.GetCustomAttributes().OfType<IParameterMapper>();

		foreach (var attribute in attributes)
		{
			FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 680);
			attribute.ApplyTo(parameterMap);
		}
	}

	/// <summary>
	/// Applies attribute configurations to the map.
	/// </summary>
	/// <param name="referenceMap">The parameter reference map.</param>
	protected virtual void ApplyAttributes(ParameterReferenceMap referenceMap)
	{
		FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 691);
		var parameter = referenceMap.Data.Parameter;
		var attributes = parameter.GetCustomAttributes().OfType<IParameterReferenceMapper>();

		foreach (var attribute in attributes)
		{
			FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 697);
			attribute.ApplyTo(referenceMap);
		}
	}

	/// <summary>
	/// Applies attribute configurations to the map.
	/// </summary>
	/// <param name="memberMap">The member map.</param>
	protected virtual void ApplyAttributes(MemberMap memberMap)
	{
		FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 708);
		if (memberMap.Data.Member == null)
		{
			FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 711);
			return;
		}

		FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 715);
		var member = memberMap.Data.Member;
		var attributes = member.GetCustomAttributes().OfType<IMemberMapper>();

		foreach (var attribute in attributes)
		{
			FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 721);
			attribute.ApplyTo(memberMap);
		}
	}

	/// <summary>
	/// Applies attribute configurations to the map.
	/// </summary>
	/// <param name="referenceMap">The member reference map.</param>
	protected virtual void ApplyAttributes(MemberReferenceMap referenceMap)
	{
		FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 732);
		var member = referenceMap.Data.Member;
		var attributes = member.GetCustomAttributes().OfType<IMemberReferenceMapper>();

		foreach (var attribute in attributes)
		{
			FuzzingLogsCollector.Log("ClassMap", "ApplyAttributes", 738);
			attribute.ApplyTo(referenceMap);
		}
	}
}
