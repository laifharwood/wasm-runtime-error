using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace wasmbrowser;

public static class DateTimeUtility
{
	private static readonly Regex s_hasTimeZonePattern = new(@"(?:[+-](?:[0-9]|[0-1][0-9])(?::[0-5]?[0-9])?$)|(?:Z$)", RegexOptions.Compiled);
	private static readonly ConcurrentDictionary<TimeSpan, TimeZoneInfo> s_timeZonesByOffset = [];

	public static bool TryParse(string dateTimeIsoString, TimeZoneInfo? destinationTimeZone, out DateTime dateTime,
		IFormatProvider? formatProvider = null)
	{
		if (!DateTimeOffset.TryParse(dateTimeIsoString, formatProvider, System.Globalization.DateTimeStyles.None, out DateTimeOffset dateTimeOffset))
		{
			dateTime = default;
			return false;
		}
		destinationTimeZone ??= TimeZoneInfo.FindSystemTimeZoneById("UTC");
		if (s_hasTimeZonePattern.IsMatch(dateTimeIsoString))
		{
			TimeZoneInfo sourceTimeZone = s_timeZonesByOffset.GetOrAdd(dateTimeOffset.Offset, key =>
				TimeZoneInfo.CreateCustomTimeZone("id", key, string.Empty, string.Empty));
			dateTime = TimeZoneInfo.ConvertTime(dateTimeOffset.DateTime, sourceTimeZone, destinationTimeZone);
			dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified); // Prevent dates from being serialized w/ time zone offset
		}
		else
		{
			dateTime = dateTimeOffset.DateTime;
		}
		return true;
	}
	public static string ToString(DateTime dateTime)
	{
		var dateTimeUnspecifiedKind = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
		return dateTimeUnspecifiedKind.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
	}

	[return: NotNullIfNotNull(nameof(dateTime))]
	public static string? ToString(DateTime? dateTime) => (dateTime != null) ? ToString(dateTime.Value) : null;
}
