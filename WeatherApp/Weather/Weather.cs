using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherApp.Weather {
	public class Weather {
		public int length;
		public DateTime[] time;
		public float[] temperature;
		public float[] precipitation;
		public float[] rainProbability;
		public float[] cloudCover;
		public float[] windSpeeds;
		public static Dictionary<string, string> units;

		private string cityName;

		private ObservableCollection<HourlyWeather> _AllHourlyWeathers = new ObservableCollection<HourlyWeather>();
		
		// exposed fields
		public ObservableCollection<HourlyWeather> AllHourlyWeathers => _AllHourlyWeathers;
		public ObservableCollection<HalfDayWeather> todayResume { get; private set; } = new ObservableCollection<HalfDayWeather>();
		public ObservableCollection<HalfDayWeather> tomorrowResume { get; private set; } = new ObservableCollection<HalfDayWeather>();
		public ObservableCollection<HalfDayWeather> afterTomorrowResume { get; private set; } = new ObservableCollection<HalfDayWeather>();
		public string title => $"Weather for {cityName}. Data from open-meteo.com."; 

		public async static Task<Weather> getWeather(HttpClient client, City.City city) {
			string result;

			// Create the HttpContent for the form to be posted.
			var requestContent = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("latitude", city.latitude.ToString(MainWindow.cultureInfo)),
				new KeyValuePair<string, string>("longitude", city.longitude.ToString(MainWindow.cultureInfo)),
				new KeyValuePair<string, string>("hourly", "temperature_2m,precipitation,precipitation_probability,cloud_cover,wind_speed_10m"),
				new KeyValuePair<string, string>("format", "json"),
			});

			// Get the response.
			HttpResponseMessage response = await client.PostAsync(
				"https://api.open-meteo.com/v1/forecast",
				requestContent);

			// Get the response content.
			HttpContent responseContent = response.Content;
			Debug.WriteLine("Reading Content...");
			// Get the stream of the content.
			using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync())) {
				// Write the output.
				result = await reader.ReadToEndAsync();
			}

			Debug.WriteLine(result);
			RawWeather rawWeather = JsonSerializer.Deserialize<RawWeather>(result);
			Weather weather = rawWeather.toWeather();
			Debug.WriteLine("Read Content.");

			weather.cityName = city.full_name;
			weather.addAll();
			weather.addHalfResume();
			weather.trimFarFuture();
			weather.removePast();

			return weather;
		}

		private void addHourlyWeather(int index) {
			_AllHourlyWeathers.Add(new HourlyWeather(time[index],
				temperature[index],
				precipitation[index],
				rainProbability[index],
				cloudCover[index],
				windSpeeds[index]));
		}

		public void addAll() {
			for (int i = 0; i < length; i++) {
				addHourlyWeather(i);
			}
		}

		public void removePast() {
			DateTime min = DateTime.Now - new TimeSpan(1, 0, 0);
			for (int i = 0; i < _AllHourlyWeathers.Count; i++) {
				HourlyWeather w = _AllHourlyWeathers[i];
				if (w.time < min) {
					_AllHourlyWeathers.RemoveAt(i);
					i--;
				} else {
					break;
				}
			}
		}

		public void trimFarFuture() {
			for (int i = 72; i < _AllHourlyWeathers.Count; i++) {
				HourlyWeather w = _AllHourlyWeathers[i];
				if (w.time.Hour % 6 != 0) {
					_AllHourlyWeathers.RemoveAt(i);
					i--;
				}
			}
		}

		public void addHalfResume() {
			var today = new List<HourlyWeather>();
			var tomorrow = new List<HourlyWeather>();
			var afterTomorrow = new List<HourlyWeather>();

			today.AddRange(_AllHourlyWeathers.Take(24));
			tomorrow.AddRange(_AllHourlyWeathers.Skip(24).Take(24));
			afterTomorrow.AddRange(_AllHourlyWeathers.Skip(48).Take(24));

			todayResume.Add(new HalfDayWeather(today.Skip(5).Take(8)));
			todayResume.Add(new HalfDayWeather(today.Skip(13).Take(6)));

			tomorrowResume.Add(new HalfDayWeather(tomorrow.Skip(5).Take(8)));
			tomorrowResume.Add(new HalfDayWeather(tomorrow.Skip(13).Take(6)));

			afterTomorrowResume.Add(new HalfDayWeather(afterTomorrow.Skip(5).Take(8)));
			afterTomorrowResume.Add(new HalfDayWeather(afterTomorrow.Skip(13).Take(6)));
		}
	}
}
