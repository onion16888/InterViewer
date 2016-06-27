// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using UIKit;

namespace InterViewer.iOS
{
	public partial class ImageManagerTableCell : UITableViewCell
	{
		public ImageManagerTableCell (IntPtr handle) : base (handle)
		{
		}
		public void ReloadData(string filePath, bool fileOrFolder)
		{
			char[] seperater = { '/' };
			string[] filePathTemp = filePath.Split(seperater);
			LabelFilePath.Text = filePathTemp[filePathTemp.Length - 1];
			if (fileOrFolder == true)
				ButtonSelected.SetTitle("File", UIControlState.Normal);
			else
				ButtonSelected.SetTitle("Path", UIControlState.Normal);
		}
	}
}
