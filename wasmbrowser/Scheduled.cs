using System;
using System.Collections.Generic;

namespace wasmbrowser;

public sealed class ScheduledArrival : Arrival
{
	public IList<ScheduledArrivalEntry> Entries { get; set; } = [];
}

public sealed class ScheduledArrivalEntry
{
	public DateTime Date { get; set; }
	[JsonDefault(1)]
	public Expression<int> Quantity { get; set; } = 1;
}
