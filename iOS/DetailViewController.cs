using System;
using CoreGraphics;
using System.Collections.Generic;
using System.IO;
using UIKit;
using AssetsLibrary;
using Foundation;
using System.Drawing;
using System.Linq;
using Debug = System.Diagnostics.Debug;
namespace InterViewer.iOS
{
	public partial class DetailViewController : UIViewController
	{
		private bool openPen { get; set; }
		private CGPDFDocument _pdf;
		private int _pageNumber;
		/// <summary>
		/// Edit, Add
		/// </summary>
		/// <value>The type of the pdf.</value>
		public string PDF_Type { get; set; }
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

		CGPath path;

		List<CGPoint> pointlist = new List<CGPoint>();


		public DetailViewController(IntPtr handle) : base(handle)
		{
			_pageNumber = 1;

		}
		public string PDF_RECORD_DIR = string.Empty;
		public UIImageView newimage = new UIImageView();
		public UIImage photo = new UIImage();
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Initial();

			_pdf = CGPDFDocument.FromFile(Doc.Reference);
			this.View.BackgroundColor = UIColor.Gray;
			Debug.WriteLine(PageNumber);
			imageView.Image = GetThumbForPage();
			scrollView.ContentSize = imageView.Image.Size;


			//draw line
			UIPanGestureRecognizer pan = new UIPanGestureRecognizer((a) =>
			   {
				   CGPoint tempoint = a.LocationInView(scrollView);

				   pointlist.Add(tempoint);

				   CGPDFPage pdfPg = _pdf.GetPage(PageNumber);

				   CGRect pageRect = pdfPg.GetBoxRect(CGPDFBox.Media);
				   nfloat scale = this.View.Frame.Width / pageRect.Width;
				   pageRect.Size = new CGSize(pageRect.Width * scale, pageRect.Height * scale);
				   newimage.Frame = pageRect;
				   scrollView.AddSubview(newimage);

				   newimage.Image = Drawline();
				   if (a.State.ToString() == "Ended")
				   {

					   pointlist = new List<CGPoint>();

				   }

			   });


			btnPencil.TouchUpInside += delegate
			{
				if (openPen == false)
				{
					this.scrollView.AddGestureRecognizer(pan);
					openPen = true;
					path = new CGPath();

				}
				else {
					this.scrollView.RemoveGestureRecognizer(pan);
					openPen = false;

					SettingUIView(AttachmentTypeEnum.Paint, null);
					newimage.RemoveFromSuperview();
				};

			};


			btnCamera.BackgroundColor = UIColor.FromRGB(94, 255, 0);
			btnCamera.TouchUpInside += delegate
			{
				CameraCapture.IsOpenCamera = true;
				CameraCapture.TakePicture(this, (obj) =>
				{
					photo = obj.ValueForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;

					SettingUIView(AttachmentTypeEnum.Photo, null);

				});
			};

			btnNote.TouchUpInside += delegate
			{
				SettingUIView(AttachmentTypeEnum.Note);

			};


			#region Swipe Left and Right Gesture
			this.scrollView.UserInteractionEnabled = true;
			UISwipeGestureRecognizer rightSwipeGes = setScrollViewChangPanGesture(UISwipeGestureRecognizerDirection.Right);
			rightSwipeGes.Direction = UISwipeGestureRecognizerDirection.Right;
			this.scrollView.AddGestureRecognizer(rightSwipeGes);

			UISwipeGestureRecognizer leftSwipeGes = setScrollViewChangPanGesture(UISwipeGestureRecognizerDirection.Left);
			leftSwipeGes.Direction = UISwipeGestureRecognizerDirection.Left;
			this.scrollView.AddGestureRecognizer(leftSwipeGes);


			#endregion

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

			    ClearAllUIView();
			    LoadingAttachments();
			    SaveLoadJsonData();
				scrollView.ScrollRectToVisible(new CGRect(0, 0, 100, 100), true);


		   });

		}

		private void Initial()
		{
			btnClock.Enabled = false;
			btnTag.Enabled = false;

			btnNote.TouchUpInside += ChangeButtonState;
			btnMicrophone.TouchUpInside += ChangeButtonState;
			btnPencil.TouchUpInside += ChangeButtonState;

			imageView.AccessibilityIdentifier = "PDFImageView";

			PDF_RECORD_DIR = PDF_Type == "Add" ? DateTime.Now.Ticks.ToString() : Path.GetFileNameWithoutExtension(Doc.Name);
			if (PDF_Type == "Add")
			{
				Doc.Name = string.Format("{0}.json", PDF_RECORD_DIR);
			}

		}


		public UIImage Drawline()
		{

			CGPDFPage pdfPg = _pdf.GetPage(PageNumber);

			CGRect pageRect = pdfPg.GetBoxRect(CGPDFBox.Media);
			nfloat scale = this.View.Frame.Width / pageRect.Width;
			pageRect.Size = new CGSize(pageRect.Width * scale, pageRect.Height * scale);

			UIGraphics.BeginImageContext(pageRect.Size);
			CGContext context = UIGraphics.GetCurrentContext();

			context.SetLineWidth(2);
			path.AddLines(pointlist.ToArray());

			UIColor.Red.SetStroke();
			context.AddPath(path);
			context.DrawPath(CGPathDrawingMode.Stroke);
			context.SaveState();
			UIImage thm = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return thm;
		}



		private UIImage CropImage(UIImage SrcImage, CGRect CropRect)
		{
			using (CGImage cr = SrcImage.CGImage.WithImageInRect(CropRect))
			{
				return UIImage.FromImage(cr);
			}
		}




		#region getSaveImageLocalSystemPath


		private string getSaveImageLocalSystemPath(string IdentifierName, AttachmentTypeEnum savetype, UIImage uiimage )
		{


			string SAVE_FILE_NAME = string.Format("{0}.{1}", IdentifierName, savetype == AttachmentTypeEnum.Photo ? "jpg" : "png");
			string SYSTEM_FILE_PATH = Path.Combine(PDF_RECORD_DIR, SAVE_FILE_NAME);
			var documentsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), PDF_RECORD_DIR);
			if (!Directory.Exists(documentsDirectory))
			{
				Directory.CreateDirectory(documentsDirectory);
			}

			#region save to Local Photo
			/*
			ALAssetsLibrary library = new ALAssetsLibrary();
			library.WriteImageToSavedPhotosAlbum(photo.CGImage, meta, (assetUrl, error) =>
			{
				Debug.WriteLine(assetUrl);
			});
			*/
			#endregion

			string saveFilePath = Path.Combine(documentsDirectory, SAVE_FILE_NAME);

			using (NSData imageData = savetype == AttachmentTypeEnum.Paint ? uiimage.AsPNG() : uiimage.AsJPEG() )
			{
				NSError err = null;
				imageData.Save(
					saveFilePath,
					NSDataWritingOptions.Atomic,
					out err
				);
			}
			if (!File.Exists(saveFilePath))
			{
				SYSTEM_FILE_PATH = string.Empty;
			}
			return SYSTEM_FILE_PATH;
		}


		#endregion


		public enum UIType
		{

			UIButton,

			UIImageView,

			UITextView,
		}


		public void SettingAttachments(Attachment attachment)
		{

			if (Doc.Attachments == null)
			{
				Doc.Attachments = new List<Attachment>();
				Doc.Attachments.Add(attachment);
			}
			else
			{
				var editAttachment = Doc.Attachments.Where(x => x.Name == attachment.Name).SingleOrDefault();
				if (editAttachment == null)
				{
					Doc.Attachments.Add(attachment);
				}
				else
				{
					editAttachment = attachment;
				}
			}

		}

		public void LoadingAttachments()
		{
			List<Attachment> attachments = Doc.Attachments.Where(x => x.PageIndex == PageNumber).ToList();
			foreach (Attachment attachment in attachments)
			{
				
				switch (attachment.Type)
				{
					case AttachmentTypeEnum.Note:
						SettingUIView(AttachmentTypeEnum.Note, attachment);
						break;
					case AttachmentTypeEnum.Paint:
						SettingUIView(AttachmentTypeEnum.Paint, attachment);
						break;
					case AttachmentTypeEnum.Photo:
						SettingUIView(AttachmentTypeEnum.Photo, attachment);
						break;

				}
			
			}
		}


		public void SettingUIView(AttachmentTypeEnum type, Attachment attachment = null)
		{
			CGPoint centerPoint = new CGPoint();
			CGRect CGRectFrame = new CGRect();
			if (attachment != null)
			{
				centerPoint = new CGPoint(attachment.X, attachment.Y);
				CGRectFrame = new CGRect(attachment.X, attachment.Y, attachment.Width, attachment.Height);
			}
			string identifier = DateTime.Now.Ticks.ToString();
			if (type == AttachmentTypeEnum.Note)
			{
				UITextView textview = new UITextView();
				textview.Text = attachment == null ? string.Empty: attachment.Path;
				textview.AccessibilityIdentifier = attachment == null ? identifier : attachment.Name;
				textview.BackgroundColor = UIColor.FromRGB(242, 255, 0);
				textview.Frame = attachment == null ? new CoreGraphics.CGRect(scrollView.Center.X, scrollView.Center.Y, 100, 100): CGRectFrame;
				//textview.Center = attachment == null ? new CGPoint(scrollView.Center.X, scrollView.Center.Y) : centerPoint;
				textview.Font = UIFont.FromName("Helvetica-Bold", 30f);
				UIPanGestureRecognizer panGesture = SettingUIPanGesture(textview, UIType.UITextView);
				textview.UserInteractionEnabled = true;
				textview.AddGestureRecognizer(panGesture);

				textview.Ended += delegate
				{
					var editAttachment = Doc.Attachments.Where(x => x.Name == textview.AccessibilityIdentifier).SingleOrDefault();
					editAttachment.Path = textview.Text;
					SettingAttachments(editAttachment);
				};
				//NoteList.Add(Identifier, textNote);
				this.scrollView.InsertSubview(textview, 1);

				if (attachment == null)
				{
					Attachment newattachment = new Attachment();
					newattachment.Name = identifier;
					newattachment.PageIndex = PageNumber;
					newattachment.Path = textview.Text;
					newattachment.Type = AttachmentTypeEnum.Note;
					newattachment.Width = textview.Frame.Width;
					newattachment.Height = textview.Frame.Height;
					newattachment.X = textview.Frame.Location.X;
					newattachment.Y = textview.Frame.Location.Y;
					SettingAttachments(newattachment);
				}


			}

			if (type == AttachmentTypeEnum.Paint || type == AttachmentTypeEnum.Photo)
			{
				string systemPath = string.Empty;
				if (type == AttachmentTypeEnum.Paint && attachment == null)
				{
					newimage.BackgroundColor = UIColor.Clear;
					systemPath = getSaveImageLocalSystemPath(identifier, AttachmentTypeEnum.Paint, CropImage(newimage.Image, path.BoundingBox));
				}
				else if (type == AttachmentTypeEnum.Photo && attachment == null)
				{
					CameraCapture.IsOpenCamera = false;
					systemPath = getSaveImageLocalSystemPath(identifier, AttachmentTypeEnum.Photo, photo);
				}
				else
				{
					systemPath = attachment.Path;
				}

				string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), attachment == null ? systemPath : attachment.Path);
				var imageView = new UIImageView();

				imageView.BackgroundColor = UIColor.Clear;
				imageView.Image = UIImage.FromBundle(filePath);

				imageView.AccessibilityIdentifier = attachment == null ? identifier : attachment.Name;
				UIPanGestureRecognizer panGesture = SettingUIPanGesture(imageView, UIType.UIImageView);
				if (type == AttachmentTypeEnum.Paint && attachment == null)
				{
					imageView.Frame = new CGRect(
						path.BoundingBox.Location,
						path.BoundingBox.Size);
				}
				else
				{
					imageView.Frame = attachment == null ? new CGRect(scrollView.Center.X, scrollView.Center.Y, 225, 225) : CGRectFrame;
				}
				//imageView.Center = attachment == null ? new CGPoint(scrollView.Center.X, scrollView.Center.Y) : centerPoint;
				imageView.UserInteractionEnabled = true;
				imageView.AddGestureRecognizer(panGesture);
				this.scrollView.InsertSubview(imageView, 1);

				if (attachment == null)
				{
					Attachment newattachment = new Attachment();
					newattachment.Name = identifier;
					newattachment.PageIndex = PageNumber;
					newattachment.Path = systemPath;
					newattachment.Type = type;
					newattachment.Width = imageView.Frame.Width;
					newattachment.Height = imageView.Frame.Height;
					newattachment.X = imageView.Frame.Location.X;
					newattachment.Y = imageView.Frame.Location.Y;
					SettingAttachments(newattachment);
				}
			}
		}

		public void ClearAllUIView()
		{
			foreach (var ui in scrollView.Subviews)
			{
				if (ui is UITextView)
				{
					var view = ui as UITextView;
					view.RemoveFromSuperview();
				}

				if (ui is UIImageView)
				{
					var view = ui as UIImageView;
					if (view.AccessibilityIdentifier != "PDFImageView")
						view.RemoveFromSuperview();
				}
			}
		}

		public void SaveLoadJsonData()
		{
			InterViewerService interviewerservice = new InterViewerService();
			interviewerservice.SaveAsJson(Doc);
		}


		public UIPanGestureRecognizer SettingUIPanGesture(object uiobject, UIType uitype)
		{
			string identifier = string.Empty;
			CGPoint centerPoint = new CGPoint();
			CGRect CGRectFrame = new CGRect();
			/*if (attachment != null)
			{
				centerPoint = new CGPoint(attachment.X, attachment.Y);
				CGRectFrame = new CGRect(attachment.X, attachment.Y, attachment.Width, attachment.Height);
			}*/
			nfloat X = 0;
			nfloat Y = 0;
			nfloat x = 0;
			nfloat y = 0;
			return new UIPanGestureRecognizer((pen) =>
				   {
					   if ((pen.State == UIGestureRecognizerState.Began || pen.State == UIGestureRecognizerState.Changed) && (pen.NumberOfTouches == 1))
					   {
						   nfloat uiCenterX = 0;
						   nfloat uiCenterY = 0;
						   switch (uitype)
						   {
							   case UIType.UIImageView:
								   var uiimage = (UIImageView)uiobject;
								   uiCenterX = uiimage.Center.X;
								   uiCenterY = uiimage.Center.Y;
								   break;
							   case UIType.UIButton:
								   var uibutton = (UIButton)uiobject;
								   uiCenterX = uibutton.Center.X;
								   uiCenterY = uibutton.Center.Y;
								   break;

							   case UIType.UITextView:
								   var uitext = (UITextView)uiobject;
								   uiCenterX = uitext.Center.X;
								   uiCenterY = uitext.Center.Y;

								   break;
						   }

						   var p0 = pen.LocationInView(scrollView);

						   if (x == 0) x = p0.X - uiCenterX;

						   if (y == 0) y = p0.Y - uiCenterY;

						   var p1 = new CGPoint(p0.X - x, p0.Y - y);

						   switch (uitype)
						   {
							   case UIType.UIImageView:
								   var uiimage = (UIImageView)uiobject;
								   uiimage.Center = p1;
								   X = uiimage.Frame.Location.X;
								   Y = uiimage.Frame.Location.Y;
								   break;
							   case UIType.UIButton:
								   var uibutton = (UIButton)uiobject;
								   uibutton.Center = p1;
								   X = uibutton.Frame.Location.X;
								   Y = uibutton.Frame.Location.Y;
								   break;
							   case UIType.UITextView:
								   var uitext = (UITextView)uiobject;
								   uitext.Center = p1;
								   X = uitext.Frame.Location.X;
								   Y = uitext.Frame.Location.Y;
								   break;
						   }
					   }
					   else if (pen.State == UIGestureRecognizerState.Ended)
					   {
						   x = 0;
						   y = 0;
					       
						   switch (uitype)
						   {
							   case UIType.UIImageView:
								   var uiimage = (UIImageView)uiobject;
								   centerPoint = uiimage.Center;
								   identifier = uiimage.AccessibilityIdentifier;
								   break;
							   case UIType.UIButton:
								   var uibutton = (UIButton)uiobject;
								   centerPoint = uibutton.Center;
								   identifier = uibutton.AccessibilityIdentifier;
								   break;
							   case UIType.UITextView:
								   var uitext = (UITextView)uiobject;
								   centerPoint = uitext.Center;
								   identifier = uitext.AccessibilityIdentifier;
								   break;
						   }

						   var moveAttachment = Doc.Attachments.Where(z => z.Name == identifier).SingleOrDefault();
						   if (moveAttachment != null)
						   {
							   moveAttachment.X = X;
							   moveAttachment.Y = Y;
						   }
						 
					   }
				   });

		}

	}
}


