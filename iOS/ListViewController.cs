using System;

using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using System.IO;
using AssetsLibrary;

namespace InterViewer.iOS
{
	public partial class ListViewController : UIViewController
	{
		UIImagePickerController imagePicker;
		string FileName;
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
			//btnImages.TouchUpInside += (object sender, EventArgs e) => 
			{
			//	imagePicker = new UIImagePickerController();

			//	// set our source to the photo library
			//	imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;

			//	// set what media types
			//	imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);

			//	imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
			//	imagePicker.Canceled += Handle_Canceled;

			//	UINavigationController ss = new UINavigationController();
			//	// show the picker
			//	InvokeOnMainThread(() =>
			//	{
			//		ss.PresentModalViewController(imagePicker, true);
			//	});
			//	//NavigationController.PresentModalViewController(imagePicker, true);

			};
		}
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			btnImages.TouchUpInside += (object sender, EventArgs e) =>
			{
				imagePicker = new UIImagePickerController();

				// set our source to the photo library
				imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;

				// set what media types
				imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);

				imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
				imagePicker.Canceled += Handle_Canceled;

				UINavigationController ss = new UINavigationController();
				// show the picker
				ss.PresentModalViewController(imagePicker, true);
				//NavigationController.PresentModalViewController(imagePicker, true);
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
		void Handle_Canceled(object sender, EventArgs e)
		{
			Console.WriteLine("picker cancelled");
			imagePicker.DismissModalViewController(true);
		}
		protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
		{
			// determine what was selected, video or image
			bool isImage = false;
			switch (e.Info[UIImagePickerController.MediaType].ToString())
			{
				case "public.image":
					Console.WriteLine("Image selected");
					isImage = true;
					break;

				case "public.video":
					Console.WriteLine("Video selected");
					break;
			}

			Console.Write("Reference URL: [" + UIImagePickerController.ReferenceUrl + "]");
			var vv = UIImagePickerController.ReferenceUrl;

			// get common info (shared between images and video)
			NSUrl referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceURL")] as NSUrl;
			if (referenceURL != null)
				Console.WriteLine(referenceURL.ToString());
			ALAssetsLibrary assetsLibrary = new ALAssetsLibrary();
			assetsLibrary.AssetForUrl(referenceURL, delegate (ALAsset asset)
			{

				ALAssetRepresentation representation = asset.DefaultRepresentation;

				if (representation == null)
				{
					return;

				}
				else {


					FileName = representation.Filename;
					var gg = representation.Description;
					Console.WriteLine("Image Filename :" + FileName);
					UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;

					using (NSData imageData = originalImage.AsPNG())
					{
						Byte[] myByteArray = new Byte[imageData.Length];
						System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));

						string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

						//string qwe = NibBundle.ResourcePath;
						var FilePath = Path.Combine(path, FileName);
						File.WriteAllBytes(FilePath, myByteArray);
					}
				}
			}, delegate (NSError error)
			{
				Console.WriteLine("User denied access to photo Library... {0}", error);

			});

			// if it was an image, get the other image info
			if (isImage)
			{
				// get the original image
				UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
				if (originalImage != null)
				{
					// do something with the image
					Console.WriteLine("got the original image");

					//imageView.Image = originalImage;
				}

				// get the edited image
				UIImage editedImage = e.Info[UIImagePickerController.EditedImage] as UIImage;
				if (editedImage != null)
				{
					// do something with the image
					Console.WriteLine("got the edited image");
					//imageView.Image = editedImage;
				}

				//- get the image metadata
				NSDictionary imageMetadata = e.Info[UIImagePickerController.MediaMetadata] as NSDictionary;
				if (imageMetadata != null)
				{
					// do something with the metadata
					Console.WriteLine("got image metadata");
				}

			}
			// if it's a video
			else
			{
				// get video url
				NSUrl mediaURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
				if (mediaURL != null)
				{
					//
					Console.WriteLine(mediaURL.ToString());
				}
			}

			// dismiss the picker
			imagePicker.DismissModalViewController(true);
		}
	}
}


