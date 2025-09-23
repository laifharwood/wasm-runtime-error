using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace wasmbrowser;

[System.Diagnostics.DebuggerDisplay("{(object?)Script ?? Value}")]
[JsonConverter(typeof(ExpressionJsonConverter))]
public sealed class Expression<T> : ICompilable
	where T : struct
{
	#region Fields/Properties
	public T Value { get; set; }

	public string? Script { get; }

	[JsonIgnore]
	public bool HasValue => Script is null;

	[JsonIgnore]
	object? ICompilable.Executor { get; set; }
	#endregion

	#region Initialization
	public Expression()
	{
	}

	public Expression(T value) => Value = value;

	public Expression(string script) => Script = script;

	[JsonConstructor, EditorBrowsable(EditorBrowsableState.Never)]
	public Expression(T value, string? script)
	{
		Value = value;
		Script = script;
	}
	#endregion

	#region Methods
	public override string ToString() => Script ?? Value.ToString();
	#endregion

	#region Operator Declarations
	public static implicit operator Expression<T>(T value) => new(value);
	public static implicit operator Expression<T>(string? script) => (script == null) ? null! : new(script);
	#endregion
}

[System.Diagnostics.DebuggerDisplay("{Value}")]
[JsonConverter(typeof(ReferenceExpressionJsonConverter))]
public sealed class ReferenceExpression(string value) : ICompilable
{
	#region Fields/Properties
	public string Value { get; } = value;
	[JsonIgnore]
	string? ICompilable.Script => Value;
	[JsonIgnore]
	object? ICompilable.Executor { get; set; }
	#endregion

	#region Methods
	public override string ToString() => Value;
	#endregion

	#region Operator Declarations
	public static implicit operator ReferenceExpression(string? value) => (value == null) ? null! : new(value);
	#endregion
}

#region Classes
internal sealed class ExpressionJsonConverter : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Expression<>);
	}
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type valueType = typeToConvert.GetGenericArguments()[0];
		if (valueType == typeof(DateTime))
		{
			return new JsonDateTimeConverter(options);
		}

		return (JsonConverter)Activator.CreateInstance(
			typeof(JsonTypedConverter<>).MakeGenericType(valueType),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: [options],
			culture: null);
	}

	private class JsonTypedConverter<T>(JsonSerializerOptions options) : JsonConverter<Expression<T>>
		where T : struct
	{
		protected readonly JsonConverter<T> ValueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
		protected readonly JsonConverter<string> StringConverter = (JsonConverter<string>)options.GetConverter(typeof(string));

		public override Expression<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartObject)
			{
				throw new JsonException();
			}

			return (reader.TokenType == JsonTokenType.String)
				? new Expression<T>(StringConverter.Read(ref reader, typeof(string), options)!)
				: new Expression<T>(ValueConverter.Read(ref reader, typeof(T), options)!);
		}
		public override void Write(Utf8JsonWriter writer, Expression<T> value, JsonSerializerOptions options)
		{
			if (value.Script is null)
			{
				ValueConverter.Write(writer, value.Value, options);
			}
			else
			{
				StringConverter.Write(writer, value.Script, options);
			}
		}
	}
	private sealed class JsonDateTimeConverter(JsonSerializerOptions options) : JsonTypedConverter<DateTime>(options)
	{
		public override Expression<DateTime> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string value = StringConverter.Read(ref reader, typeof(string), options)!;
			JsonConverter? converterWithTimeZone = options.Converters.FirstOrDefault(o => o is IHasTimeZone);
			if (converterWithTimeZone is IHasTimeZone hasTimeZone && DateTimeUtility.TryParse(value, hasTimeZone.TimeZone, out DateTime dateTime))
			{
				return new Expression<DateTime>(dateTime);
			}

			return new Expression<DateTime>(value);
		}
	}
}

internal sealed class ReferenceExpressionJsonConverter : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(ReferenceExpression);
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		return (JsonConverter)Activator.CreateInstance(
			typeof(JsonTypedConverter),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: [options],
			culture: null);
	}

	private sealed class JsonTypedConverter(JsonSerializerOptions options) : JsonConverter<ReferenceExpression>
	{
		private readonly JsonConverter<string> _stringConverter = (JsonConverter<string>)options.GetConverter(typeof(string));

		public override ReferenceExpression Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartObject)
			{
				throw new JsonException();
			}

			return new ReferenceExpression(_stringConverter.Read(ref reader, typeof(string), options)!);
		}
		public override void Write(Utf8JsonWriter writer, ReferenceExpression value, JsonSerializerOptions options)
		{
			_stringConverter.Write(writer, value.Value, options);
		}
	}
}
#endregion
