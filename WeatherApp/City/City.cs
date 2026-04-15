using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.City {
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
}
