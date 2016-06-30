
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
	
	[Activity(Label = "DetailActivity"//, MainLauncher = true
			  , ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]



	public class DetailActivity : Activity, View.IOnTouchListener
	{

		//private ImageView _imageView;
		/// <summary>
		/// Edit, Add
		/// </summary>
		/// <value>The type of the pdf.</value>
		public static string PDF_Type { get; set; } = "Add";
		public string PDF_RECORD_DIR = string.Empty;
		PDFDocument pdf;
		private bool openPen { get; set; } = false;
		int _pageNumber = 0;
		public static Document Doc { get; set; }
		private float startX, endX = 0;
		private Bitmap drawRectLine;
		private string DocumentPath;

		ScrollView pdfScrollView;
		RelativeLayout pdfContent;
		PencilDrawLine drawLineView;
		RelativeLayout detail_main;
		RelativeLayout pdfRelativeLayout;

		ImageView pdfImageView;

		ImageView cameraImageView;

		#region tool bar
		ImageButton btnPencil;
		ImageButton btnCamera;
		#endregion

		private InterViewerService interviewerservice;
		private IOService ioService;

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
			//Doc = new Document();
			ioService = new IOService();
			interviewerservice = new InterViewerService(ioService);
			DocumentPath = ioService.GetDocumentDirectory();

			Initial();

			ImageView drawImageView = new ImageView(pdfContent.Context);
			drawLineView = new PencilDrawLine(pdfContent.Context);
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
					//drawImageView.SetBackgroundColor(Color.Blue);
					pdfContent.AddView(drawLineView);
					pdfContent.AddView(drawImageView);
				}
				else 
				{
					openPen = false;
					//設置透明底色
					btnPencil.SetBackgroundColor(Color.Transparent);
				    drawRectLine = drawLineView.GetRectBitmap(drawLineView);

					/*drawImageView.SetImageBitmap(drawRectLine);
					drawImageView.SetX(drawLineView.GetRectLeftX());
					drawImageView.SetY(drawLineView.GetRectTopY());
					var drawSaveDirPath = Environment.ExternalStorageDirectory.AbsolutePath;
					var drawLinePNGFilePath = System.IO.Path.Combine(drawSaveDirPath, drawImageFileName);
					var stream = new FileStream(drawLinePNGFilePath, FileMode.Create);
					drawRectLine.Compress(Bitmap.CompressFormat.Png, 100, stream);
					stream.Close();*/

					//Java.IO.File DrawLinePNGFileIO = new Java.IO.File(drawLinePNGFilePath);
					//if (DrawLinePNGFileIO.Exists())
					//{
					//	Bitmap savePNG = BitmapFactory.DecodeFile(drawLinePNGFilePath);
					//	drawImageView.SetImageBitmap(savePNG);
					//	drawImageView.SetBackgroundColor(Color.Transparent);
					//	drawImageView.SetX(drawLineView.GetRectLeftX());
					//	drawImageView.SetY(drawLineView.GetRectTopY());


					//	//drawImageView.SetOnTouchListener(new View.IOnTouchListener () =>{ })
					//	pdfContent.RemoveView(drawLineView);
					//}

					//SettingUIView(AttachmentTypeEnum.Paint, null);
					AddAttachmentAndSaveJsonData(AttachmentTypeEnum.Paint);
				
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


			if (!System.IO.File.Exists(Doc.Reference))
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
				//var count = pdf.Count;

				pdfImageView.SetImageBitmap(pdf.Images[0]);



				//reloadAttachment
				LoadingAttachments();

			}
			//pdf = new PDFDocument(this, pdfFilepath);
			var count = pdf.Count;



			//pdfImageView.SetImageBitmap(pdf.Images[0]);


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
			f		//drawImageView.SetImageBitmap(draw.getcautbitmap());
					Console.WriteLine("11");
				}
			};*/
			//li.RemoveView(FindViewById(Resource.Id.scrollView1));
			pdfImageView.SetOnTouchListener(this);

		}

		private void Initial()
		{
			pdfImageView = FindViewById<ImageView>(Resource.Id.pdfImageView);

			//pdfImageView.LayoutParameters = new ViewGroup.LayoutParams(
			//	ViewGroup.LayoutParams.MatchParent,
			//	ViewGroup.LayoutParams.MatchParent
			//);

			pdfImageView = FindViewById<ImageView>(Resource.Id.pdfImageView);
			pdfScrollView = FindViewById<ScrollView>(Resource.Id.pdfScrollView);
			pdfContent = FindViewById<RelativeLayout>(Resource.Id.pdfContent);
			pdfRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.ly1);

			detail_main = FindViewById<RelativeLayout>(Resource.Id.detail_main);
			btnCamera = FindViewById<ImageButton>(Resource.Id.btnCamera);
			btnPencil = FindViewById<ImageButton>(Resource.Id.btnPencil);
			PDF_RECORD_DIR = PDF_Type == "Add" ? DateTime.Now.Ticks.ToString() : System.IO.Path.GetFileNameWithoutExtension(Doc.Name);
			if (PDF_Type == "Add" && Doc.Name == null)
			{
				Doc.Name = string.Format("{0}.json", PDF_RECORD_DIR);
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
					//startX = e.GetX();
					//path.LineTo(point.X, point.Y);
					break;

				case MotionEventActions.Up:
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

					ClearAllView();
					LoadingAttachments();
					SaveLoadJsonData();

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

			//int height = Resources.DisplayMetrics.HeightPixels;
			int height = 150;
			int width = Math.Abs(75);
			CameraApp.bitmap = CameraApp._file.Path.LoadAndResizeBitmap(width, height);
			if (CameraApp.bitmap != null)
			{
				/*cameraImageView.SetImageBitmap(CameraApp.bitmap);
				cameraImageView.Id = View.GenerateViewId();
				cameraImageView.SetX(Math.Abs(pdfContent.Width / 4));
				cameraImageView.SetY(Math.Abs(pdfContent.Width / 4));
				pdfContent.AddView(cameraImageView);*/

				//must put the call method here!!! when the cameraImageView isn't null
				//SettingUIView(AttachmentTypeEnum.Photo, null);
				AddAttachmentAndSaveJsonData(AttachmentTypeEnum.Photo);

				CameraApp.bitmap = null;
			}
	
			// Dispose of the Java side bitmap.
			GC.Collect();
		}

		#endregion

		#region getSaveImageLocalSystemPath

		private string getSaveImageLocalSystemPath(string IdentifierName, AttachmentTypeEnum savetype, Bitmap image)
		{


			string SAVE_FILE_NAME = string.Format("{0}.{1}", IdentifierName, savetype == AttachmentTypeEnum.Photo ? "jpg" : "png");
			string SYSTEM_FILE_PATH = System.IO.Path.Combine(PDF_RECORD_DIR, SAVE_FILE_NAME);
			var documentsDirectory = System.IO.Path.Combine(DocumentPath, PDF_RECORD_DIR);
			if (!Directory.Exists(documentsDirectory))
			{
				Directory.CreateDirectory(documentsDirectory);
			}

			string saveFilePath = System.IO.Path.Combine(documentsDirectory, SAVE_FILE_NAME);

			SaveBitmapFile(saveFilePath, image);
			if (!System.IO.File.Exists(saveFilePath))
			{
				SYSTEM_FILE_PATH = string.Empty;
			}
			return SYSTEM_FILE_PATH;
		}

		private void SaveBitmapFile(string drawImageFileName, Bitmap image)
		{

			var drawSaveDirPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
			var drawLinePNGFilePath = System.IO.Path.Combine(drawSaveDirPath, drawImageFileName);
			var stream = new FileStream(drawLinePNGFilePath, FileMode.Create);
			image.Compress(Bitmap.CompressFormat.Png, 100, stream);
			stream.Close();
		}

		#endregion

		public void SettingAttachments(Attachment attachment)
		{

			if (Doc.Attachments == null)
			{
				Doc.Attachments = new List<Attachment>();
				Doc.Attachments.Add(attachment);
			}
			else
			{
				var editAttachment = Doc.Attachments.Where(x => x.Name == attachment.Name).SingleOrDefault();
				if (editAttachment == null)
				{
					Doc.Attachments.Add(attachment);
				}
				else
				{
					editAttachment = attachment;
				}
			}

		}

		public void LoadingAttachments()
		{
			if (Doc.Attachments != null)
			{
				List<Attachment> attachments = Doc.Attachments.Where(x => x.PageIndex == PageNumber).ToList();
				foreach (Attachment attachment in attachments)
				{

					switch (attachment.Type)
					{
						case AttachmentTypeEnum.Note:
							SettingUIView(AttachmentTypeEnum.Note, attachment);
							break;
						case AttachmentTypeEnum.Paint:
							SettingUIView(AttachmentTypeEnum.Paint, attachment);
							break;
						case AttachmentTypeEnum.Photo:
							SettingUIView(AttachmentTypeEnum.Photo, attachment);
							break;

					}

				}
			}

		}

		public void SettingUIView(AttachmentTypeEnum type, Attachment attachment = null)
		{
			var rectPoint = new ImageRectPoint();
			//CGRect CGRectFrame = new CGRect();
			if (attachment != null)
			{
				rectPoint.LeftX = attachment.X;
				rectPoint.TopY = attachment.Y;
				rectPoint.Width = attachment.Width;
				rectPoint.Height = attachment.Height;
				//CGRectFrame = new CGRect(attachment.X, attachment.Y, attachment.Width, attachment.Height);
			}
			string identifier = DateTime.Now.Ticks.ToString();
			if (type == AttachmentTypeEnum.Note)
			{
				/*	
					UITextView textview = new UITextView();
					textview.Text = attachment == null ? string.Empty : attachment.Path;
					textview.AccessibilityIdentifier = attachment == null ? identifier : attachment.Name;
					textview.BackgroundColor = UIColor.FromRGB(242, 255, 0);
					textview.Frame = attachment == null ? new CoreGraphics.CGRect(scrollView.Center.X, scrollView.Center.Y, 100, 100) : CGRectFrame;
					//textview.Center = attachment == null ? new CGPoint(scrollView.Center.X, scrollView.Center.Y) : centerPoint;
					textview.Font = UIFont.FromName("Helvetica-Bold", 30f);
					UIPanGestureRecognizer panGesture = SettingUIPanGesture(textview, UIType.UITextView);
					textview.UserInteractionEnabled = true;
					textview.AddGestureRecognizer(panGesture);

					textview.Ended += delegate
					{
						var editAttachment = Doc.Attachments.Where(x => x.Name == textview.AccessibilityIdentifier).SingleOrDefault();
						editAttachment.Path = textview.Text;
						SettingAttachments(editAttachment);
					};
					//NoteList.Add(Identifier, textNote);
					this.scrollView.InsertSubview(textview, 1);

					if (attachment == null)
					{
						Attachment newattachment = new Attachment();
						newattachment.Name = identifier;
						newattachment.PageIndex = PageNumber;
						newattachment.Path = textview.Text;
						newattachment.Type = AttachmentTypeEnum.Note;
						newattachment.Width = textview.Frame.Width;
						newattachment.Height = textview.Frame.Height;
						newattachment.X = textview.Frame.Location.X;
						newattachment.Y = textview.Frame.Location.Y;
						SettingAttachments(newattachment);
					}
                  */
				}

				if (type == AttachmentTypeEnum.Paint || type == AttachmentTypeEnum.Photo)
			{
				string systemPath = string.Empty;
				if (type == AttachmentTypeEnum.Paint && attachment == null)
				{
					//newimage.BackgroundColor = UIColor.Clear;
					systemPath = getSaveImageLocalSystemPath(identifier, AttachmentTypeEnum.Paint, drawRectLine);
					rectPoint.LeftX = drawLineView.GetRectLeftX();
					rectPoint.TopY = drawLineView.GetRectTopY();
					rectPoint.Width = drawLineView.Width;
					rectPoint.Height = drawLineView.Height;
				}
				else if (type == AttachmentTypeEnum.Photo && attachment == null)
				{
					//CameraCapture.IsOpenCamera = false;
					systemPath = getSaveImageLocalSystemPath(identifier, AttachmentTypeEnum.Photo, CameraApp.bitmap);
					rectPoint.LeftX = cameraImageView.GetX()+600;
					rectPoint.TopY = drawLineView.GetY()+ 250;
					rectPoint.Width = 150;
					rectPoint.Height = 75;
				}
				else
				{
					systemPath = attachment.Path;
				}

				string filePath = System.IO.Path.Combine(DocumentPath, attachment == null ? systemPath : attachment.Path);

				Java.IO.File DrawLinePNGFileIO = new Java.IO.File(filePath);
				if (DrawLinePNGFileIO.Exists())
				{
					Bitmap savePNG = BitmapFactory.DecodeFile(filePath);
					var imageView = new ImageView(this);
					imageView.SetImageBitmap(savePNG);
					imageView.SetBackgroundColor(Color.Transparent);
					imageView.SetX((float)rectPoint.LeftX);
					imageView.SetY((float)rectPoint.TopY);
					imageView.SetMaxWidth((int)rectPoint.Width); 
					imageView.SetMaxHeight((int)rectPoint.Height);
					pdfContent.RemoveView(drawLineView);


					//imageView.AccessibilityIdentifier = attachment == null ? identifier : attachment.Name;
					//UIPanGestureRecognizer panGesture = SettingUIPanGesture(imageView, UIType.UIImageView);

					//if (type == AttachmentTypeEnum.Paint && attachment == null)
					//{
					//	imageView.Frame = new CGRect(
					//		path.BoundingBox.Location,
					//		path.BoundingBox.Size);
					//}
					//else
					//{
					//	imageView.Frame = attachment == null ? new CGRect(scrollView.Center.X, scrollView.Center.Y, 225, 225) : CGRectFrame;
					//}

					//imageView.UserInteractionEnabled = true;
					//imageView.AddGestureRecognizer(panGesture);

					pdfRelativeLayout.AddView(imageView);

					if (attachment == null)
					{
						Attachment newattachment = new Attachment();
						newattachment.Name = identifier;
						newattachment.PageIndex = PageNumber;
						newattachment.Path = systemPath;
						newattachment.Type = type;
						newattachment.Width = imageView.Width;
						newattachment.Height = imageView.Height;
						newattachment.X = imageView.GetX();
						newattachment.Y = imageView.GetY();
						SettingAttachments(newattachment);
					}
				}
			}
		}

		public void ClearAllView()
		{
			//remain pdfScrollView
			pdfContent.RemoveViewsInLayout(1, pdfContent.ChildCount-1);

			//remain pdfImageView
			pdfRelativeLayout.RemoveViewsInLayout(1, pdfRelativeLayout.ChildCount-1);
		}

		public void AddAttachmentAndSaveJsonData(AttachmentTypeEnum type)
		{
			SettingUIView(type);
			SaveLoadJsonData();
		}

		public void SaveLoadJsonData()
		{
			interviewerservice.SaveAsJson(Doc);
		}

		public class ImageRectPoint
		{
			public double LeftX { get; set; }
			public double TopY { get; set; }
			public double Width { get; set;}
			public double Height { get; set;}

			public ImageRectPoint()
			{
			}
		}
	}
}

