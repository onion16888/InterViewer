
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using Debug = System.Diagnostics.Debug;
namespace InterViewer.Droid
{
	[Activity(Label = "DetailActivity", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class DetailActivity : Activity
	{
		PDFDocument pdf;
		Bitmap bitmap;
		int _pageNumber;
		public static Document document { get; set;}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Detail);
			var d = document.Name;
			ImageView iv = FindViewById<ImageView>(Resource.Id.imageView1);

			/*var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			var pdfFilepath = System.IO.Path.Combine(dir, "0200B9.pdf");

			if (!File.Exists(pdfFilepath))
			{
				using (var source = Assets.Open(@"0200B9.pdf"))
				using (var dest = OpenFileOutput("0200B9.pdf", FileCreationMode.WorldReadable | FileCreationMode.WorldWriteable))
				{
					source.CopyTo(dest);
				}
			}*/

			pdf = new PDFDocument(this, document.Name);
			var count = pdf.Count;

			iv.SetImageBitmap(pdf.Images[0]);

			/*botton1.Click += (object sender, EventArgs e) =>
			{
				this.PageNumber--;
				iv.SetImageBitmap(pdf.Images[PageNumber]);

			};

			botton2.Click += delegate
			{
				this.PageNumber++;
				iv.SetImageBitmap(pdf.Images[PageNumber]);

			};*/
		}

		public int PageNumber
		{
			get { return this._pageNumber; }
			set
			{
				if (value >= 0 && value <= pdf.Count - 1)
				{
					_pageNumber = value;
				}
			}
		}
	}
}

