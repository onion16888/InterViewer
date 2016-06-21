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
	[Register ("ListViewController")]
	partial class ListViewController
	{
		[Outlet]
		UIKit.UIButton btnAdd { get; set; }

		[Outlet]
		UIKit.UIButton btnDocuments { get; set; }

		[Outlet]
		UIKit.UIButton btnImages { get; set; }

		[Outlet]
		UIKit.UIButton btnTemplate { get; set; }

		[Outlet]
		UIKit.UICollectionView MyCollectionView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MyCollectionView != null) {
				MyCollectionView.Dispose ();
				MyCollectionView = null;
			}

			if (btnAdd != null) {
				btnAdd.Dispose ();
				btnAdd = null;
			}

			if (btnDocuments != null) {
				btnDocuments.Dispose ();
				btnDocuments = null;
			}

			if (btnImages != null) {
				btnImages.Dispose ();
				btnImages = null;
			}

			if (btnTemplate != null) {
				btnTemplate.Dispose ();
				btnTemplate = null;
			}
		}
	}
}
