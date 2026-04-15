using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace WeatherApp.Weather {
	public class HalfDayWeather {
		private float sumRain;
		public string rainStr => $"{sumRain:.1}{Weather.units["precipitation"]}";

		private float meanRainPb;
		public string meanRainPbStr => $"{meanRainPb:.0}{Weather.units["precipitation_probability"]}";

		private float meanCover;
		public string meanCoverStr => $"{meanCover}{Weather.units["cloud_cover"]}";

		private float meanWindSpd;
		public string meanWindSpdStr => $"{meanWindSpd}{Weather.units["wind_speed_10m"]}";

		private float minTemp;
		public string minTempStr => $"min: {minTemp}{Weather.units["temperature_2m"]}";

		private float maxTemp;
		public string maxTempStr => $"max: {maxTemp}{Weather.units["temperature_2m"]}";

		public ImageSource image {
			get {
				if (meanRainPb > .75 && sumRain > .5) {
					return MainWindow.imageDict["rainy"];
				}
				if (meanCover > .25) {
					return MainWindow.imageDict["cloudy"];
				}

				return MainWindow.imageDict["suny"];
			}
		}

		public HalfDayWeather(IEnumerable<HourlyWeather> data) {
			sumRain = data.Sum(h => h.precipitation);
			meanRainPb = data.Average(h => h.rainProbability);
			meanCover = data.Average(h => h.cloudCover);
			meanWindSpd = data.Average(h => h.windSpeeds);
			minTemp = data.Min(h => h.temperature);
			maxTemp = data.Max(h => h.temperature);
		}
	}
}
