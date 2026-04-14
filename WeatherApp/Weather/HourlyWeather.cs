using System;
using System.Windows.Media;

namespace WeatherApp.Weather {
	public class HourlyWeather {
		public DateTime time;
		public int opacity => time.Hour == 0 && time.Minute == 0 ? 100 : 0;
		public string timeString => $"{time.Hour:00}h{time.Minute:00}";

		public string temperatureString => $"{temperature}{Weather.units["temperature_2m"]}";

		public string windString => $"{windSpeeds}{Weather.units["wind_speed_10m"]}";

		public ImageSource image {
			get {
				if (rainProbability > .75 && precipitation > .5) {
					return MainWindow.imageDict["rainy"];
				}
				if (cloudCover > .25) {
					return MainWindow.imageDict["cloudy"];
				}

				return MainWindow.imageDict["suny"];
			}
		}

		public float temperature;
		public float precipitation;
		public float rainProbability;
		public float cloudCover;
		public float windSpeeds;

		public HourlyWeather(DateTime time,
			float temperature,
			float precipitation,
			float rainProbability,
			float cloudCover,
			float windSpeeds) {
			this.time = time;
			this.temperature = temperature;
			this.precipitation = precipitation;
			this.rainProbability = rainProbability;
			this.cloudCover = cloudCover;
			this.windSpeeds = windSpeeds;
		}
	}
}
