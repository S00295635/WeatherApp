using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.Security.RightsManagement;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var client = new HttpClient();
            var foo = Task.Run(async () => await Weather.getWeather(client));
            foo.Wait();
            Weather weather = foo.Result;

            H1Time.Text = weather.temperature[0].ToString();
        }
    }

    public class RawWeather
    {
        public float latitude {  get; set; }
        public float longitude { get; set; }
        public float generation_time { get; set; }
        public int utc_offset_seconds { get; set; }
        public string timezone { get; set; }
        public string timezone_abbreviation { get; set; }
        public float elevation { get; set; }
        public Dictionary<string, string> hourly_units { get; set; }
        public Dictionary<string, IList<object>> hourly {  get; set; }

        public Weather toWeather()
        {
            Weather weather = new Weather();

            weather.units = hourly_units;
            weather.time = getDateTimes();
            weather.temperature = getTemperatures();
            weather.precipitation = getPrecipitation();
            weather.rainProbability = getPrecipitationProbability();
            weather.cloudCover = getCloudCover();
            weather.windSpeeds = getWindSpeed();
            weather.length = weather.time.Length;

            return weather;
        }

        private DateTime[] getDateTimes()
        {
            IList<object> rawData = hourly["time"];
            DateTime[] dateTimes = new DateTime[rawData.Count];
            for (int i = 0; i < rawData.Count; i++)
            {
                dateTimes[i] = DateTime.Parse($"{rawData[i]}");
            }

            return dateTimes;
        }

        private float[] getTemperatures()
        {
            IList<object> rawData = hourly["temperature_2m"];
            float[] temps = new float[rawData.Count];
            for (int i = 0; i < rawData.Count; i++)
            {
                temps[i] = float.Parse($"{rawData[i]}");
            }

            return temps;
        }

        private float[] getPrecipitation()
        {
            IList<object> rawData = hourly["precipitation"];
            float[] preci = new float[rawData.Count];
            for (int i = 0; i < rawData.Count; i++)
            {
                preci[i] = float.Parse($"{rawData[i]}");
            }

            return preci;
        }

        private float[] getPrecipitationProbability()
        {
            IList<object> rawData = hourly["precipitation_probability"];
            float[] probas = new float[rawData.Count];
            for (int i = 0; i < rawData.Count; i++)
            {
                probas[i] = float.Parse($"{rawData[i]}");
            }

            return probas;
        }

        private float[] getWindSpeed()
        {
            IList<object> rawData = hourly["wind_speed_10m"];
            float[] windSp = new float[rawData.Count];
            for (int i = 0; i < rawData.Count; i++)
            {
                windSp[i] = float.Parse($"{rawData[i]}");
            }

            return windSp;
        }

        private float[] getCloudCover()
        {
            IList<object> rawData = hourly["cloud_cover"];
            float[] cover = new float[rawData.Count];
            for (int i = 0; i < rawData.Count; i++)
            {
                cover[i] = float.Parse($"{rawData[i]}");
            }

            return cover;
        }
    }

    public class Weather
    {
        public int length;
        public DateTime[] time;
        public float[] temperature;
        public float[] precipitation;
        public float[] rainProbability;
        public float[] cloudCover;
        public float[] windSpeeds;
        public Dictionary<string, string> units;

        public async static Task<Weather> getWeather(HttpClient client)
        {
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
            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            {
                // Write the output.
                result = await reader.ReadToEndAsync();
            }

            Debug.WriteLine(result);
            RawWeather rawWeather = JsonSerializer.Deserialize<RawWeather>(result);
            Weather weather = rawWeather.toWeather();
            Debug.WriteLine("Read Content.");

            return weather;
        }

        public string getTime(int index)
        {
            DateTime date = time[0];
            return $"{date.Hour}:{date.Minute}";
        }
    }
}
