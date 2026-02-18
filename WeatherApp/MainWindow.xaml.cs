using FileHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WeatherApp {
	[IgnoreFirst]
	[DelimitedRecord(",")]
	public class City {
		public int id;
		public string name;
		public int state_id;
		public string state_code;
		public string state_nane;
		public int country_id;
		public string country_code;
		public string country_name;
		public float latitude;
		public float longitude;
		public string native;
		public string type;
		public string level;
		public int parent_id;
		public int population;
		public string timezone;
		public string wikiId;
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static CultureInfo cultureInfo = new CultureInfo("en-IE"); // because my pc is french so it causes errors while parsing (the decimal separator is a , in french not a .)

		public static Dictionary<string, ImageSource> imageDict = new Dictionary<string, ImageSource>
		{
			{ "suny", new BitmapImage(new Uri($"Content/suny.jpg", UriKind.Relative))},
			{ "cloudy", new BitmapImage(new Uri($"Content/cloudy.png", UriKind.Relative))},
			{ "rainy", new BitmapImage(new Uri($"Content/rainy.png", UriKind.Relative))},
		};

		public MainWindow() {
			
			var client = new HttpClient();
			var foo = Task.Run(async () => await Weather.getWeather(client)); // runs an async task in a sync function
			foo.Wait();
			Weather weather = foo.Result;

			DataContext = weather;
			weather.addAll();
			weather.addHalfResume();


			InitializeComponent();
		}

		private List<City> loadCSV(string filename) {
			var engine = new FileHelperEngine<City>();
			var cities = engine.ReadFile("cities.csv");

			return cities.ToList();
		}

		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
			e.Handled = true;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				Close();
			}
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			SearchBarTB.Text = (string)((ListBoxItem)((ListBox)e.Source).SelectedValue).Content;
		}
	}

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

		public string temps => $"min: {minTempStr} max: {maxTempStr}";

		public HalfDayWeather(List<HourlyWeather> today, int MorE) {
			this.today = today;
			IEnumerable<HourlyWeather> set = MorE == 0
				? this.today.Where(h => h.time >= startMorning && h.time < endMorning)
				: this.today.Where(h => h.time >= startEvening && h.time <= endEvening);

			meanRain = set.Average(h => h.precipitation);
			meanRainPb = set.Average(h => h.rainProbability);
			meanCover = set.Average(h => h.cloudCover);
			meanWindSpd = set.Average(h => h.windSpeeds);
			minTemp = set.Min(h => h.temperature);
			maxTemp = set.Max(h => h.temperature);
		}
	}

	public class HourlyWeather {
		public DateTime time;

		public string timeString => $"{time.Hour:00}:{time.Minute:00}";


		public string temperatureString => $"{temperature}{Weather.units["temperature_2m"]}";

		public string windString => $"{windSpeeds}{Weather.units["wind_speed_10m"]}";

		public ImageSource image {
			get {
				// throw new Exception();
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

	public class Weather {
		public int length;
		public DateTime[] time;
		public float[] temperature;
		public float[] precipitation;
		public float[] rainProbability;
		public float[] cloudCover;
		public float[] windSpeeds;
		public static Dictionary<string, string> units;

		private ObservableCollection<HourlyWeather> _AllHourlyWeathers = new ObservableCollection<HourlyWeather>();
		public ObservableCollection<HourlyWeather> AllHourlyWeathers => _AllHourlyWeathers;

		public ObservableCollection<HalfDayWeather> halfDayResume { get; private set; } = new ObservableCollection<HalfDayWeather>();

		public async static Task<Weather> getWeather(HttpClient client) {
			string result;

			// Create the HttpContent for the form to be posted.
			var requestContent = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("latitude", "54.2697"),
				new KeyValuePair<string, string>("longitude", "-8.4694"),
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

			return weather;
		}

		public string getTime(int index) {
			if (index < 0 || index >= length)
				return "";

			DateTime date = time[index];
			return $"{date.Hour}:{date.Minute}";
		}

		public void addHourlyWeather(int index) {
			_AllHourlyWeathers.Add(new HourlyWeather(time[index],
				temperature[index],
				precipitation[index],
				rainProbability[index],
				cloudCover[index],
				windSpeeds[index]));
		}

		public void addAll(int start = 0) {
			DateTime now = DateTime.Now;
			DateTime min = now - new TimeSpan(1, 0, 0);

			for (int i = start; i < length; i++) {
				if (time[i] >= min)
					addHourlyWeather(i);
			}
		}

		public void addHalfResume() {
			List<HourlyWeather> today = new List<HourlyWeather>();
			for (int i = 0; i < 24; i++)
				today.Add(_AllHourlyWeathers[i]);

			halfDayResume.Add(new HalfDayWeather(today, 0));
			halfDayResume.Add(new HalfDayWeather(today, 1));
		}
	}
}
