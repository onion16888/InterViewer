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
	[Register ("DetailViewController")]
	partial class DetailViewController
	{
		[Outlet]
		UIKit.UIButton btnCamera { get; set; }

		[Outlet]
		UIKit.UIButton btnClock { get; set; }

		[Outlet]
		UIKit.UIButton btnMicrophone { get; set; }

		[Outlet]
		UIKit.UIButton btnNote { get; set; }

		[Outlet]
		UIKit.UIButton btnPencil { get; set; }

		[Outlet]
		UIKit.UIButton btnTag { get; set; }

		[Outlet]
		UIKit.UIImageView imageView { get; set; }

		[Outlet]
		UIKit.UINavigationItem navItem { get; set; }

		[Outlet]
		UIKit.UIScrollView scrollView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (navItem != null) {
				navItem.Dispose ();
				navItem = null;
			}

			if (btnCamera != null) {
				btnCamera.Dispose ();
				btnCamera = null;
			}

			if (btnClock != null) {
				btnClock.Dispose ();
				btnClock = null;
			}

			if (btnMicrophone != null) {
				btnMicrophone.Dispose ();
				btnMicrophone = null;
			}

			if (btnNote != null) {
				btnNote.Dispose ();
				btnNote = null;
			}

			if (btnPencil != null) {
				btnPencil.Dispose ();
				btnPencil = null;
			}

			if (btnTag != null) {
				btnTag.Dispose ();
				btnTag = null;
			}

			if (imageView != null) {
				imageView.Dispose ();
				imageView = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}
		}
	}
}
