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
		//public Document document { get; set; }
		public Document Doc { get; set; }
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
		Dictionary<string, object> NoteList = new Dictionary<string, object>();
		public DetailViewController(IntPtr handle) : base(handle)
		{
			
			_pageNumber = 1;
			//String filename = Path.Combine(NibBundle.ResourcePath, "0200B9.pdf");

		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Initial();

			// Perform any additional setup after loading the view, typically from a nib.

			_pdf = CGPDFDocument.FromFile(Doc.Reference);
			this.View.BackgroundColor = UIColor.Gray;
			Debug.WriteLine(PageNumber);
			imageView.Image = GetThumbForPage();
			scrollView.ContentSize = imageView.Image.Size;


			/*btnNote.TouchUpInside += delegate
			{


				var textNote = new UITextView();
				textNote.BackgroundColor = UIColor.FromRGB(242, 255, 0);
				textNote.Frame = new CoreGraphics.CGRect(740, 85, 225, 225);
				textNote.Font = UIFont.FromName("Helvetica-Bold", 30f);
				//textNote.Text = string.Format("text{0}", DateTime.Now.Ticks.ToString());
				string Identifier = string.Format("key{0}", DateTime.Now.Ticks.ToString());
				textNote.AccessibilityIdentifier = Identifier;
				nfloat dx = 0;
				nfloat dy = 0;

				UIPanGestureRecognizer panGesture = new UIPanGestureRecognizer((pen) =>
				{
					if ((pen.State == UIGestureRecognizerState.Began || pen.State == UIGestureRecognizerState.Changed) && (pen.NumberOfTouches == 1))
					{
						
						var p0 = pen.LocationInView(scrollView);

						if (dx == 0) dx = p0.X - textNote.Center.X;

						if (dy == 0) dy = p0.Y - textNote.Center.Y;

						var p1 = new CGPoint(p0.X - dx, p0.Y - dy);

						textNote.Center = p1;
					}
					else if (pen.State == UIGestureRecognizerState.Ended)
					{
						dx = 0;
						dy = 0;
						NoteList[textNote.AccessibilityIdentifier] = textNote;
						UITextView a = (UITextView)NoteList[textNote.AccessibilityIdentifier];
						Debug.WriteLine(a.Center.X);
					}
				});
				textNote.UserInteractionEnabled = true;
				textNote.AddGestureRecognizer(panGesture);

				NoteList.Add(Identifier, textNote);
				this.scrollView.InsertSubview(textNote, 1);

			};*/

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
			else 
			{
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


