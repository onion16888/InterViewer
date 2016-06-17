using System;

using UIKit;

namespace InterViewer.iOS
{
	public partial class DetailViewController : UIViewController
	{
		public DetailViewController(IntPtr handle) : base(handle)
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

				switch (btn.CurrentTitle)
				{
					case "筆記":
						btnNote.Selected = false;
						break;
					case "便利貼":
						btnPencil.Selected = false;
						break;
				}
			}
		}

		private void Initial()
		{
			btnClock.Enabled = false;
			btnTag.Enabled = false;

			btnNote.TouchUpInside += ChangeButtonState;
			btnMicrophone.TouchUpInside += ChangeButtonState;
			btnPencil.TouchUpInside += ChangeButtonState;
		}
	}
}


