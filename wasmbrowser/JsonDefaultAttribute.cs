using System;

namespace wasmbrowser;

[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonDefaultAttribute : Attribute
{
	public object? DefaultValue { get; }

	public bool UseDefaultValueFromProperty { get; }

	public string? DefaultPropertyName { get; }

	public JsonDefaultAttribute(object defaultValue)
	{
		DefaultValue = defaultValue;
	}
	public JsonDefaultAttribute(string defaultPropertyName, bool useDefaultValueFromProperty)
	{
		if (!useDefaultValueFromProperty)
			throw new ArgumentException($"{nameof(useDefaultValueFromProperty)} must be true when specifying {nameof(defaultPropertyName)}.");

		DefaultPropertyName = defaultPropertyName;
		UseDefaultValueFromProperty = useDefaultValueFromProperty;
	}
}
