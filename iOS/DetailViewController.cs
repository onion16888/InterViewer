using System;
using CoreGraphics;
using System.Collections.Generic;
using System.IO;

using UIKit;
using Debug = System.Diagnostics.Debug;


namespace InterViewer.iOS
{
	public partial class DetailViewController : UIViewController
	{
		private CGPDFDocument _pdf;
		private int _pageNumber;
		public Document doc;

		public int PageNumber
		{
			get { return this._pageNumber; }
			set
			{
				if (value >= 1 && value <= _pdf.Pages)
				{
					_pageNumber = value;
					this.View.SetNeedsDisplay();
				}
			}
		}

		public DetailViewController(IntPtr handle) : base(handle)
		{
			_pageNumber = 1;
			String filename = Path.Combine(NibBundle.ResourcePath, "0200B9.pdf");
			_pdf = CGPDFDocument.FromFile(filename);
		}
		private nfloat startX, endX = 0;
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Initial();

			// Perform any additional setup after loading the view, typically from a nib.

			this.View.BackgroundColor = UIColor.Gray;
			Debug.WriteLine(PageNumber);
			imageView.Image = GetThumbForPage();
			scrollView.ContentSize = imageView.Image.Size;


			this.scrollView.UserInteractionEnabled = true;
			UISwipeGestureRecognizer rightSwipeGes = setScrollViewChangPanGesture(UISwipeGestureRecognizerDirection.Right);
			rightSwipeGes.Direction = UISwipeGestureRecognizerDirection.Right;
			this.scrollView.AddGestureRecognizer(rightSwipeGes);

			UISwipeGestureRecognizer leftSwipeGes = setScrollViewChangPanGesture(UISwipeGestureRecognizerDirection.Left);
			leftSwipeGes.Direction = UISwipeGestureRecognizerDirection.Left;
			this.scrollView.AddGestureRecognizer(leftSwipeGes);
		

		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		public UIImage GetThumbForPage()
		{

			CGPDFPage pdfPg = _pdf.GetPage(PageNumber);
			nfloat scale;
			CGRect pageRect = pdfPg.GetBoxRect(CGPDFBox.Media);
			if (pageRect.Height > pageRect.Width)
			{
				/*nfloat swithWidth = 0.0f;
				swithWidth = pageRect.Width;
				pageRect.Width = pageRect.Height;
				pageRect.Height = swithWidth;*/
				scale = (this.View.Frame.Width-80.0f) / pageRect.Width;
			}
			else {
				scale = this.View.Frame.Height / pageRect.Height;
			}

			pageRect.Size = new CGSize(pageRect.Width * scale, pageRect.Height * scale);

			UIGraphics.BeginImageContext(pageRect.Size);
			CGContext context = UIGraphics.GetCurrentContext();

			context.SetFillColor((nfloat)1.0, (nfloat)1.0, (nfloat)1.0, (nfloat)1.0);
			context.FillRect(pageRect);

			context.SaveState();
	
			context.TranslateCTM(0, pageRect.Size.Height);
			context.ScaleCTM(1, -1);
			/*if (pageRect.Width > pageRect.Height)
			{
				context.ConcatCTM(pdfPg.GetDrawingTransform(CGPDFBox.Crop, pageRect, 0, true));
				context.TranslateCTM(-100f, -100f);
			}*/

			context.ConcatCTM(CGAffineTransform.MakeScale(scale, scale));


			context.DrawPDFPage(pdfPg);
			context.RestoreState();

			UIImage thm = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return thm;
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			imageView.Image = GetThumbForPage();
			scrollView.ContentSize = imageView.Image.Size;
			//imageView.Image.DrawAsPatternInRect(new CGRect(0, 0, 1, 1));
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


		private UISwipeGestureRecognizer setScrollViewChangPanGesture(UISwipeGestureRecognizerDirection direction)
		{
			return	new UISwipeGestureRecognizer((swipeDirection) =>
		   {
			  
			   switch (direction)
			   {
				   case UISwipeGestureRecognizerDirection.Right:
					   PageNumber++;   
					break;
				   case UISwipeGestureRecognizerDirection.Left:
					   PageNumber--;
					break;
			   }

				imageView.Image = GetThumbForPage();
			   
				scrollView.ScrollRectToVisible(new CGRect(0, 0, 100, 100), true);

				Debug.WriteLine(direction.ToString());
		   });

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


