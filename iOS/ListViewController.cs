using System;
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

		public ListViewController(IntPtr handle) : base(handle)
		{
			selectedColor = new UIColor(red: 0.95f, green: 0.52f, blue: 0.00f, alpha: 1.0f);
			normalColor = new UIColor(red: 0.70f, green: 0.70f, blue: 0.70f, alpha: 1.0f);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Perform any additional setup after loading the view, typically from a nib.
			IEnumerable<string> fileOrDirectory = GetDirFileList("Sliders2");

			CheckButtonIsSelected(btnTemplate);

			CollectionViewInit(fileOrDirectory);
			//var qwe = Directory.EnumerateFileSystemEntries("./InterView/Sliders");

			btnTemplate.TouchUpInside += (object sender, EventArgs e) =>
			{
				InvokeOnMainThread(() =>
				{
					CheckButtonIsSelected(btnTemplate);

					CollectionViewInit(GetDirFileList("Sliders2"));
				});
			};
			btnDocuments.TouchUpInside+= (object sender, EventArgs e) => 
			{
				InvokeOnMainThread(() =>
				{
					CheckButtonIsSelected(btnDocuments);

					CollectionViewInit(GetDirFileList("Documents2"));
				});
			};
		}

		public static  IEnumerable<string> GetDirFileList(string Whichfolder)
		{
			var fileOrDirectory = Directory.EnumerateFileSystemEntries("./InterView/" + Whichfolder);
			foreach (var entry in fileOrDirectory)
			{
				Console.WriteLine(entry);
			}

			return fileOrDirectory;
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
			var qq = ListViewController.GetDirFileList("PdfFile2");

			Doc.Reference = e.Selected.Replace(".png",".pdf").Replace("Sliders2","PdfFile2").Replace("Documents2","PdfFile2");
			Console.WriteLine(Doc.Reference);


			InvokeOnMainThread(() =>
			{
				PerformSegue("moveToDetailViewSegue", this);
			});
		}
		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue(segue, sender);
			switch (segue.Identifier)
			{
				case @"moveToDetailViewSegue":
					{
						if (segue.DestinationViewController is DetailViewController)
						{
							var Detailviewcontroller = segue.DestinationViewController as DetailViewController;
							//把這個頁面的值傳給新頁面的屬性
							Detailviewcontroller.doc = this.Doc;

						}
						break;
					}
					//break;
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


