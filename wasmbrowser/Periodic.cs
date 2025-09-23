namespace wasmbrowser;

public sealed class PeriodicArrival : Arrival
{
	[JsonDefault(1)]
	public Expression<int> Quantity { get; set; } = 1;

	[JsonDefault(1)]
	public Expression<double> Interval { get; set; } = 1;

	public Expression<double> FirstTime { get; set; } = 0;

	public Expression<int>? Occurrences { get; set; }
}
