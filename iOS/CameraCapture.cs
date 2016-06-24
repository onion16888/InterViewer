using System;
using UIKit;
using Foundation;

namespace InterViewer.iOS
{
	public static class CameraCapture
	{
		static CameraPickerController picker;
		static Action<NSDictionary> _callback;
		//static public UIViewController p;

		static public bool IsOpenCamera { get; set; } = false;


		static void Init()
		{
			if (picker != null)
				return;
			CameraCapture.IsOpenCamera = true;
			picker = new CameraPickerController();
			picker.Delegate = new CameraDelegate();

		}

		class CameraDelegate : UIImagePickerControllerDelegate
		{

			public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
			{
				var cb = _callback;
				_callback = null;

				picker.DismissModalViewController(true);
				cb(info);
			}
		}

		public static void TakePicture(UIViewController parent, Action<NSDictionary> callback)
		{
			Init();
			picker.SourceType = UIImagePickerControllerSourceType.Camera;
			_callback = callback;
			parent.PresentModalViewController(picker, true);
		}

		public static void SelectPicture(UIViewController parent, Action<NSDictionary> callback)
		{
			Init();
			picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
			_callback = callback;
			parent.PresentModalViewController(picker, true);
		}
	}
}

