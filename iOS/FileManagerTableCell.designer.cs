// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace InterViewer.iOS
{
	[Register ("FileManagerTableCell")]
	partial class FileManagerTableCell
	{
		[Outlet]
		UIKit.UIButton ButtonSelected { get; set; }

		[Outlet]
		UIKit.UILabel LabelFilePath { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LabelFilePath != null) {
				LabelFilePath.Dispose ();
				LabelFilePath = null;
			}

			if (ButtonSelected != null) {
				ButtonSelected.Dispose ();
				ButtonSelected = null;
			}
		}
	}
}
