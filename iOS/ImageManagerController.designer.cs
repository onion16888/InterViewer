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
	[Register ("ImageManagerController")]
	partial class ImageManagerController
	{
		[Outlet]
		UIKit.UITableView ImageManagerTableView { get; set; }

		[Outlet]
		UIKit.UIButton ReturnButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ReturnButton != null) {
				ReturnButton.Dispose ();
				ReturnButton = null;
			}

			if (ImageManagerTableView != null) {
				ImageManagerTableView.Dispose ();
				ImageManagerTableView = null;
			}
		}
	}
}
