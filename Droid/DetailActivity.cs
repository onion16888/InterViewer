
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Debug = System.Diagnostics.Debug;
namespace InterViewer.Droid
{
	[Activity(Label = "DetailActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class DetailActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Detail);
		}
	}
}

