using System;

using Foundation;
using UIKit;

namespace InterViewer.iOS
{
	public partial class MyCollectionViewCell : UICollectionViewCell
	{
		protected MyCollectionViewCell(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}
		public void UpdateCellData(string Data)
		{
			//TestLabel.Text = i.ToString();
			PdfIcon.Image = UIImage.FromFile(Data);
		}
	}
}
