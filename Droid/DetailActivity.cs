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
using Android.Provider;
using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using CameraAppDemo;

namespace InterViewer.Droid
{

	[Activity(Label = "DetailActivity"//, MainLauncher = true
			  , ScreenOrientation = ScreenOrientation.Landscape)]



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
		private Bitmap drawRectLineBitmap;
		private string DocumentPath;

		RelativeLayout pdfContent;
		PencilDrawLine drawLineView;
		RelativeLayout pdfRelativeLayout;
		ImageView pdfImageView;
		ImageView cameraImageView;
		Button btnHome;
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

			ioService = new IOService();
			interviewerservice = new InterViewerService(ioService);
			DocumentPath = ioService.GetDocumentDirectory();


			Initial();

			//reloadAttachment
			LoadingAttachments();
		}

		#region 按鈕事件 
		public void ButtonEventHandle()
		{

			//drawLineView = new PencilDrawLine(pdfContent.Context);
			btnPencil.Click += (object sender, EventArgs e) =>
			{
				//ImageView drawImageView = new ImageView(pdfContent.Context);
				//drawImageView.Id = View.GenerateViewId();

				if (!openPen)
				{
					openPen = true;
					//drawImageView = new ImageView(pdfContent.Context);
					btnPencil.SetBackgroundColor(Color.Red);
					drawLineView = new PencilDrawLine(pdfContent.Context);

					pdfContent.AddView(drawLineView);
					//pdfContent.AddView(drawImageView);
				}
				else
				{
					openPen = false;
					//設置透明底色
					btnPencil.SetBackgroundColor(Color.Transparent);
					//取得畫布的線的矩形大小的圖片
					drawRectLineBitmap = drawLineView.GetRectBitmap(drawLineView);
					pdfContent.RemoveView(drawLineView);

					CreateNewUIViewHandle(AttachmentTypeEnum.Paint);

				}

			};

			//回Page1
			btnHome.Click += (object sender, EventArgs e) =>
			{
				StartActivity(typeof(MainActivity));
			};

			#region Camera 
			//判斷手機是否有相機裝置
			if (IsThereAnAppToTakePictures())
			{
				CreateDirectoryForPictures();
				cameraImageView = new ImageView(pdfContent.Context);
				btnCamera.Click += TakeAPicture;

			}
			#endregion
		}

		#endregion

		private void Initial()
		{

			#region 延展pdf的長寬至螢幕大小
			pdfImageView = FindViewById<ImageView>(Resource.Id.pdfImageView);
			Display display = WindowManager.DefaultDisplay;

			Point size = new Point();
			display.GetSize(size);

			int width = size.X;
			int height = size.Y;

			pdfImageView.LayoutParameters = new RelativeLayout.LayoutParams(width, height);
			#endregion

			#region 初始化物件

			pdfContent = FindViewById<RelativeLayout>(Resource.Id.pdfContent);
			pdfRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.ly1);

			btnHome = FindViewById<Button>(Resource.Id.btnHome);
			btnCamera = FindViewById<ImageButton>(Resource.Id.btnCamera);
			btnPencil = FindViewById<ImageButton>(Resource.Id.btnPencil);

			#endregion

			#region 產生存放畫線＆圖片的資料夾名稱

			PDF_RECORD_DIR = PDF_Type == "Add" ? DateTime.Now.Ticks.ToString() : System.IO.Path.GetFileNameWithoutExtension(Doc.Name);
			if (PDF_Type == "Add" && Doc.Name == null)
			{
				Doc.Name = string.Format("{0}.json", PDF_RECORD_DIR);
			}

			#endregion

			#region Page3測試用讀PDF
			/*
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
			*/
			#endregion

			#region pdfimageview讀取pdf轉成的image
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

				pdfImageView.SetImageBitmap(pdf.Images[0]);
			}

			#endregion

			//按鈕事件處理
			ButtonEventHandle();

			//新增pdf的Touch事件
			pdfImageView.SetOnTouchListener(this);
		}

		public bool OnTouch(View v, MotionEvent e)
		{
			switch (e.Action)
			{
				case MotionEventActions.Down:
					startX = e.GetX();
					break;
				case MotionEventActions.Move:

					break;

				case MotionEventActions.Up:
				case MotionEventActions.Cancel:

					endX = e.GetX();
					float diff = startX - endX;
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
					//翻頁清除頁面所有物件
					ClearAllView();
					//讀取記憶體中的物件
					LoadingAttachments();
					//儲存json
					SaveJsonData();
					break;
			}

			return true;
		}

		#region 相機事件處理

		private void CreateDirectoryForPictures()
		{
			//save to phone local picture dir
			CameraApp._dir = new Java.IO.File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "CameraPictures");
			if (!CameraApp._dir.Exists())
			{
				CameraApp._dir.Mkdirs();
			}
		}

		/// <summary>
		/// 判斷裝置是否有相機
		/// </summary>
		/// <returns>The there an app to take pictures.</returns>
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

				CreateNewUIViewHandle(AttachmentTypeEnum.Photo);

				CameraApp.bitmap = null;
			}

			// Dispose of the Java side bitmap.
			GC.Collect();
		}

		#endregion

		#region 資料儲存處理

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

			saveBitmapFile(saveFilePath, image);
			if (!System.IO.File.Exists(saveFilePath))
			{
				SYSTEM_FILE_PATH = string.Empty;
			}
			return SYSTEM_FILE_PATH;
		}

		private void saveBitmapFile(string drawImageFileName, Bitmap image)
		{

			var drawSaveDirPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
			var drawLinePNGFilePath = System.IO.Path.Combine(drawSaveDirPath, drawImageFileName);
			var stream = new FileStream(drawLinePNGFilePath, FileMode.Create);
			image.Compress(Bitmap.CompressFormat.Png, 100, stream);
			stream.Close();
		}

		#endregion


		public void SaveJsonData()
		{
			interviewerservice.SaveAsJson(Doc);
		}

		public void CreateNewUIViewHandle(AttachmentTypeEnum type)
		{
			//建立新的附件物件
			Attachment newAttachment = GetNewAttachment(type);
			//根據Attachment物件，建立UIView物件
			CreateUIView(newAttachment);
			//編輯Attachment物件
			EditDocumentAttachments(newAttachment);
			//儲存至Json檔
			SaveJsonData();
		}

		#endregion

		#region Attachments物件處理

		public void EditDocumentAttachments(Attachment attachment)
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
					CreateUIView(attachment);
				}
			}
		}

		#endregion

		/// <summary>
		/// 新增新的附件元素
		/// </summary>
		/// <returns>The new attachment.</returns>
		/// <param name="type">Type.</param>
		public Attachment GetNewAttachment(AttachmentTypeEnum type)
		{
			string identifier = DateTime.Now.Ticks.ToString();
			var rectPoint = new ImageRectPoint();
			string systemPath = string.Empty;
			if (type == AttachmentTypeEnum.Note)
			{

			}
			else
			{
				systemPath = string.Empty;
				Bitmap saveBitmap = type == AttachmentTypeEnum.Paint ? drawRectLineBitmap : CameraApp.bitmap;
				systemPath = getSaveImageLocalSystemPath(identifier, type, saveBitmap);
				rectPoint.LeftX = type == AttachmentTypeEnum.Paint ? drawLineView.GetRectLeftX() : cameraImageView.GetX() + 600;
				rectPoint.TopY = type == AttachmentTypeEnum.Paint ? drawLineView.GetRectTopY() : drawLineView.GetY() + 250;
				rectPoint.Width = type == AttachmentTypeEnum.Paint ? drawLineView.Width : 150;
				rectPoint.Height = type == AttachmentTypeEnum.Paint ? drawLineView.Height : 75;
				string filePath = System.IO.Path.Combine(DocumentPath, systemPath);

			}

			Attachment newAttachment = new Attachment();
			newAttachment.Name = identifier;
			newAttachment.PageIndex = PageNumber;
			newAttachment.Note = string.Empty;
			newAttachment.Type = type;
			newAttachment.Path = systemPath;
			newAttachment.Width = (int)rectPoint.Width;
			newAttachment.Height = (int)rectPoint.Height;
			newAttachment.X = (float)rectPoint.LeftX;
			newAttachment.Y = (float)rectPoint.TopY;

			return newAttachment;
		}

		#region UIView實作

		public void CreateUIView(Attachment attachment)
		{
			var rectPoint = new ImageRectPoint();
			rectPoint.LeftX = attachment.X;
			rectPoint.TopY = attachment.Y;
			rectPoint.Width = attachment.Width;
			rectPoint.Height = attachment.Height;

			string identifier = DateTime.Now.Ticks.ToString();
			if (attachment.Type == AttachmentTypeEnum.Note)
			{

			}
			else
			{

				string filePath = System.IO.Path.Combine(DocumentPath, attachment.Path);

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

					pdfRelativeLayout.AddView(imageView);

				}
			}
		}

		public void ClearAllView()
		{
			//remain pdfScrollView
			pdfContent.RemoveViewsInLayout(1, pdfContent.ChildCount - 1);

			//remain pdfImageView
			pdfRelativeLayout.RemoveViewsInLayout(1, pdfRelativeLayout.ChildCount - 1);
		}

		#endregion

		public class ImageRectPoint
		{
			public double LeftX { get; set; }
			public double TopY { get; set; }
			public double Width { get; set; }
			public double Height { get; set; }
		}
	}
}

