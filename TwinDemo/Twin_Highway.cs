using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twin.Connector
{
	public class Twin_Highway_Fake
	{
		public List<ITwinSensor> AllSensors;


		public Twin_Highway_Fake()
		{
			AllSensors = new List<ITwinSensor>()
			{
					new TwinTempSensor("T1-SB-TEMP-1"),
					new TwinTempSensor("T1-SB-TEMP-2"),
					new TwinTempSensor("T1-SB-TEMP-3"),

					new TwinAirSensor("T1-SB-AIR-1"),
					new TwinAirSensor("T1-SB-AIR-2"),
					new TwinAirSensor("T1-SB-AIR-3"),

					new TwinCameraSensor("T1-SB-CAM-1")

            };

			UpdateCurrentData();
		}

		public void SendCommand(string cmd)
		{
			if (cmd == "crisis")
			{
				Random rnd = new Random();
				AllSensors[rnd.Next(0, AllSensors.Count - 1)].Crisis = true;
			}
			if (cmd == "solve")
			{
				foreach (var sensor in AllSensors)
				{
					if (sensor.Crisis)
					{
						sensor.Crisis = false;
						break;
					}
				}
			}

			foreach (var sensor in AllSensors)
			{
				sensor.UpdateData();
			}
		}

		private async Task UpdateCurrentData()
		{
			await Task.Delay(3000);

			foreach (var sensor in AllSensors)
			{
				sensor.UpdateData();
			}

			UpdateCurrentData();
		}
	}

	public interface ITwinSensor
	{
		string Id { get; }
		bool Crisis { get; set; }

		void UpdateData();
	}

	public class TwinTempSensor : ITwinSensor
	{
		public bool Crisis { get; set; }
		public string Id => id;
		private string id;

		public TwinTempSensor(string id)
		{
			this.id = id;
			UpdateData();
		}

		public double CurrTemperatureC;

		double minNormalTemp = 25;
		double maxNormalTemp = 28;

		double minCrisisTemp = 44;
		double maxCrisisTemp = 46;

		public void UpdateData()
		{
			if (Crisis)
				CurrTemperatureC = DataUpdateHelpers.WobbleNumberInRange(CurrTemperatureC, minCrisisTemp, maxCrisisTemp, .2f);
			else
				CurrTemperatureC = DataUpdateHelpers.WobbleNumberInRange(CurrTemperatureC, minNormalTemp, maxNormalTemp, .2f);
		}
	}

	public class TwinAirSensor : ITwinSensor
	{
		public bool Crisis { get; set; }
		public string Id => id;
		private string id;

		public TwinAirSensor(string id)
		{
			this.id = id;
			UpdateData();
		}

		public double CurrCOppm;
		public double CurrNO2ppb;

		double minNormalCO = 2;
		double maxNormalCO = 4;

		double minCrisisCO = 8;
		double maxCrisisCO = 10;

		double minNormalNO2 = 67;
		double maxNormalNO2 = 70;


		public void UpdateData()
		{
			if (Crisis)
				CurrCOppm = DataUpdateHelpers.WobbleNumberInRange(CurrCOppm, minCrisisCO, maxCrisisCO, .2f);
			else
				CurrCOppm = DataUpdateHelpers.WobbleNumberInRange(CurrCOppm, minNormalCO, maxNormalCO, .2f);

			CurrNO2ppb = DataUpdateHelpers.WobbleNumberInRange(CurrNO2ppb, minNormalNO2, maxNormalNO2, .2f);
		}
	}

    public class TwinCameraSensor : ITwinSensor
    {
        public bool Crisis { get; set; }
        public string Id => id;
        private string id;

        public TwinCameraSensor(string id)
        {
            this.id = id;

			CurrentPictureUrl = picUrls[0];

            UpdateData();
        }

		public string CurrentPictureUrl = "";

		public List<string> picUrls = new List<string>()
		{
            "https://drive.google.com/uc?export=download&id=1HTHv1kZw5EtlqhbVeCNm8X5fKY0g2AKk",
            "https://drive.google.com/uc?export=download&id=1wMVCbzDBSrAdqMmgZhRaj4-RUseBTBqE",
            "https://drive.google.com/uc?export=download&id=1qDmhAwUyVNbSJl0l1mIU7mNdVBoc7a_b",
            "https://drive.google.com/uc?export=download&id=1E4wqraSTEmRCaKnQSu4XvkH1zB6mRvd9",
        };

        public int CurrPictureIndex;

		private const int countToWaitToSwapPic = 3;
		private int counter = 0;

        public void UpdateData()
        {
			if(counter < countToWaitToSwapPic)
			{
				counter++;
				return;
			}

			counter = 0;
			CurrPictureIndex++;
			if (CurrPictureIndex >= picUrls.Count)
				CurrPictureIndex = 0;
			CurrentPictureUrl = picUrls[CurrPictureIndex];
        }
    }

    public static class DataUpdateHelpers
	{
		private static Random r = new Random();
		public static double GetRandomNumberInRange(double minNumber, double maxNumber)
		{
			return r.NextDouble() * (maxNumber - minNumber) + minNumber;
		}

		public static double WobbleNumberInRange(double currValue, double minNumber, double maxNumber, double step)
		{
			if (currValue < minNumber || currValue > maxNumber)
				return GetRandomNumberInRange(minNumber, maxNumber);

			var adjustedStep = GetRandomNumberInRange(step - .1, step + .1);
			if (r.Next(0, 2) > 0)
				currValue += adjustedStep;
			else
				currValue -= adjustedStep;

			return Math.Min(maxNumber, Math.Max(minNumber, currValue));
		}
	}
}
