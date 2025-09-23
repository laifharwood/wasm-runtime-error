using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace wasmbrowser;

[AttributeUsage(AttributeTargets.Class)]
public sealed class JsonDerivedTypesAttribute(params Type[] types) : JsonConverterAttribute
{
	public const string TypeDiscriminatorPropertyName = "$type";
	public Type[] Types => types;

	public override JsonConverter? CreateConverter(Type typeToConvert)
	{
		Type genericType = typeof(JsonDerivedTypesConverter<>);
		Type constructedType = genericType.MakeGenericType(typeToConvert);
		return (JsonConverter?)Activator.CreateInstance(constructedType, types);
	}

	private sealed class JsonDerivedTypesConverter<T>(params Type[] types) : JsonConverter<T> where T : class
	{
		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var jsonDocument = JsonDocument.ParseValue(ref reader);
			if (jsonDocument.RootElement.TryGetProperty(TypeDiscriminatorPropertyName, out JsonElement typeElement))
			{
				string? typeName = typeElement.GetString();
				if (!string.IsNullOrEmpty(typeName))
				{
					foreach (Type type in types)
					{
						if (typeName!.Equals(type.Name, StringComparison.OrdinalIgnoreCase))
						{
							return JsonSerializer.Deserialize(jsonDocument.RootElement.GetRawText(), type, options) as T;
						}
					}
				}
				throw new JsonException($"'{typeName}' is an invalid value for '{TypeDiscriminatorPropertyName}' property. " +
					$"Valid values: {string.Join(", ", types.Select(o => o.Name))}.");
			}
			throw new JsonException($"Required '{TypeDiscriminatorPropertyName}' property is missing.");
		}
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			Type type = value.GetType();
			JsonTypeInfo jsonTypeInfo = options.TypeInfoResolver?.GetTypeInfo(type, options)!;
			var jsonPropertiesByName = jsonTypeInfo.Properties.ToDictionary(o => o.Name, StringComparer.OrdinalIgnoreCase);
			writer.WriteStartObject();
			writer.WriteString(TypeDiscriminatorPropertyName, type.Name);
			foreach (System.Reflection.PropertyInfo? property in type.GetProperties())
			{
				object propertyValue = property.GetValue(value);
				JsonPropertyInfo jsonPropertyInfo = jsonPropertiesByName[property.Name];
				if (jsonPropertyInfo.ShouldSerialize != null && !jsonPropertyInfo.ShouldSerialize(value, propertyValue))
				{
					continue;
				}
				writer.WritePropertyName(jsonPropertyInfo.Name);
				JsonSerializer.Serialize(writer, propertyValue, options);
			}
			writer.WriteEndObject();
		}
	}
}
