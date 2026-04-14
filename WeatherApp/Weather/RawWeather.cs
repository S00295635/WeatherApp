using System;
using System.Collections.Generic;
using System.Globalization;

namespace WeatherApp.Weather {
	public class RawWeather {
		public float latitude { get; set; }
		public float longitude { get; set; }
		public float generation_time { get; set; }
		public int utc_offset_seconds { get; set; }
		public string timezone { get; set; }
		public string timezone_abbreviation { get; set; }
		public float elevation { get; set; }
		public Dictionary<string, string> hourly_units { get; set; }
		public Dictionary<string, IList<object>> hourly { get; set; }

		public Weather toWeather() {
			Weather weather = new Weather();

			Weather.units = hourly_units;
			weather.time = getDateTimes();
			weather.temperature = getTemperatures();
			weather.precipitation = getPrecipitation();
			weather.rainProbability = getPrecipitationProbability();
			weather.cloudCover = getCloudCover();
			weather.windSpeeds = getWindSpeed();
			weather.length = weather.time.Length;

			return weather;
		}

		private DateTime[] getDateTimes() {
			IList<object> rawData = hourly["time"];
			DateTime[] dateTimes = new DateTime[rawData.Count];
			for (int i = 0; i < rawData.Count; i++) {
				dateTimes[i] = DateTime.Parse($"{rawData[i]}");
			}

			return dateTimes;
		}

		private float[] getTemperatures() {
			IList<object> rawData = hourly["temperature_2m"];
			float[] temps = new float[rawData.Count];
			for (int i = 0; i < rawData.Count; i++) {
				if (!float.TryParse($"{rawData[i]}", NumberStyles.Float, MainWindow.cultureInfo, out temps[i]))
					temps[i] = float.NaN;
			}

			return temps;
		}

		private float[] getPrecipitation() {
			IList<object> rawData = hourly["precipitation"];
			float[] preci = new float[rawData.Count];
			for (int i = 0; i < rawData.Count; i++) {
				if (!float.TryParse($"{rawData[i]}", NumberStyles.Float, MainWindow.cultureInfo, out preci[i]))
					preci[i] = float.NaN;
			}

			return preci;
		}

		private float[] getPrecipitationProbability() {
			IList<object> rawData = hourly["precipitation_probability"];
			float[] probas = new float[rawData.Count];
			for (int i = 0; i < rawData.Count; i++) {
				if (!float.TryParse($"{rawData[i]}", NumberStyles.Float, MainWindow.cultureInfo, out probas[i]))
					probas[i] = float.NaN;
			}

			return probas;
		}

		private float[] getWindSpeed() {
			IList<object> rawData = hourly["wind_speed_10m"];
			float[] windSp = new float[rawData.Count];
			for (int i = 0; i < rawData.Count; i++) {
				if (!float.TryParse($"{rawData[i]}", NumberStyles.Float, MainWindow.cultureInfo, out windSp[i]))
					windSp[i] = float.NaN;
			}

			return windSp;
		}

		private float[] getCloudCover() {
			IList<object> rawData = hourly["cloud_cover"];
			float[] cover = new float[rawData.Count];
			for (int i = 0; i < rawData.Count; i++) {
				if (!float.TryParse($"{rawData[i]}", NumberStyles.Float, MainWindow.cultureInfo, out cover[i]))
					cover[i] = float.NaN;
			}

			return cover;
		}
	}
}
