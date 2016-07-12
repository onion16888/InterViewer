using System;
using System.Linq;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using System.IO;
using Newtonsoft.Json;

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
		List<JsonIndex> JsonNameAndPng=new List<JsonIndex>();
		private IOService IIO;
		private InterViewerService InterViewService;
		private string TempJsonName=null;
		public ListViewController(IntPtr handle) : base(handle)
		{
			selectedColor = new UIColor(red: 0.95f, green: 0.52f, blue: 0.00f, alpha: 1.0f);
			normalColor = new UIColor(red: 0.70f, green: 0.70f, blue: 0.70f, alpha: 1.0f);
		}
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			IIO = new IOService();
			InterViewService = new InterViewerService(IIO);

			Console.WriteLine(IIO.AppPath);

			//把共用目錄路徑丟進去檢查公用目錄下的/Interview是否存在
			CheckDir(IIO.AppPath);

			// Perform any additional setup after loading the view, typically from a nib.
			//練習用
			var documents = IIO.AppPath + "/InterView/Documents";

			//練習用
			var filename = Path.Combine(documents, "Write.txt");
			File.WriteAllText(filename, "Write this text into a file");

			//預設先撈Sliders
			IEnumerable<string> fileOrDirectory = GetDirPngFile("Sliders");
			//把Sliders下的.png集合發給grid
			JsonNameAndPng = null;
			CollectionViewInit(fileOrDirectory);

			CheckButtonIsSelected(btnTemplate);


			//Slider按鈕
			btnTemplate.TouchUpInside += (object sender, EventArgs e) =>
			{
				_AddOrEdit = _Add;
				InvokeOnMainThread(() =>
				{
					CheckButtonIsSelected(btnTemplate);
					JsonNameAndPng = null;
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

					//GetJsonFile();

					//取得Documents下的.png送給grid
					CollectionViewInit(GetJsonFile());
					//CollectionViewInit(GetDirPngFile("Documents"));
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
			if (!Directory.Exists(Path + "/InterView"))
			{
				Directory.Move("./InterView", Path + "/InterView");
			}
		}
		//撈出資料夾下所有.Png檔案
		public IEnumerable<string> GetDirPngFile(string Whichfolder)
		{
			//看送過來的是哪個資料夾 撈出底下所有的.png 回傳
			IEnumerable<string> PngFileList = Directory.EnumerateFiles(IIO.AppPath + "/InterView/" + Whichfolder);

			IEnumerable<string> result = PngFileList.Where(FilePath => Path.GetExtension(FilePath) == ".png");

			return result;
		}
		//撈出Jimmy底下所有的json
		public List<string> GetJsonFile()
		{
			//會有重複累加,如果有就清空
			if (JsonNameAndPng != null)
			{ JsonNameAndPng.Clear(); }
			else
			//沒有累加就初始化	
			{ JsonNameAndPng = new List<JsonIndex>(); }

			List<string> DocPng = new List<string>();
			List<string> JsonList = GetJsonLFileList();
			foreach (var h in JsonList)
			{
				//讀取每一筆路徑下的Json資料
				string JsonContent = IIO.ReadAllText(h);
				//反序列化
				Document DocJson = JsonConvert.DeserializeObject<Document>(JsonContent);
				//利用GetFileName取得路徑下的縮圖檔案名
				String filename = Path.GetFileName(DocJson.Thumbnail);
				//組成參考圖路徑
				String path = IIO.AppPath + "/InterView/Sliders/" + filename;
				Boolean Check = File.Exists(path);
				//加到陣列裡，準備給第二個按鈕排列用
				DocPng.Add(path);
				JsonNameAndPng.Add(new JsonIndex { JsonName = DocJson.Name, JsonPng = path });
			}

			return DocPng;
		}
		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
		public void CollectionViewInit(IEnumerable<string> PngSource)
		{
			List<string> _PngSource = new List<string>();
			_PngSource.AddRange(PngSource);
			//var v=from qwe in ss where 
			var source = new TableSource(PngSource,JsonNameAndPng);
			MyCollectionView.Source = source;

			//設定東西南北距離，長寬大小，橫排直排
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
			public List<JsonIndex> _JsonName { get; set;}


			public TableSource(IEnumerable<string> list,List<JsonIndex> _JsonName)
			{
				this._JsonName = _JsonName;

				Source = new List<string>();
				Source.AddRange(list);
			}

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
				string JsonFileName=null;
				if(this._JsonName!=null)
				{
					JsonIndex SomeItem=_JsonName.ToArray()[indexPath.Row];
					JsonFileName = SomeItem.JsonName;
				}

				var data = Source[indexPath.Row];

				collectionView.DeselectItem(indexPath, true);

				// Raise Event
				EventHandler<SelectedEventArgs> handle = Selected;

				if (null != handle)
				{
					var args = new SelectedEventArgs { Selected = data,JsonName=JsonFileName };
					handle(this, args);
				}
			}

			public class SelectedEventArgs : EventArgs
			{
				public string Selected { get; set; }
				public string JsonName { get; set; }
			}
		}
		//GridView ItemTouch
		private void ItemOnSelected(Object sender, TableSource.SelectedEventArgs e)
		{
			//var qq = ListViewController.GetDirFileList("PdfFile2");

			//Doc.Reference = e.Selected.Replace(".png",".pdf").Replace("Sliders2","PdfFile2").Replace("Documents2","PdfFile2");
			//Doc.Thumbnail = e.Selected;

			if (File.Exists(e.Selected.Replace(".png", ".pdf")))
				Doc.Reference = e.Selected.Replace(".png", ".pdf");
			else
				Doc.Reference = e.Selected;
			Doc.Thumbnail = e.Selected;

			Console.WriteLine(e.JsonName);
			TempJsonName = e.JsonName;
			Console.WriteLine(Doc.Reference + Doc.Thumbnail);

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
						var Detailviewcontroller = segue.DestinationViewController as DetailViewController;

						if (_AddOrEdit == _Add)
						{
								Detailviewcontroller.PDF_Type = "Add";
								Detailviewcontroller.Doc = this.Doc;
						}
						else
						{
								//賦予第三頁PDF類型
							Detailviewcontroller.PDF_Type = "Edit";
							List<Document> Json=InterViewService.GetDocuments();
							foreach(var h in Json)
							{
								if(h.Name.Equals(TempJsonName))
								{
									Detailviewcontroller.Doc = InterViewService.CopyAttachment(this.Doc,h);
								}
							}
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
		//去公開目錄底下，撈出所有所有.json資料回傳成List
		public List<string> GetJsonLFileList()
		{
			IEnumerable<string> JsonFile = Directory.EnumerateFiles(IIO.AppPath);

			IEnumerable<string> result = JsonFile.Where(FilePath => Path.GetExtension(FilePath) == ".json");

			List<string> JsonList = result.ToList();

			return JsonList;
		}
		public class JsonIndex
		{
			public string JsonName { get; set;}
			public string JsonPng { get; set;}
		}
	}
}


