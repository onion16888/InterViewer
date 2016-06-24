
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
	
	[Activity(Label = "DetailActivity"//, MainLauncher = true
	          , ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class DetailActivity : Activity, View.IOnTouchListener
	{
		PDFDocument pdf;
		Bitmap bitmap;
		int _pageNumber = 0;
		public static Document Doc { get; set; }
		private float startX, endX = 0;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Detail);

			ImageView iv = FindViewById<ImageView>(Resource.Id.imageView1);
			//ScrollView sc = FindViewById<ScrollView>(Resource.Id.scrollView1);
			/*var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			pdfFilepath = System.IO.Path.Combine(dir, "0200B9.pdf");

			if (!File.Exists(pdfFilepath))
			{
				using (var source = Assets.Open(@"0200B9.pdf"))
				using (var dest = OpenFileOutput("0200B9.pdf", FileCreationMode.WorldReadable | FileCreationMode.WorldWriteable))
				{
					source.CopyTo(dest);
				}
			}*/

			if (!File.Exists(Doc.Reference))
			{
				var alertDialog1 = new AlertDialog.Builder(this).Create();
				// 設定Title
				alertDialog1.SetTitle("警告");
				// 內文
				alertDialog1.SetMessage("PDF檔案不存在!");

				//第一顆按鈕
				alertDialog1.SetButton("確認", (sender, args) => Toast.MakeText(this, "請確認PDF檔案位置!", ToastLength.Short).Show());

				alertDialog1.Show();
			}
			else
			{
				pdf = new PDFDocument(this, Doc.Reference);
				var count = pdf.Count;

				iv.SetImageBitmap(pdf.Images[0]);

			}

			iv.SetOnTouchListener(this);
		}


		public bool OnTouch(View v, MotionEvent e)
		{
			switch (e.Action)
			{
				case MotionEventActions.Down:
					Debug.WriteLine("D");
					startX = e.GetX();
					break;
				case MotionEventActions.Move:
					//startX = e.GetX();
					break;

				case MotionEventActions.Cancel:
					Debug.WriteLine("C");    

					endX = e.GetX();
					float diff = startX - endX;
					Debug.WriteLine(diff.ToString());
					//-left to right
					if (startX < endX)
					{
						if (PageNumber == pdf.Count - 1)
						{
							Toast.MakeText(this, "此為最後一頁", ToastLength.Long).Show();
						}

						PageNumber++;
					}
					else {
						if (PageNumber == 0)
						{
							Toast.MakeText(this, "此為第一頁", ToastLength.Long).Show();
						}

						PageNumber--;
					}
					ImageView iv = FindViewById<ImageView>(Resource.Id.imageView1);
					iv.SetImageBitmap(pdf.Images[PageNumber]);

					startX = endX = 0;
					Debug.WriteLine(PageNumber);
					break;

			}

			return true;
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

