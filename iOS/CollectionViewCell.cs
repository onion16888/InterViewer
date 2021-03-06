// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using UIKit;

namespace InterViewer.iOS
{
	public partial class CollectionViewCell : UICollectionViewCell
	{
		public CollectionViewCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCellData(Document doc)
		{
			ImageView.Image = UIImage.FromFile(doc.Thumbnail);
			Label.Text = doc.Title;
		}
	}
}
