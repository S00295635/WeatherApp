using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.City {
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
}
