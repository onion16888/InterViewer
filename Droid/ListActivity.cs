
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace InterViewer.Droid
{
	[Activity(Label = "ListActivity",MainLauncher = true ,ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class ListActivity : Activity
	{
		private const int ImagePick = 1000;
		private const int PdfPick = 2000;
		Android.Net.Uri uri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
		//public string icon=Android.OS.Environment.ExternalStorageDirectory+"/Download/Template";
		public string IconPath = Android.OS.Environment.ExternalStorageDirectory + "/Download/InterView/Pdf";
		List<FileSystemInfo> visibleThings = new List<FileSystemInfo>();
		Button btnTemplate;
		Button btnDocuments;
		Button btnImages;
		Button btnAdd;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.List);

			init();

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

				Intent destIntent = Intent.CreateChooser(fileintent, "選取Pdf");
				StartActivityForResult(destIntent, PdfPick);
			};
		}

		void init()
		{
			btnTemplate = FindViewById<Button>(Resource.Id.btnTemplate);
			btnDocuments = FindViewById<Button>(Resource.Id.btnDocuments);
			btnImages = FindViewById<Button>(Resource.Id.btnImages);
			btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
		}
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{

			string publicDir = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Download/";
			base.OnActivityResult(requestCode, resultCode, data);

			//如果使用者要選Image
			if (resultCode == Result.Ok && requestCode == ImagePick)
			{
				if (data.Data != null)
				{
					//var imageView = FindViewById<ImageView> (Resource.Id.myImageView);
					//imageView.SetImageURI (data.Data);
					string Source = GetPathToImage(data.Data);

					string Des = System.IO.Path.Combine(publicDir, new Java.IO.File(Source).Name);
					if (new Java.IO.File(Des).Exists())
					{
						this.copy(new Java.IO.File(Source), new Java.IO.File(Des));
					}
					this.copy(new Java.IO.File(Source), new Java.IO.File(Des));
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
							string Des = System.IO.Path.Combine(publicDir, new Java.IO.File(Source).Name);
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
				if (data.Data != null)
				{
					var SourcePath = System.Net.WebUtility.UrlDecode(data.Data.ToString());
					var PathArray = SourcePath.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
					var Source = System.IO.Path.Combine(ForPdfDir, PathArray[2]);
					var Des = System.IO.Path.Combine(ForPdfDir, "DownLoad/" + new Java.IO.File(Source).Name);
					Java.IO.File FileSou = new Java.IO.File(Source);
					Java.IO.File FileDes = new Java.IO.File(Des);
					if (FileDes.Exists())
					{
						//ShowAlert ("相同檔案名稱"+FileDes.Name+"已存在", null);
						this.copy(FileSou, FileDes);
						return;
					}

					this.copy(FileSou, FileDes);
				}
				//複選
				else
				{
					//string s = Android.OS.Environment.ExternalStorageDirectory.Path + "/Sample/test.txt";
					//output=new FileOutputStream(Android.OS.Environment.ExternalStorageDirectory.ToString()+"/Sample/test.txt");
					ClipData clipData = data.ClipData;
					int count = clipData.ItemCount;
					if (count > 0)
					{
						Android.Net.Uri[] uris = new Android.Net.Uri[count];
						for (int i = 0; i < count; i++)
						{
							uris[i] = clipData.GetItemAt(i).Uri;
							var SourcePaths = System.IO.Path.GetFullPath(uris[i].Path);
							var PathArray = SourcePaths.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
							var Source = System.IO.Path.Combine(ForPdfDir, PathArray[1]);

							//FromFileName = new Java.IO.File (Source);
							var Des = System.IO.Path.Combine(ForPdfDir, "DownLoad/" + new Java.IO.File(Source).Name);

							Java.IO.File FileSou = new Java.IO.File(Source);
							Java.IO.File FileDes = new Java.IO.File(Des);

							if (FileDes.Exists())
							{
								//ShowAlert ("相同檔案名"+FileDes.Name+"稱已存在", null);
								this.copy(FileSou, FileDes);
								continue;
							}

							this.copy(FileSou, FileDes);

						}
					}
					//ShowAlert ("以完成搬移檔案至DownLoad資料夾下",null);
				}
			}

		}
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
		private String GetPathToImage(Android.Net.Uri uri)
		{
			string path = null;
			// The projection contains the columns we want to return in our query.
			string[] projection = new[] { Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Data };
			using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
			{
				if (cursor != null)
				{
					int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Data);
					cursor.MoveToFirst();
					path = cursor.GetString(columnIndex);
				}
			}
			return path;
		}
		private List<FileSystemInfo> FindTemplateIcon(string icoopath, List<FileSystemInfo> visibleThings)
		{
			DirectoryInfo DirInfo = new DirectoryInfo(icoopath);
			foreach (var AllFile in DirInfo.GetFileSystemInfos().Where(item => item.Exists))
			{
				bool IsPngFile = AllFile.Extension.ToLower().EndsWith(".png");
				if (IsPngFile)
				{
					visibleThings.Add(AllFile);
				}
			}
			return visibleThings;
		}
	}
}

