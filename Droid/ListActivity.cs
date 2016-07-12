
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Java.IO;
using Android.Provider;
using Java.Lang;

using Debug = System.Diagnostics.Debug;

namespace InterViewer.Droid
{
	[Activity(Label = "ListActivity"//,MainLauncher = true 
	          ,ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class ListActivity : Activity
	{
		//大家共用要傳的物件
		public static Document Doc { set; get; }
		//新增Image的辨識碼
		private const int ImagePick = 1000;
		//新增Pdf的辨識碼
		private const int PdfPick = 2000;
		//媒體庫公開目錄
		Android.Net.Uri uri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
		//此App的專用目錄(暫定)
		public string AppDir = Android.OS.Environment.ExternalStorageDirectory + "/Download/InterView";
		//只是一個物件 要存放某個資料夾下的資訊
		List<FileSystemInfo> visibleThings = new List<FileSystemInfo>();
		//右側四個Btn
		Button btnTemplate;
		Button btnDocuments;
		Button btnImages;
		Button btnAdd;
		//Grid
		GridView gridviewShow;
		//轉出縮圖的物件
		PDFDocument Pdf;

		IOService serviceHelper;
		InterViewerService interViewerServiceHelper;
		List<Document> listDocumentTempDoc;

		//List<Document> ReturnIconJson;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.List);
			//StartActivity(typeof(DetailActivity));

			//var sq=System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

			//Directory.CreateDirectory(sq +"/ InterView");
			//var ss = Directory.Exists(sq + "/dog");
			init();
			//檢查專用目錄是否存在,不存在就建立
			this.DirCheck(AppDir);

			List<FileSystemInfo> ReturnIcons = this.FindPngInPath(AppDir+"/Slides", visibleThings);

			List<string> hh=new List<string>();

			var h = from qwe in ReturnIcons select new {qwe.FullName};
			foreach(var v in h)
			{
				hh.Add(v.FullName);
			}
			var cc=hh.Count;

			//預設載入Sliders
			gridviewShow.Adapter = new GridViewAdapter(this, queryFilesName(FindPngInPath(AppDir + "/Slides", visibleThings)));
			CheckButtonIsSelected(btnTemplate);

			DetailActivity.PDF_Type = "Add";

			btnTemplate.Click += (object sender, EventArgs e) =>
			{
				CheckButtonIsSelected(btnTemplate);
				gridviewShow.Adapter = new GridViewAdapter(this, queryFilesName(FindPngInPath(AppDir + "/Slides", visibleThings)));
			};

			btnDocuments.Click += (object sender, EventArgs e) =>
			{
				CheckButtonIsSelected(btnDocuments);

				listDocumentTempDoc = interViewerServiceHelper.GetDocuments();
				var filepaths = from theDoc in listDocumentTempDoc
								select theDoc.Thumbnail;

				gridviewShow.Adapter = new GridViewAdapter(this, filepaths.ToArray());
			};
				
			btnImages.Click+= (object sender, EventArgs e) => 
			{
				var imageIntent = new Intent(Intent.ActionPick, uri);
				imageIntent.SetType("image/png");
				imageIntent.PutExtra(Intent.ExtraAllowMultiple, true);

				StartActivityForResult(Intent.CreateChooser(imageIntent, "選取您要匯入的檔案"), ImagePick);
			};
			btnAdd.Click += (object sender, EventArgs e) => 
			{
				Intent fileintent = new Intent(Intent.ActionGetContent);
				//fileintent.SetType("gagt/sdf");
				fileintent.SetType("application/pdf");
				fileintent.PutExtra(Intent.ExtraAllowMultiple, true);
				fileintent.PutExtra(Intent.ExtraLocalOnly, true);

				Intent destIntent = Intent.CreateChooser(fileintent, "選取Pdf");
				StartActivityForResult(destIntent, PdfPick);
			};
			gridviewShow.ItemClick += (object sender, AdapterView.ItemClickEventArgs args) =>
			{
				//Toast.MakeText(this, ReturnIcons[args.Position].FullName, ToastLength.Long).Show();
				//Intent DetailAc = new Intent(this, typeof(DetailActivity));

				var q = gridviewShow.SelectedItem;

				if (btnTemplate.Selected == true)
				{
					if (System.IO.File.Exists(ReturnIcons[args.Position].FullName.Replace(".png", ".pdf")))
						Doc.Reference = ReturnIcons[args.Position].FullName.Replace(".png", ".pdf");
					else
						Doc.Reference = ReturnIcons[args.Position].FullName;//passing a .png path
					Doc.Thumbnail = ReturnIcons[args.Position].FullName;
				}
				if(btnDocuments.Selected == true)
				{
					var filepaths = from theDoc in listDocumentTempDoc
									where theDoc.Thumbnail == listDocumentTempDoc[args.Position].Thumbnail
									select theDoc;
					interViewerServiceHelper.CopyAttachment(Doc,filepaths.ToArray()[0]);
					if (System.IO.File.Exists(listDocumentTempDoc[args.Position].Reference))
						Doc.Reference = listDocumentTempDoc[args.Position].Reference;
					else
						Doc.Reference = listDocumentTempDoc[args.Position].Thumbnail;//passing a .png path
					Doc.Thumbnail = listDocumentTempDoc[args.Position].Thumbnail;
				}

				DetailActivity.Doc = Doc;
				StartActivity(typeof(DetailActivity));
			};
		}

		void init()
		{
			btnTemplate = FindViewById<Button>(Resource.Id.btnTemplate);
			btnDocuments = FindViewById<Button>(Resource.Id.btnDocuments);
			btnImages = FindViewById<Button>(Resource.Id.btnImages);
			btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
			gridviewShow = FindViewById<GridView>(Resource.Id.gridviewShow);

			serviceHelper = new IOService();
			interViewerServiceHelper = new InterViewerService(serviceHelper);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			string SaveImageDir = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Download/InterView/";
			base.OnActivityResult(requestCode, resultCode, data);

			//如果使用者要選Image 且 是要選Image
			if (resultCode == Result.Ok && requestCode == ImagePick)
			{
				if (data.Data != null)
				{
					//再確認一次Appdir是否存在
					this.DirCheck(SaveImageDir);
					//var imageView = FindViewById<ImageView> (Resource.Id.myImageView);
					//imageView.SetImageURI (data.Data);
					//得到Url物件,送去處理，回傳真實路徑
					string Source = getPathFromUri(this, data.Data);

					//將回傳路徑丟進JAVA.IO.FILE利用NAME得到檔案名稱,組出新路徑跟檔案名稱
					string Des = System.IO.Path.Combine(SaveImageDir+"/Slides", new Java.IO.File(Source).Name);
					if (new Java.IO.File(Des).Exists())
					{
						//搬完檔案之後重新載入Slides
						//this.copy(new Java.IO.File(Source), new Java.IO.File(Des));

						#region 縮圖
						BitmapFactory.Options options = new BitmapFactory.Options();
						options.InPreferredConfig = Bitmap.Config.Argb8888;
						Bitmap bitmap = BitmapFactory.DecodeFile(Source, options);

						double scale = 1f / 5f;

						Bitmap resizedBitmap = Bitmap.CreateScaledBitmap(
							bitmap,
							(Int32)(bitmap.Width * scale),
							(Int32)(bitmap.Height * scale),
							false
						);

						Stream output = new FileStream(Des, FileMode.Create);
						resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, output);
						output.Flush();
						output.Close();
						#endregion

						gridviewShow.Adapter = new GridViewAdapter(this, queryFilesName(FindPngInPath(AppDir + "/Slides", visibleThings)));
					}
					else
					{
						#region 縮圖
						BitmapFactory.Options options = new BitmapFactory.Options();
						options.InPreferredConfig = Bitmap.Config.Argb8888;
						Bitmap bitmap = BitmapFactory.DecodeFile(Source, options);

						double scale = 1f / 4f;

						Bitmap resizedBitmap = Bitmap.CreateScaledBitmap(
							bitmap, 
							(Int32)(bitmap.Width * scale), 
							(Int32)(bitmap.Height * scale), 
							false
						);

						Stream output = new FileStream(Des, FileMode.Create);
						resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, output);
						output.Flush();
						output.Close();
						#endregion

						gridviewShow.Adapter = new GridViewAdapter(this, queryFilesName(FindPngInPath(AppDir + "/Slides", visibleThings)));
					}
				}
				else
				{
					ClipData clipData = data.ClipData;
					int count = clipData.ItemCount;
					if (count > 0)
					{
						Android.Net.Uri[] uris = new Android.Net.Uri[count];
						for (int i = 0; i < count; i++)
						{
							uris[i] = clipData.GetItemAt(i).Uri;
							string Source = GetPathToImage(uris[i]);
							string Des = System.IO.Path.Combine(SaveImageDir, new Java.IO.File(Source).Name);
							if (new Java.IO.File(Des).Exists())
							{
								this.copy(new Java.IO.File(Source), new Java.IO.File(Des));
							}
							this.copy(new Java.IO.File(Source), new Java.IO.File(Des));
						}
					}
				}
			}
			//如果使用者白目不選檔案
			if (resultCode == Result.Canceled)
			{
				//NotThink
			}
			//如果選擇的是 PDf
			if (resultCode == Result.Ok && requestCode == PdfPick)
			{
				string ForPdfDir = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

				var uris = new List<Android.Net.Uri>();
				if (data.Data != null)
				{
					uris.Add(data.Data);
				}
				else 
				{
					for (int i = 0; i < data.ClipData.ItemCount; i++)
					{
						uris.Add(data.ClipData.GetItemAt(i).Uri);
					}
				}

				foreach (var fileUri in uris)
				{
					string Source = getPathFromUri(this, fileUri);

					Java.IO.File FileSou = new Java.IO.File(Source);

					string Des = System.IO.Path.Combine(ForPdfDir, "DownLoad/InterView/Slides/" + FileSou.Name);

					Java.IO.File FileDes = new Java.IO.File(Des);

					this.copy(FileSou, FileDes);
					this.WritePngToDir(Source, Des);
				}
				gridviewShow.Adapter = new GridViewAdapter(this, queryFilesName(FindPngInPath(AppDir + "/Slides", visibleThings)));
			}
		}
		//MaX Add
		public string getPathFromUri(Context context, Android.Net.Uri uri)
		{
			if (uri == null)
			{
				return null;
			}
			// 判斷是否為Android 4.4之後的版本
			bool after44 = (Int32)Build.VERSION.SdkInt >= 19;

			if (after44 && DocumentsContract.IsDocumentUri(context, uri))
			{
				// 如果是Android 4.4之後的版本，而且屬於文件URI
				string authority = uri.Authority;

				// 判斷Authority是否為本地端檔案所使用的
				if ("com.android.externalstorage.documents".Equals(authority))
				{
					// 外部儲存空間
					string docId = DocumentsContract.GetDocumentId(uri);
					string[] divide = docId.Split(':');
					string type = divide[0];

					if ("primary".Equals(type))
					{
						return Android.OS.Environment.ExternalStorageDirectory + "/" + divide[1];
					}
				}
				else if ("com.android.providers.downloads.documents".Equals(authority))
				{
					// 下載目錄
					string docId = DocumentsContract.GetDocumentId(uri);
					Android.Net.Uri downloadUri = ContentUris.WithAppendedId(Android.Net.Uri.Parse("content://downloads/public_downloads"), Long.ParseLong(docId));
					return queryAbsolutePath(context, downloadUri);
				}
				else if ("com.android.providers.media.documents".Equals(authority))
				{
					// 圖片、影音檔案
					string docId = DocumentsContract.GetDocumentId(uri);
					string[] divide = docId.Split(':');
					string type = divide[0];
					Android.Net.Uri mediaUri = null;
					if ("image".Equals(type))
					{
						mediaUri = MediaStore.Images.Media.ExternalContentUri;
					}
					else if ("video".Equals(type))
					{
						mediaUri = MediaStore.Video.Media.ExternalContentUri;
					}
					else if ("audio".Equals(type))
					{
						mediaUri = MediaStore.Audio.Media.ExternalContentUri;
					}
					else {
						return null;
					}
					mediaUri = ContentUris.WithAppendedId(mediaUri, Long.ParseLong(divide[1]));
					return queryAbsolutePath(context, mediaUri);
				}
			}
			else {
				// 如果是一般的URI
				string scheme = uri.Scheme;
				string path = null;
				if ("content".Equals(scheme))
				{
					// 內容URI
					path = queryAbsolutePath(context, uri);
				}
				else if ("file".Equals(scheme))
				{
					// 檔案URI
					path = uri.Path;
				}
				return path;
				//return createFileObjFromPath(path, mustCanRead);
			}
			return null;
		}
		//MAX ADD
		public string queryAbsolutePath(Context context, Android.Net.Uri uri)
		{
			string[] projection = { MediaStore.MediaColumns.Data};
			ICursor cursor = null;
			try
			{
				cursor = context.ContentResolver.Query(uri, projection, null, null, null);
				if (cursor != null && cursor.MoveToFirst())
				{
					int index = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Data);
					return cursor.GetString(index);
				}
			}
			catch (Java.Lang.Exception ex)
			{
				ex.PrintStackTrace();
				if (cursor != null)
				{
					cursor.Close();
				}
			}
			return null;
		}

		//Sid ADD
		void WritePngToDir(string Source, string Des)
		{
			//把PDF初始化被給予檔案路徑
			Pdf = new PDFDocument(this, Source, 0);
			//接收第一張Bitmap
			var BitmapIcon = Pdf.Images[0];
			//把.PDF轉成.PNG 開啟建檔
			var stream = new FileStream(Des.Replace(".pdf", ".png"), FileMode.Create);

			//寫成PNG.100%.關閉
			BitmapIcon.Compress(Bitmap.CompressFormat.Png, 100, stream);
			stream.Close();
		}
		//Sid ADD
		public void copy(Java.IO.File src, Java.IO.File dst)
		{
			InputStream sou = new FileInputStream(src);
			OutputStream des = new FileOutputStream(dst);

			// Transfer bytes from in to out
			byte[] buf = new byte[1024];
			int len;
			while ((len = sou.Read(buf)) > 0)
			{
				des.Write(buf, 0, len);
			}
			sou.Close();
			des.Close();
		}

		private void ShowAlert(string message, EventHandler<Android.Content.DialogClickEventArgs> positiveButtonClickHandle)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			alert.SetTitle(message);
			alert.SetPositiveButton("OK!", positiveButtonClickHandle);
			RunOnUiThread(() =>
			{
				alert.Show();
			});
		}

		//Sid ADD
		private void DirCheck(string AppDir)
		{
			Java.IO.File AppDirCheck = new Java.IO.File(AppDir);
			bool ap=AppDirCheck.Exists();
			bool si = Directory.Exists(AppDir + "/Slides");
			bool dc = Directory.Exists(AppDir + "/Documents");
			if (!AppDirCheck.Exists()||!Directory.Exists(AppDir + "/Slides")||!Directory.Exists(AppDir + "/Documents"))
			{
				AppDirCheck.Mkdirs();
				Directory.CreateDirectory(AppDir + "/Slides");
				Directory.CreateDirectory(AppDir + "/Documents");
			}
		}
		//把Uri拆解成為真實路徑
		//private String GetPathToImage(Android.Net.Uri uri)
		//{
		//	string path = null;
		//	// The projection contains the columns we want to return in our query.
		//	string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
		//	using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
		//	{
		//		if (cursor != null)
		//		{
		//			int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
		//			cursor.MoveToFirst();
		//			path = cursor.GetString(columnIndex);
		//		}
		//	}
		//	return path;
		//}
		//Sid ADD
		private string GetPathToImage(Android.Net.Uri uri)
		{
			string doc_id = "";
			using (var c1 = ContentResolver.Query(uri, null, null, null, null))
			{
				c1.MoveToFirst();
				string document_id = c1.GetString(0);
				doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
			}

			string path = null;

			// The projection contains the columns we want to return in our query.
			string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
			using (var cursor = ManagedQuery(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
			{
				if (cursor == null) return path;
				var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
				cursor.MoveToFirst();
				path = cursor.GetString(columnIndex);
			}
			return path;
		}

		//找到路徑底下的.png
		private List<FileSystemInfo> FindPngInPath(string icoopath, List<FileSystemInfo> visibleThings)
		{
			visibleThings.Clear();
			DirectoryInfo DirInfo = new DirectoryInfo(icoopath);
			foreach (var AllFile in DirInfo.GetFileSystemInfos().Where(item => item.Exists))
			{
				bool IsPngFile = AllFile.Extension.ToLower().EndsWith(".png");
				bool IsJpgFile = AllFile.Extension.ToLower().EndsWith(".jpg");
				System.Console.WriteLine(AllFile.Extension);
				if (IsPngFile || IsJpgFile)
				{
					visibleThings.Add(AllFile);
				}
			}
			return visibleThings;
		}
		/// <summary>
		/// Queries the name of the files.
		/// </summary>
		/// <returns>The files name.</returns>
		/// <param name="FolderName">Folder name. for example,if the source is in "Slides" path folder,set string "Slides" here</param>
		/// Sean ADD
		private string[] queryFilesName(string FolderName)
		{
			//Debug.WriteLine("files:");

			var files = Directory.EnumerateFiles(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).ToString() + @"/InterView/" + FolderName + "/").ToArray();
			foreach (string file in files)
			{
				Debug.WriteLine(file);
			}

			return files;
		}
		//Sid Add
		private string[] queryFilesName(List<FileSystemInfo> visibleThings)
		{
			List<string> hh = new List<string>(); ;

			var h = from qwe in visibleThings select new { qwe.FullName };
			foreach (var v in h)
			{
				hh.Add(v.FullName);
			}

			return hh.ToArray();
		}

		// ?? Add
		private void CheckButtonIsSelected(Button button)
		{
			Button otherBtn = button.Id == Resource.Id.btnTemplate ? btnDocuments : btnTemplate;

			if (otherBtn.Selected)
			{
				otherBtn.Selected = false;
				otherBtn.SetBackgroundResource(Resource.Drawable.sub_command_normal);
			}

			button.Selected = true;
			button.SetBackgroundResource(Resource.Drawable.sub_command_selected);
		}

		//以後有機會測試,套這個Adapter會Error Sid ADD
		public class ImageAdapter : BaseAdapter
		{
			List<FileSystemInfo> _visibleThings;


			//		int[] thumbIds = {
			//			Resource.Drawable.sample_0, Resource.Drawable.sample_1,
			//			Resource.Drawable.sample_2, Resource.Drawable.sample_3,
			//			Resource.Drawable.sample_4, Resource.Drawable.sample_5,
			//			Resource.Drawable.sample_6, Resource.Drawable.sample_7
			//		};
			private Activity _context;

			public ImageAdapter(Activity context, string icoopath, List<FileSystemInfo> visibleThings)
			{
				_context = context;
				_visibleThings = visibleThings;
				var HowManyFile = visibleThings.Count;
			}

			public override int Count
			{
				get { return _visibleThings.Count; }
			}

			public override Java.Lang.Object GetItem(int position)
			{
				return null;
			}

			public override long GetItemId(int position)
			{
				return 0;
			}
			// create a new ImageView for each item referenced by the Adapter
			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				ImageView imageView;

				if (convertView == null)
				{  // if it's not recycled, initialize some attributes
					imageView = new ImageView(_context);
					imageView.LayoutParameters = new GridView.LayoutParams(85, 85);
					imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
					imageView.SetPadding(8, 8, 8, 8);
				}
				else {
					imageView = (ImageView)convertView;
				}
				var filepath = _visibleThings[position];
				Bitmap bmp = BitmapFactory.DecodeFile(filepath.FullName);

				imageView.SetImageBitmap(bmp);
				return imageView;
			}
		}
	}
}

