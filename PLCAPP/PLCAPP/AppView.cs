using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace PLCAPP
{
	public class AppView : Application
	{
		public AppView ()
		{
            MainPage = new login();
        }
	}
}