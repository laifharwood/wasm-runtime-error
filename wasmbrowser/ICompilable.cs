namespace wasmbrowser;

public interface ICompilable
{
	string? Script { get; }

	object? Executor { get; set; }
}

public interface IDynamicCompilable : IHasName, ICompilable
{
	bool DynamicEvaluation { get; }
	object? Evaluator { get; set; }
}
