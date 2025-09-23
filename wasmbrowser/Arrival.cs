using System.ComponentModel;

namespace wasmbrowser;
[DisplayName(nameof(Arrival))]
[JsonDerivedTypes(typeof(PeriodicArrival), typeof(ScheduledArrival))]
public abstract class Arrival
{
	public string Entity { get; set; } = null!;
	public ReferenceExpression Location { get; set; } = null!;
	[JsonDefault(1)]
	public Expression<double> Units { get; set; } = 1;
	public Expression<int>? TakePriority { get; set; }
	public bool PreventFailures { get; set; }
	public int? EventPriority { get; set; }
}
