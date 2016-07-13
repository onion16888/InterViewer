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
		private InterViewerService interviewerservice;
		private IOService ioService;
		private string DocumentPath;
		private bool IsPdfBackground = true;
		private CGRect imageBackgroundRect;

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

		//要畫畫的View
		public UIImageView DrawImageView = new UIImageView();

		public UIImage photo = new UIImage();
		public CGRect PDFpageRect = new CGRect();

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ioService = new IOService();
			interviewerservice = new InterViewerService(ioService);
			DocumentPath = ioService.GetDocumentDirectory();

			Initial();

			//reloadAttachment
			LoadingAttachments();
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		#region PDFDrawImage&LineHandle


		public UIImage GetDrawlineImage()
		{

			CGRect pageRect;
			if (IsPdfBackground == true)
			{
				pageRect = PDFpageRect;
			}
			else {
				pageRect = imageBackgroundRect;
			}
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


		public UIImage GetCropImage(UIImage SrcImage, CGRect CropRect)
		{
			using (CGImage cr = SrcImage.CGImage.WithImageInRect(CropRect))
			{
				return UIImage.FromImage(cr);
			}
		}

		public UIImage GetPDFImageForPage()
		{

			CGPDFPage pdfPg = _pdf.GetPage(PageNumber);
			nfloat scale;
			PDFpageRect = pdfPg.GetBoxRect(CGPDFBox.Media);
			if (PDFpageRect.Height > PDFpageRect.Width)
			{
				scale = (this.View.Frame.Width - 80.0f) / PDFpageRect.Width;
			}
			else
			{
				scale = this.View.Frame.Height / PDFpageRect.Height;
			}

			PDFpageRect.Size = new CGSize(PDFpageRect.Width * scale, PDFpageRect.Height * scale);

			UIGraphics.BeginImageContext(PDFpageRect.Size);
			CGContext context = UIGraphics.GetCurrentContext();

			context.SetFillColor((nfloat)1.0, (nfloat)1.0, (nfloat)1.0, (nfloat)1.0);
			context.FillRect(PDFpageRect);

			context.SaveState();

			context.TranslateCTM(0, PDFpageRect.Size.Height);
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

			imageView.Image = GetPDFImageForPage();
			scrollView.ContentSize = imageView.Image.Size;
		}

		#endregion

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

		#region Button Event 
		private void ButtonEventHandle()
		{
			UIPanGestureRecognizer pencil = GetUIPencilPanGesture();
			btnPencil.TouchUpInside += delegate
			{
				if (openPen == false)
				{
					//新增畫筆的指型事件
					scrollView.AddGestureRecognizer(pencil);
					openPen = true;
					path = new CGPath();
				}
				else {
					scrollView.RemoveGestureRecognizer(pencil);
					openPen = false;

					//儲存畫布至記憶體
					CreateNewUIViewHandle(AttachmentTypeEnum.Paint);
					//移除畫布
					DrawImageView.RemoveFromSuperview();
				};

			};


			btnCamera.BackgroundColor = UIColor.FromRGB(94, 255, 0);
			btnCamera.TouchUpInside += delegate
			{
				CameraCapture.IsOpenCamera = true;
				CameraCapture.TakePicture(this, (obj) =>
				{
					photo = obj.ValueForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;

					CreateNewUIViewHandle(AttachmentTypeEnum.Photo);

				});
			};

			btnNote.TouchUpInside += delegate
			{
				CreateNewUIViewHandle(AttachmentTypeEnum.Note);
			};
		}
		#endregion


		private void Initial()
		{
			btnClock.Enabled = false;
			btnTag.Enabled = false;

			btnNote.TouchUpInside += ChangeButtonState;
			btnPencil.TouchUpInside += ChangeButtonState;

			//將pdf的imageVie設定識別碼
			imageView.AccessibilityIdentifier = "PDFImageView";

			#region 產生存放畫線＆圖片的資料夾名稱
			PDF_RECORD_DIR = PDF_Type == "Add" ? DateTime.Now.Ticks.ToString() : Path.GetFileNameWithoutExtension(Doc.Name);
			//only doc.name is null had to process file name
			if (PDF_Type == "Add" && Doc.Name == null)
			{
				Doc.Name = string.Format("{0}.json", PDF_RECORD_DIR);
			}

			#endregion

			#region 處理image的資料來源，如pdf 或是jpg

			if (Path.GetExtension(Doc.Reference).ToLower().Equals(".pdf"))
			{
				_pdf = CGPDFDocument.FromFile(Doc.Reference);
				this.View.BackgroundColor = UIColor.Gray;
				Debug.WriteLine(PageNumber);
				imageView.Image = GetPDFImageForPage();
				IsPdfBackground = true;
			}
			else {
				UIImage imageBackground = UIImage.FromFile(Doc.Reference);
				int width = (int)imageBackground.Size.Width;
				int height = (int)imageBackground.Size.Height;
				nfloat scale;
				if (height > width)
				{
					scale = (View.Frame.Width - 80.0f) / width;
				}
				else
				{
					scale = this.View.Frame.Height / height;
				}
				imageView.Image = imageBackground;
				imageBackgroundRect = new CGRect();
				imageBackgroundRect.Size = new CGSize(width * scale, height * scale);
				IsPdfBackground = false;
			}

			//將整個scrollview大小對應pdf的大小
			scrollView.ContentSize = imageView.Image.Size;


			#endregion

			#region 工具列事件處理
			// add done button in navigation bar
			var navButtonDone = new UIBarButtonItem(UIBarButtonSystemItem.Done, (s, e) =>
			{
				#region save data

				// 畫筆未關時，進行關閉
				if (openPen)
				{
					openPen = false;
					//儲存畫布至記憶體中
					CreateNewUIViewHandle(AttachmentTypeEnum.Paint);
					//移除畫布
					DrawImageView.RemoveFromSuperview();
				}

				UIViewDataSaveHandle();

				#endregion

				NavigationController.PopToRootViewController(true);
			});
			navItem.SetRightBarButtonItem(navButtonDone, true);
			#endregion

			//按鈕事件處理
			ButtonEventHandle();

			#region 判斷是pdf來源才處理滑動換頁事件
			if (IsPdfBackground == true)
			{
				SwipeChangPageHandle();
			}
			#endregion
		}

		#region 資料儲存處理

		#region getSaveImageLocalSystemPath


		private string getSaveImageLocalSystemPath(string IdentifierName, AttachmentTypeEnum savetype, UIImage uiimage)
		{
			string SAVE_FILE_NAME = string.Format("{0}.{1}", IdentifierName, savetype == AttachmentTypeEnum.Photo ? "jpg" : "png");
			string SYSTEM_FILE_PATH = Path.Combine(PDF_RECORD_DIR, SAVE_FILE_NAME);
			var documentsDirectory = Path.Combine(DocumentPath, PDF_RECORD_DIR);
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

			using (NSData imageData = savetype == AttachmentTypeEnum.Paint ? uiimage.AsPNG() : uiimage.AsJPEG())
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

		/// <summary>
		/// 建立新的UIView處理程序
		/// </summary>
		/// <returns>The UIV iew setting handle.</returns>
		/// <param name="type">Type.</param>
		public void CreateNewUIViewHandle(AttachmentTypeEnum type)
		{
			//建立新的附件物件
			Attachment newAttachment = GetNewAttachment(type);
			//根據Attachment物件，建立UIView物件
			CreateUIView(newAttachment);
			//編輯Attachment物件
			EditDocumentAttachments(newAttachment);
			//儲存至Json檔
			SaveJsonData();
		}

		public void SaveJsonData()
		{
			interviewerservice.SaveAsJson(Doc);
		}

		#endregion

		#region Attachments物件處理

		public void EditDocumentAttachments(Attachment attachment)
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

		/// <summary>
		/// 新增新的附件元素
		/// </summary>
		/// <returns>The new attachment.</returns>
		/// <param name="type">Type.</param>
		public Attachment GetNewAttachment(AttachmentTypeEnum type)
		{
			string identifier = DateTime.Now.Ticks.ToString();
			UIView uiview = new UIView();
			string systemPath = string.Empty;
			if (type == AttachmentTypeEnum.Note)
			{
				uiview.Frame = new CGRect(scrollView.Center.X, scrollView.Center.Y, 100, 100);
			}
			else
			{
				if (type == AttachmentTypeEnum.Paint)
				{
					uiview.Frame = new CGRect(path.BoundingBox.Location, path.BoundingBox.Size);
				}
				else
				{
					uiview.Frame = new CGRect(scrollView.Center.X, scrollView.Center.Y, 225, 225);
				}

				UIImage saveUImage = type == AttachmentTypeEnum.Paint ? GetCropImage(DrawImageView.Image, path.BoundingBox) : photo;
				systemPath = getSaveImageLocalSystemPath(identifier, type, saveUImage);
			}

			Attachment newAttachment = new Attachment();
			newAttachment.Name = identifier;
			newAttachment.PageIndex = PageNumber;
			newAttachment.Note = string.Empty;
			newAttachment.Type = type;
			newAttachment.Path = systemPath;
			newAttachment.Width = uiview.Frame.Width;
			newAttachment.Height = uiview.Frame.Height;
			newAttachment.X = uiview.Frame.Location.X;
			newAttachment.Y = uiview.Frame.Location.Y;

			return newAttachment;
		}

		public void LoadingAttachments()
		{
			if (Doc.Attachments != null)
			{
				List<Attachment> attachments = Doc.Attachments.Where(x => x.PageIndex == PageNumber).ToList();
				foreach (Attachment attachment in attachments)
				{
					CreateUIView(attachment);
				}
			}

		}

		#endregion

		#region UIView實作

		/// <summary>
		/// UIView資料處理與建立方法
		/// </summary>
		/// <returns>The iew data save handle.</returns>
		public void UIViewDataSaveHandle()
		{
			ClearAllUIView();
			LoadingAttachments();
			SaveJsonData();
		}



		public void CreateUIView(Attachment attachment)
		{
			CGPoint centerPoint = new CGPoint();
			CGRect CGRectFrame = new CGRect();
			centerPoint = new CGPoint(attachment.X, attachment.Y);
			CGRectFrame = new CGRect(attachment.X, attachment.Y, attachment.Width, attachment.Height);
			if (attachment.Type == AttachmentTypeEnum.Note)
			{
				UITextView textview = new UITextView();
				textview.Text = attachment.Note;
				textview.AccessibilityIdentifier = attachment.Name;
				textview.BackgroundColor = UIColor.FromRGB(242, 255, 0);
				textview.Frame = CGRectFrame;
				textview.Font = UIFont.FromName("Helvetica-Bold", 30f);
				UIPanGestureRecognizer panGesture = GetUIViewPanGesture(textview);
				textview.UserInteractionEnabled = true;
				textview.AddGestureRecognizer(panGesture);

				textview.Ended += delegate
				{
					var editAttachment = Doc.Attachments.Where(x => x.Name == textview.AccessibilityIdentifier).SingleOrDefault();
					editAttachment.Note = textview.Text;
				};

				scrollView.InsertSubview(textview, 1);
			}
			else
			{
				string systemPath = string.Empty;
				string filePath = Path.Combine(DocumentPath, attachment.Path);
				var imageView = new UIImageView();
				imageView.BackgroundColor = UIColor.Clear;
				imageView.Image = UIImage.FromBundle(filePath);
				imageView.AccessibilityIdentifier = attachment.Name;
				imageView.Frame = CGRectFrame;
				imageView.UserInteractionEnabled = true;
				imageView.AddGestureRecognizer(GetUIViewPanGesture(imageView));
				scrollView.InsertSubview(imageView, 1);
			}
		}

		public void ClearAllUIView()
		{
			foreach (var ui in scrollView.Subviews)
			{
				//移除所有便利貼物件
				if (ui is UITextView)
				{
					var view = ui as UITextView;
					view.RemoveFromSuperview();
				}

				//移除所有image物件，pdf的底圖不移除
				if (ui is UIImageView)
				{
					var view = ui as UIImageView;
					if (view.AccessibilityIdentifier != "PDFImageView")
						view.RemoveFromSuperview();
				}
			}
		}

		#endregion

		#region UIView指型事件設定

		//滑動換頁處理方法
		public void SwipeChangPageHandle()
		{
			scrollView.UserInteractionEnabled = true;
			UISwipeGestureRecognizer rightSwipeGes = GetScrollViewChangPanGesture(UISwipeGestureRecognizerDirection.Right);
			rightSwipeGes.Direction = UISwipeGestureRecognizerDirection.Right;
			scrollView.AddGestureRecognizer(rightSwipeGes);

			UISwipeGestureRecognizer leftSwipeGes = GetScrollViewChangPanGesture(UISwipeGestureRecognizerDirection.Left);
			leftSwipeGes.Direction = UISwipeGestureRecognizerDirection.Left;
			scrollView.AddGestureRecognizer(leftSwipeGes);
		}

		/// <summary>
		/// 取得畫筆指型滑動事件
		/// </summary>
		/// <returns>The UIP encil pan gesture.</returns>
		public UIPanGestureRecognizer GetUIPencilPanGesture()
		{
			return new UIPanGestureRecognizer((pencil) =>
			   {
				   CGPoint tempoint = pencil.LocationInView(scrollView);

				   pointlist.Add(tempoint);

				   if (IsPdfBackground == true)
				   {
					   DrawImageView.Frame = PDFpageRect;
				   }
				   else {
					   DrawImageView.Frame = imageBackgroundRect;
				   }

				   scrollView.AddSubview(DrawImageView);

				   //移動時，持續對畫布畫畫
				   DrawImageView.Image = GetDrawlineImage();

				   if (pencil.State.ToString() == "Ended")
				   {
					   //放開時，清除紀錄筆觸的容器
					   pointlist = new List<CGPoint>();
				   }
			   });
		}


		/// <summary>
		/// 取得滑動換頁指型事件
		/// </summary>
		/// <returns>The scroll view chang pan gesture.</returns>
		/// <param name="direction">Direction.</param>
		public UISwipeGestureRecognizer GetScrollViewChangPanGesture(UISwipeGestureRecognizerDirection direction)
		{
			return new UISwipeGestureRecognizer((swipeDirection) =>
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
			  imageView.Image = GetPDFImageForPage();
			  UIViewDataSaveHandle();
			  scrollView.ScrollRectToVisible(new CGRect(0, 0, 100, 100), true);
		  });
		}

		/// <summary>
		/// 取得設定UIImageView,UITextView指型事件
		/// </summary>
		/// <returns>The UIV iew pan gesture.</returns>
		/// <param name="uiview">Uiview.</param>
		public UIPanGestureRecognizer GetUIViewPanGesture(UIView uiview)
		{
			string identifier = uiview.AccessibilityIdentifier; ;
			nfloat diffX = 0;
			nfloat diffY = 0;
			return new UIPanGestureRecognizer((pen) =>
				   {
					   if ((pen.State == UIGestureRecognizerState.Began || pen.State == UIGestureRecognizerState.Changed) && (pen.NumberOfTouches == 1))
					   {
						   nfloat uiCenterX = uiview.Center.X; ;
						   nfloat uiCenterY = uiview.Center.Y;

						   var touchPoint = pen.LocationInView(scrollView);

						   if (diffX == 0)
						   {
							   diffX = touchPoint.X - uiCenterX;
						   }

						   if (diffY == 0)
						   {
							   diffY = touchPoint.Y - uiCenterY;
						   }

						   uiview.Center = new CGPoint(touchPoint.X - diffX, touchPoint.Y - diffY);

					   }
					   else if (pen.State == UIGestureRecognizerState.Ended)
					   {
						   diffX = 0;
						   diffY = 0;

						   var moveAttachment = Doc.Attachments.Where(x => x.Name == identifier).SingleOrDefault();
						   if (moveAttachment != null)
						   {
							   moveAttachment.X = uiview.Frame.Location.X;
							   moveAttachment.Y = uiview.Frame.Location.Y;
						   }
					   }
				   });
		}


		#endregion

	}
}


