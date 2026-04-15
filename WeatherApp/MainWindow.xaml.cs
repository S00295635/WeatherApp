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
using WeatherApp.City;

namespace WeatherApp {


	

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static CultureInfo cultureInfo = new CultureInfo("en-IE"); // because my pc is french so it causes errors while parsing (the decimal separator is a , in french not a .)
		public static City.City sligo = new City.City(57400, "Sligo", "Ireland", 54.25000000f, -8.66667000f);

		public static Dictionary<string, ImageSource> imageDict = new Dictionary<string, ImageSource>
		{
			{ "suny", new BitmapImage(new Uri($"Content/sun.png", UriKind.Relative))},
			{ "cloudy", new BitmapImage(new Uri($"Content/cloud.png", UriKind.Relative))},
			{ "rainy", new BitmapImage(new Uri($"Content/rain.png", UriKind.Relative))},
		};

		private HttpClient client = new HttpClient();

		public MainWindow() {
			InitializeComponent();

			DataContext = GetWeatherOf(sligo);

			SearchBar.ItemsSource = GetCities();
			SearchBar.Placeholder = sligo;
		}

		// Helper Functions
		private IEnumerable<City.City> GetCities() {
			var engine = new FileHelperEngine<RawCity>();
			var rawCities = engine.ReadFile("Content/cities.csv");
			IEnumerable<City.City> cities = rawCities.Select(c => c.toCity());
			return cities.Where(c => c.country_name == "Ireland");
		}

		private Weather.Weather GetWeatherOf(City.City city) {
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
			City.City selected_city = box.SelectedItem as City.City;
			if (selected_city != null) {
				DataContext = GetWeatherOf(selected_city);
				box.Placeholder = selected_city;
				box.SelectedItem = null;
				box.Text = string.Empty;
			}
		}
	}
}
