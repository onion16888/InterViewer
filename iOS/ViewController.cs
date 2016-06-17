using System;

using UIKit;

namespace InterViewer.iOS
{
	public partial class ViewController : UIViewController
	{
		public ViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Initial();

			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.		
		}

		protected void ChangeButtonState(object sender, EventArgs e)
		{
			var btn = sender as UIButton;
			if (btn != null)
			{
				btn.Selected = !btn.Selected;
			}
		}

		private void Initial()
		{
			btnNote.Enabled = false;
			btnClock.Enabled = false;
			btnTag.Enabled = false;
			btnMicrophone.Enabled = false;
			btnCamera.Enabled = false;

			btnPencil.TouchUpInside += ChangeButtonState;
		}
	}
}
