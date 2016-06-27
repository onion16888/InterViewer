using System;
using System.IO;
using System.Linq;
using Android.Views;
using Android.Content;
using Android.App;
using Android.Widget;
using Android.OS;

namespace InterViewer.Droid
{
	public class GridViewAdapter : BaseAdapter
	{
		Context context;
		string[] fileName;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoadFileAndShow.Droid.ImageAdapterNew"/> class.
		/// </summary>
		/// <param name="c">Context</param>
		/// <param name="fileName">String array for full path name strings</param>
		public GridViewAdapter (Context c, string[] fileName)
		{
			context = c;
			this.fileName = fileName;
		}

		public override int Count {
			get { return fileName.Length; }
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return null;
		}

		public override long GetItemId (int position)
		{
			return 0;
		}

		// create a new ImageView for each item referenced by the Adapter
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			ImageView imageView;

			if (convertView == null) 
			{  // if it's not recycled, initialize some attributes
				imageView = new ImageView (context);
				imageView.LayoutParameters = new GridView.LayoutParams (125, 125);
				imageView.SetScaleType (ImageView.ScaleType.CenterCrop);
				imageView.SetPadding (1, 1, 1, 1);
			} 
			else 
			{
				imageView = (ImageView)convertView;
			}
			imageView.SetImageURI (Android.Net.Uri.Parse(fileName[position]));


			return imageView;
		}
	}
}

