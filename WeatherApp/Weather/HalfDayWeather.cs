using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace WeatherApp.Weather {
	public class HalfDayWeather {
		private static DateTime startMorning = DateTime.Today + new TimeSpan(5, 0, 0);
		private static DateTime endMorning = DateTime.Today + new TimeSpan(13, 0, 0);
		private static DateTime startEvening = DateTime.Today + new TimeSpan(13, 0, 0);
		private static DateTime endEvening = DateTime.Today + new TimeSpan(19, 0, 0);

		private List<HourlyWeather> today;

		private List<HourlyWeather> morning;
		private List<HourlyWeather> evening;

		private float meanRain;
		public string meanRainStr => $"{meanRain}{Weather.units["precipitation"]}";

		private float meanRainPb;
		public string meanRainPbStr => $"{meanRainPb}{Weather.units["precipitation_probability"]}";

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
				if (meanRainPb > .75 && meanRain > .5) {
					return MainWindow.imageDict["rainy"];
				}
				if (meanCover > .25) {
					return MainWindow.imageDict["cloudy"];
				}

				return MainWindow.imageDict["suny"];
			}
		}

		public HalfDayWeather(IEnumerable<HourlyWeather> data) {
			meanRain = data.Average(h => h.precipitation);
			meanRainPb = data.Average(h => h.rainProbability);
			meanCover = data.Average(h => h.cloudCover);
			meanWindSpd = data.Average(h => h.windSpeeds);
			minTemp = data.Min(h => h.temperature);
			maxTemp = data.Max(h => h.temperature);
		}
	}
}
