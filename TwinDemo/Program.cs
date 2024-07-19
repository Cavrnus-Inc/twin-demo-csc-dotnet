using CavrnusSdk.API;
using Twin.Connector;

internal class Program
{
	private static Twin_Highway_Fake highway;

	private static async Task Main(string[] args)
	{
		Console.WriteLine("Please enter your domain:");
		string server = Console.ReadLine();
		Console.WriteLine("Starting up Cavrnus...");

		CavrnusFunctionLibrary.InitializeCavrnus(false);

		//Console.WriteLine("Type your server:");
		string userName = "Twin Data Connector";

		var authRes = await CavrnusFunctionLibrary.AuthenticateAsGuest(server, userName);

		if (!authRes.IsOk)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Failed to log in: {authRes.Error}");
			Console.ResetColor();
			Console.ReadLine();
			return;
		}

		Console.WriteLine("Please enter space Join ID:");
		string spaceJoinId = Console.ReadLine();

		Console.WriteLine("Authenticated, connecting to space...");

		var spaceConnRes = await CavrnusFunctionLibrary.JoinSpace(spaceJoinId);

		if (!spaceConnRes.IsOk)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Failed to join space: {spaceConnRes.Error}");
			Console.WriteLine($"Are you sure you created a space with Join ID: \"twin-demo\"?");
			Console.ResetColor();
			Console.ReadLine();
			return;
		}
		var spaceConn = spaceConnRes.Value;

		Console.WriteLine("Connection Complete!");

		highway = new Twin_Highway_Fake();

		PollAndSend(spaceConn);

		Console.WriteLine("Data now sending data from virtual sensors...");

		Console.WriteLine("INSTRUCTIONS:");
		Console.WriteLine("\tType 'crisis'+Enter to set the value of a sensor to emergency levels.");
		Console.WriteLine("\tType 'solve'+Enter to \"fix\" the issue and set the levels back to their normal range.");

		while (true)
		{
			string cmd = Console.ReadLine();
			highway.SendCommand(cmd);
		}

	}

	private static async Task PollAndSend(CavrnusSpaceConnection spaceConn)
	{
		await Task.Delay(1000);

		//Console.WriteLine("Polling and Sending");

		foreach (var sensor in highway.AllSensors)
		{
			if (sensor is TwinAirSensor airSensor)
			{
				spaceConn.PostBoolPropertyUpdate(airSensor.Id, "Crisis", airSensor.Crisis);

				spaceConn.PostFloatPropertyUpdate(airSensor.Id, "CO-ppm", (float)airSensor.CurrCOppm);
				//Console.WriteLine($"Posting {airSensor.Id} CO {airSensor.CurrCOppm} ppm");
				spaceConn.PostFloatPropertyUpdate(airSensor.Id, "NO2-ppb", (float)airSensor.CurrNO2ppb);
				//Console.WriteLine($"Posting {airSensor.Id} NO2 {airSensor.CurrNO2ppb} ppb");
			}
			if (sensor is TwinTempSensor tempSensor)
			{
				spaceConn.PostBoolPropertyUpdate(tempSensor.Id, "Crisis", tempSensor.Crisis);

				spaceConn.PostFloatPropertyUpdate(tempSensor.Id, "Temp-C", (float)tempSensor.CurrTemperatureC);
				//Console.WriteLine($"Posting {tempSensor.Id} Temperature {tempSensor.CurrTemperatureC} C");
			}
		}

		PollAndSend(spaceConn);
	}
}