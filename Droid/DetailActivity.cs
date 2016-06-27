
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Debug = System.Diagnostics.Debug;


using Android.Provider;
using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using CameraAppDemo;

namespace InterViewer.Droid
{
	[Activity(Label = "DetailActivity", MainLauncher = true
			  , ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]



	public class DetailActivity : Activity, View.IOnTouchListener
	{

		//private ImageView _imageView;
		/// <summary>
		/// Edit, Add
		/// </summary>
		/// <value>The type of the pdf.</value>
		public string PDF_Type { get; set; } = "Add";
		public string PDF_RECORD_DIR = string.Empty;
		PDFDocument pdf;
		private bool openPen { get; set; } = false;
		int _pageNumber = 0;
		public static Document document { get; set; }
		private float startX, endX = 0;

		ScrollView pdfScrollView;
		RelativeLayout pdfContent;
		PencilDrawLine drawLineView;
		RelativeLayout detail_main;

		ImageView pdfImageView;

		ImageView cameraImageView;

		#region tool bar
		ImageButton btnPencil;
		ImageButton btnCamera;
		#endregion



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

		public static class CameraApp
		{
			public static Java.IO.File _file;
			public static Java.IO.File _dir;
			public static Bitmap bitmap;
		}


		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Detail);

			//test 
			document = new Document();

			Initial();


			ImageView drawImageView = new ImageView(pdfContent.Context);
			PencilDrawLine drawLineView = new PencilDrawLine(pdfContent.Context);
			btnPencil.Click += (object sender, EventArgs e) =>
			{
				
				drawImageView.Id = View.GenerateViewId();
				string drawImageFileName = string.Format("{0}.png", DateTime.Now.Ticks);
				Debug.WriteLine(drawImageView.Id.ToString());
				if (!openPen)
				{
					openPen = true;
					drawImageView = new ImageView(pdfContent.Context);
					btnPencil.SetBackgroundColor(Color.Red);
					drawLineView = new PencilDrawLine(pdfContent.Context);
					drawImageView.SetBackgroundColor(Color.Blue);
					pdfContent.AddView(drawLineView);
					pdfContent.AddView(drawImageView);
				}
				else 
				{
					openPen = false;
					//設置透明底色
					btnPencil.SetBackgroundColor(Color.Transparent);
				    Bitmap drawRectLine = drawLineView.GetRectBitmap(drawLineView);

					drawImageView.SetImageBitmap(drawRectLine);
					drawImageView.SetX(drawLineView.GetRectLeftX());
					drawImageView.SetY(drawLineView.GetRectTopY());
					var drawSaveDirPath = Environment.ExternalStorageDirectory.AbsolutePath;
					var drawLinePNGFilePath = System.IO.Path.Combine(drawSaveDirPath, drawImageFileName);
					var stream = new FileStream(drawLinePNGFilePath, FileMode.Create);
					drawRectLine.Compress(Bitmap.CompressFormat.Png, 100, stream);
					stream.Close();

					Java.IO.File DrawLinePNGFileIO = new Java.IO.File(drawLinePNGFilePath);
					if (DrawLinePNGFileIO.Exists())
					{
						Bitmap savePNG = BitmapFactory.DecodeFile(drawLinePNGFilePath);
						drawImageView.SetImageBitmap(savePNG);
						drawImageView.SetBackgroundColor(Color.Yellow);
						drawImageView.SetX(drawLineView.GetRectLeftX());
						drawImageView.SetY(drawLineView.GetRectTopY());


						//drawImageView.SetOnTouchListener(new View.IOnTouchListener () =>{ })
						pdfContent.RemoveView(drawLineView);
					}
				
				}
			
			};


			var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			string pdfFilepath = System.IO.Path.Combine(dir, "0200B9.pdf");

			if (!System.IO.File.Exists(pdfFilepath))
			{
				using (var source = Assets.Open(@"0200B9.pdf"))
				using (var dest = OpenFileOutput("0200B9.pdf", FileCreationMode.WorldReadable | FileCreationMode.WorldWriteable))
				{
					source.CopyTo(dest);
				}
			}

			/*if (!File.Exists(document.Reference))
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
				pdf = new PDFDocument(this, document.Reference);
				var count = pdf.Count;

				iv.SetImageBitmap(pdf.Images[0]);

			}*/
			pdf = new PDFDocument(this, pdfFilepath);
			var count = pdf.Count;



			pdfImageView.SetImageBitmap(pdf.Images[0]);


			#region Camera 
			if (IsThereAnAppToTakePictures())
			{
				CreateDirectoryForPictures();
				cameraImageView = new ImageView(pdfContent.Context);
				btnCamera.Click += TakeAPicture;

			}
			#endregion

			//drawImageView.SetOnTouchListener(this);
			/*drawImageView.Touch += (object sender, View.TouchEventArgs e) =>
			{

				if (e.Event.Action == MotionEventActions.Down)
				{
					//drawImageView.SetImageBitmap(draw.getcautbitmap());
					Console.WriteLine("11");
				}
			};*/
			//li.RemoveView(FindViewById(Resource.Id.scrollView1));
			//iv.SetOnTouchListener(this);

		}

		private void Initial()
		{
			pdfImageView = FindViewById<ImageView>(Resource.Id.pdfImageView);
			pdfScrollView = FindViewById<ScrollView>(Resource.Id.pdfScrollView);
			pdfContent = FindViewById<RelativeLayout>(Resource.Id.pdfContent);

			detail_main = FindViewById<RelativeLayout>(Resource.Id.detail_main);
			btnCamera = FindViewById<ImageButton>(Resource.Id.btnCamera);
			btnPencil = FindViewById<ImageButton>(Resource.Id.btnPencil);
			PDF_RECORD_DIR = PDF_Type == "Add" ? DateTime.Now.Ticks.ToString() : System.IO.Path.GetFileNameWithoutExtension(document.Name);
			if (PDF_Type == "Add")
			{
				document.Name = string.Format("{0}.json", PDF_RECORD_DIR);
			}

		}

		public bool OnTouch(View v, MotionEvent e)
		{
			Point point = new Point((Int32)e.GetX(), (Int32)e.GetY());
			switch (e.Action)
			{
				case MotionEventActions.Down:
					Debug.WriteLine("D");
					startX = e.GetX();
					break;
				case MotionEventActions.Move:
					startX = e.GetX();
					//path.LineTo(point.X, point.Y);
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
						   //Toast.MakeText(this, "此為最後一頁", ToastLength.Long).Show();
						}

						PageNumber++;
					}
					else {
						if (PageNumber == 0)
						{
						//	Toast.MakeText(this, "此為第一頁", ToastLength.Long).Show();
						}

						PageNumber--;
					}

					pdfImageView.SetImageBitmap(pdf.Images[PageNumber]);

					startX = endX = 0;
					Debug.WriteLine(PageNumber);

					break;

			}

			return true;
		}

		#region Camera Take Pictures Processing

		private void CreateDirectoryForPictures()
		{
			//save to phone local picture dir
			CameraApp._dir = new Java.IO.File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "CameraPictures");
			if (!CameraApp._dir.Exists())
			{
				CameraApp._dir.Mkdirs();
			}
		}

		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities =
				PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		private void TakeAPicture(object sender, EventArgs eventArgs)
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);

			CameraApp._file = new Java.IO.File(CameraApp._dir, String.Format("{0}.jpg", DateTime.Now.Ticks));

			intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(CameraApp._file));

			StartActivityForResult(intent, 0);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile(CameraApp._file);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);

			int height = Resources.DisplayMetrics.HeightPixels;

			int width = Math.Abs(pdfContent.Height / 3);
			CameraApp.bitmap = CameraApp._file.Path.LoadAndResizeBitmap(width, height);
			if (CameraApp.bitmap != null)
			{
				cameraImageView.SetImageBitmap(CameraApp.bitmap);
				cameraImageView.Id = View.GenerateViewId();
				cameraImageView.SetX(Math.Abs(pdfContent.Width / 4));
				cameraImageView.SetY(Math.Abs(pdfContent.Width / 4));
				pdfContent.AddView(cameraImageView);
				CameraApp.bitmap = null;
			}

			// Dispose of the Java side bitmap.
			GC.Collect();
		}

		#endregion


	}
}

