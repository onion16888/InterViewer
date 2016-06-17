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
	[Register ("ViewController")]
	partial class ViewController
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
		MapKit.MKMapView map { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
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

			if (map != null) {
				map.Dispose ();
				map = null;
			}
		}
	}
}
