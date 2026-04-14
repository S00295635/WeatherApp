using FileHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WeatherApp.Weather;

namespace WeatherApp {
	[IgnoreFirst]
	[DelimitedRecord(",")]
	public class RawCity {
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
		public int? parent_id;
		public int? population;
		public string timezone;
		public string wikiId;

		public City toCity() {
			return new City(id, name, country_name, latitude, longitude);
		}
	}

	public class City {
		public int id;
		public string name;
		public string country_name;
		public string full_name => $"{name}, {country_name}";
		public float latitude;
		public float longitude;

		public City(int id, string name, string country_name, float latitude, float longitude) {
			this.id = id;
			this.name = name;
			this.country_name = country_name;
			this.latitude = latitude;
			this.longitude = longitude;
		}

		public override string ToString() => full_name;
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static CultureInfo cultureInfo = new CultureInfo("en-IE"); // because my pc is french so it causes errors while parsing (the decimal separator is a , in french not a .)
		public static City sligo = new City(57400, "Sligo", "Ireland", 54.25000000f, -8.66667000f);

		public static Dictionary<string, ImageSource> imageDict = new Dictionary<string, ImageSource>
		{
			{ "suny", new BitmapImage(new Uri($"Content/sunny.png", UriKind.Relative))},
			{ "cloudy", new BitmapImage(new Uri($"Content/cloudy.png", UriKind.Relative))},
			{ "rainy", new BitmapImage(new Uri($"Content/rainy.png", UriKind.Relative))},
		};

		private HttpClient client = new HttpClient();

		public MainWindow() {
			InitializeComponent();

			DataContext = GetWeatherOf(sligo);

			SearchBar.ItemsSource = GetCities();
		}

		// Helper Functions
		private IEnumerable<City> GetCities() {
			var engine = new FileHelperEngine<RawCity>();
			var rawCities = engine.ReadFile("Content/cities.csv");
			IEnumerable<City> cities = rawCities.Select(c => c.toCity());
			return cities.Where(c => c.country_name != "Ireland");
		}

		private Weather.Weather GetWeatherOf(City city) {
			var task = Task.Run(async () => await Weather.Weather.getWeather(client, city)); // runs an async task in a sync function
			task.Wait();
			return task.Result;
		}

		// Events
		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
			e.Handled = true;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				App.Current.Shutdown();
			}
		}

		private void SearchBar_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoCompleteBox box = (AutoCompleteBox)sender;
			City selected_city = box.SelectedItem as City;
			if (selected_city != null) {
				DataContext = GetWeatherOf(selected_city);
			}
		}
	}


}
