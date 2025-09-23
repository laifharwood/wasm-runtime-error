using System;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace wasmbrowser;

partial class TestManager
{
	[JSExport]
	internal static Task ErrorInThreads()
	{
		TestModel model = new()
		{
			Arrivals = [new PeriodicArrival { Entity = "COG", Location = "Box_Container_Pallet", FirstTime = 1, Interval = 1 }],
		};
		for (int i = 0; i < 1000; i++)
		{
			var thread = new Thread(() =>
			{
				Console.WriteLine($"Work on thread: {Environment.CurrentManagedThreadId}");
				Thread.Sleep(new Random().Next(1, 5));
				string modelJson = JsonSerializer.Serialize<TestModel>(model);
				TestModel newModel = JsonSerializer.Deserialize<TestModel>(modelJson);
			});
			thread.Start();
		}

		return Task.CompletedTask;
	}
}

