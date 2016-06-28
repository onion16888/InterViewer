﻿using System;
using System.Linq;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using System.IO;

namespace InterViewer.iOS
{
	public partial class ListViewController : UIViewController
	{
		public Document Doc { get; set; }

		private UIColor selectedColor;
		private UIColor normalColor;
		private const bool _Add = true;
		private const bool _Edit = false;
		private bool _AddOrEdit = true;
		public ListViewController(IntPtr handle) : base(handle)
		{
			selectedColor = new UIColor(red: 0.95f, green: 0.52f, blue: 0.00f, alpha: 1.0f);
			normalColor = new UIColor(red: 0.70f, green: 0.70f, blue: 0.70f, alpha: 1.0f);
		}
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Perform any additional setup after loading the view, typically from a nib.
			//練習用
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/InterView/Documents";

			//把共用目錄路徑丟進去檢查公用目錄下的/Interview是否存在
			CheckDir(Environment.GetFolderPath(Environment.SpecialFolder.Personal));

			//練習用
			var filename = Path.Combine(documents, "Write.txt");
			File.WriteAllText(filename, "Write this text into a file");

			//預設先撈Sliders
			IEnumerable<string> fileOrDirectory = GetDirPngFile("Sliders");

			CheckButtonIsSelected(btnTemplate);
			//把Sliders下的.png集合發給grid
			CollectionViewInit(fileOrDirectory);

			//Slider按鈕
			btnTemplate.TouchUpInside += (object sender, EventArgs e) =>
			{
				_AddOrEdit = _Add;
				InvokeOnMainThread(() =>
				{
					CheckButtonIsSelected(btnTemplate);
					//取得Sliders下的.png送給grid
					CollectionViewInit(GetDirPngFile("Sliders"));
				});
			};
			btnDocuments.TouchUpInside += (object sender, EventArgs e) => 
			{
				_AddOrEdit = _Edit;
				InvokeOnMainThread(() =>
				{
					CheckButtonIsSelected(btnDocuments);
					//取得Documents下的.png送給grid
					CollectionViewInit(GetDirPngFile("Documents"));
				});
			};
			btnImages.TouchUpInside += (object sender, EventArgs e) =>
			{
				InvokeOnMainThread(() =>
				{
					PerformSegue(@"moveToImageManagerSegue", this);
				});
			};
			btnAdd.TouchUpInside += (object sender, EventArgs e) =>
			{
				InvokeOnMainThread(() =>
				{
					PerformSegue(@"moveToFileManagerSegue", this);
				});
			};
		}
		//檢查範例檔案是否存在,不存在就從Resources匯入
		public void CheckDir(string Path)
		{
			if(!Directory.Exists(Path + "/InterView"))
			{
				Directory.Move("./InterView", Path+"/InterView");
			}
		}
		//暫時用不到
		public static  IEnumerable<string> GetDirFileList(string Whichfolder)
		{
			var fileOrDirectory = Directory.EnumerateFileSystemEntries("./InterView/" + Whichfolder);

			foreach (var entry in fileOrDirectory)
			{
				Console.WriteLine(entry);
			}

			return fileOrDirectory;
		}
		//撈出資料夾下所有.Png檔案
		public static IEnumerable<string> GetDirPngFile(string Whichfolder)
		{
			//看送過來的是哪個資料夾 撈出底下所有的.png 回傳
			var PngFileList = Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/InterView/" + Whichfolder);

			var result = PngFileList.Where(FilePath => Path.GetExtension(FilePath) == ".png");

			return result;
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
		public void CollectionViewInit(IEnumerable<string> PngSource)
		{
			//List<Int32> temp = new List<Int32>();

			//for (Int32 i = 0; i < 500; i++)
			//{
			//	temp.Add(i);
			//}
			List<string> ss = new List<string>();
			ss.AddRange(PngSource);
			//var v=from qwe in ss where 
			var source = new TableSource(PngSource);
			MyCollectionView.Source = source;

			MyCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout
			{
				SectionInset = new UIEdgeInsets(10, 10, 10, 10),
				ItemSize = new CGSize(50, 50),
				//MinimumInteritemSpacing = 20,
				ScrollDirection = UICollectionViewScrollDirection.Vertical
			}, true);

			source.Selected += ItemOnSelected;
		}
		public class TableSource : UICollectionViewSource
		{
			const String CollectionViewCellIdentifier = "MyCollectionViewCell";

			public List<string> Source { get; set; }

			public TableSource(IEnumerable<string> list)
			{
				Source = new List<string>();
				Source.AddRange(list);
			}

			public override nint GetItemsCount(UICollectionView collectionView, nint section)
			{
				return Source.Count;
			}

			public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
			{
				var cell = collectionView.DequeueReusableCell(CollectionViewCellIdentifier, indexPath) as MyCollectionViewCell;

				var data = Source[indexPath.Row];

				cell.UpdateCellData(data);

				return cell;
			}

			public event EventHandler<SelectedEventArgs> Selected;

			public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
			{
				var data = Source[indexPath.Row];
				collectionView.DeselectItem(indexPath, true);

				// Raise Event
				EventHandler<SelectedEventArgs> handle = Selected;

				if (null != handle)
				{
					var args = new SelectedEventArgs { Selected = data };
					handle(this, args);
				}
			}

			public class SelectedEventArgs : EventArgs
			{
				public string Selected { get; set; }
			}
		}
		//GridView ItemTouch
		private void ItemOnSelected(Object sender, TableSource.SelectedEventArgs e)
		{
			//var qq = ListViewController.GetDirFileList("PdfFile2");

			//Doc.Reference = e.Selected.Replace(".png",".pdf").Replace("Sliders2","PdfFile2").Replace("Documents2","PdfFile2");
			//Doc.Thumbnail = e.Selected;

			if (File.Exists(e.Selected.Replace(".png", ".pdf")))
				Doc.Reference = e.Selected.Replace(".png", ".pdf").Replace("Sliders2", "PdfFile2").Replace("Documents2", "PdfFile2");
			else
				Doc.Reference = e.Selected;
			Doc.Thumbnail = e.Selected;

			Console.WriteLine(Doc.Reference);

			InvokeOnMainThread(() =>
			{
				PerformSegue("moveToDetailViewSegue", this);
			});
		}
		//準備傳值
		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue(segue, sender);
			switch (segue.Identifier)
			{
				case @"moveToDetailViewSegue":
					if (segue.DestinationViewController is DetailViewController)
					{
						if (segue.DestinationViewController is DetailViewController)
						{
							var Detailviewcontroller = segue.DestinationViewController as DetailViewController;

							if(_AddOrEdit==_Add)
								Detailviewcontroller.PDF_Type = "Add";
							else
								Detailviewcontroller.PDF_Type = "Edit";
						//把這個頁面的值傳給新頁面的屬性

							Detailviewcontroller.Doc = this.Doc;
						}
						break;
					}
					break;
				case @"moveToFileManagerSegue":
					if (segue.DestinationViewController is FileManagerController)
					{
						
					}
					break;
				case @"moveToImageManagerSegue":
					if (segue.DestinationViewController is ImageManagerController)
					{

					}
					break;	
				default:
					break;
			}
		}

		private void CheckButtonIsSelected(UIButton button)
		{
			UIButton otherBtn = button.CurrentTitle == "範本" ? btnDocuments : btnTemplate;

			if (otherBtn.Selected)
			{
				otherBtn.Selected = false;
				otherBtn.BackgroundColor = normalColor;
			}

			button.Selected = true;
			button.BackgroundColor = selectedColor;
		}

	}
}


