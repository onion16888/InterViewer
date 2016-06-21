
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

namespace InterViewer.Droid
{
	[Activity(Label = "ListActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class ListActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.List);

		}
	}
}

