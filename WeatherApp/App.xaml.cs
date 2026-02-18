using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private bool deactivated = false;

		protected override void OnDeactivated(EventArgs e) {
            if (!deactivated){
                Debug.WriteLine("Closing...");
                
                deactivated = true;
            }

            base.OnDeactivated(e);
        }
	}


}
