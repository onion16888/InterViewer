using System;
using System.Collections.Generic;
using System.IO;
using CoreGraphics;
using Foundation;
using UIKit;

namespace PDFToImage.iOS
{
	public class PDFDocument
	{
		private CGPDFDocument Doc;

		public Int32 Count { get; set; }

		public List<UIImage> Images { get; set; }

		public PDFDocument(String FileName)
		{
			Doc = CGPDFDocument.FromFile(FileName);
			Count = (Int32)Doc.Pages;
			ConvertToImages();
		}

		private List<UIImage> ConvertToImages()
		{
			Images = new List<UIImage>();

			if (Doc == null)
			{
				throw new Exception("Could not load document");
			}

			for (Int32 i = 1; i <= Count; i++)
			{
				using (CGPDFPage Page = Doc.GetPage(i))
				{
					CGRect PageRect = Page.GetBoxRect(CGPDFBox.Media);
					//nfloat Scale = 1;//View.Frame.Height / PageRect.Height;
					//PageRect.Size = new CGSize(PageRect.Height * Scale, PageRect.Width * Scale);

					if (PageRect.Height > PageRect.Width)
					{
						PageRect.Size = new CGSize(PageRect.Height, PageRect.Width);
					}

					CGRect MediaBox = Page.GetBoxRect(CGPDFBox.Media);
					CGRect CropBox = Page.GetBoxRect(CGPDFBox.Crop);

					nfloat TopMargin = CropBox.GetMinY() - MediaBox.GetMinY();
					nfloat BottomMargin = MediaBox.GetMaxY() - CropBox.GetMaxY();
					nfloat LeftMargin = CropBox.GetMinX() - MediaBox.GetMinX();
					nfloat RightMargin = MediaBox.GetMaxX() - CropBox.GetMaxX();

					if (TopMargin + BottomMargin + LeftMargin + RightMargin > 0)
					{
						PageRect = new CGRect(
							PageRect.Location,
							new CGSize(
								PageRect.Size.Width - (LeftMargin + RightMargin),
								PageRect.Size.Height - (TopMargin + BottomMargin)
							)
						);
					}

					UIGraphics.BeginImageContext(PageRect.Size);

					using (CGContext context = UIGraphics.GetCurrentContext())
					{
						context.SaveState();

						context.TranslateCTM(0, PageRect.Size.Height);
						context.ScaleCTM(1, -1);

						context.ConcatCTM(
							Page.GetDrawingTransform(CGPDFBox.Crop, PageRect, 0, true)
						);

						context.DrawPDFPage(Page);
						context.RestoreState();

						Images.Add(UIGraphics.GetImageFromCurrentImageContext());
					}

					UIGraphics.EndImageContext();
				}
			}
			return Images;
		}

		public void Save(String SavePath)
		{

			if (!Directory.Exists(SavePath))
			{
				Directory.CreateDirectory(SavePath);
			}

			Images.ForEach(Image =>
			{
				Int32 Index = Images.IndexOf(Image);

				using (NSData imageData = Image.AsJPEG())
				{
					NSError err = null;
					imageData.Save(
						Path.Combine(SavePath, String.Format("{0:0000}.jpg", Index)),
						NSDataWritingOptions.Atomic,
						out err
					);
				}
			});
		}
	}
}



