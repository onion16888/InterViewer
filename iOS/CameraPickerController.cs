using System;
using UIKit;

namespace InterViewer.iOS
{
	public class CameraPickerController : UIImagePickerController
	{
		

		/*public override bool ShouldAutorotate()
		{
			return false;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			return UIKit.UIInterfaceOrientationMask.All;
		}*/

		public override bool ShouldAutorotate()
		{
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			return UIInterfaceOrientationMask.Landscape;
		}

		public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
		{
			return UIInterfaceOrientation.LandscapeRight;
		}
	}
}

