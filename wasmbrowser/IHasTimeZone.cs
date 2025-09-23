using System;

namespace wasmbrowser;

public interface IHasTimeZone
{
	TimeZoneInfo? TimeZone { get; }
}
