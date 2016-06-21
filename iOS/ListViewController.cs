using System;

using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using System.IO;

namespace InterViewer.iOS
{
	public partial class ListViewController : UIViewController
	{
		public ListViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.
			IEnumerable<string> fileOrDirectory = GetIconPath("Documents");

			CollectionViewInit(fileOrDirectory);

			btnTemplate.TouchUpInside += (object sender, EventArgs e) =>
			{
				InvokeOnMainThread(() =>
				{
					CollectionViewInit(GetIconPath("Sliders"));
				});
			};
			btnDocuments.TouchUpInside+= (object sender, EventArgs e) => 
			{
				InvokeOnMainThread(() =>
				{
					CollectionViewInit(GetIconPath("Documents"));
				});
			};
		}

		static IEnumerable<string> GetIconPath(string Whichfolder)
		{
			var fileOrDirectory = Directory.EnumerateFileSystemEntries("./InterView/"+Whichfolder);
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
			List<Int32> temp = new List<Int32>();

			for (Int32 i = 0; i < 500; i++)
			{
				temp.Add(i);
			}

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
			Console.WriteLine(e.Selected);
		}
	}
}


