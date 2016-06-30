using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;

using Android.Graphics.Pdf;


namespace InterViewer.Droid
{
	public class PDFDocument
	{
		private PdfRenderer renderer;

		public Int32 Count { get; set; }

		public List<Bitmap> Images { get; set; }

		private PdfRenderer.Page currentPage { get; set; }

		public PDFDocument(Context ctxt, String FileName)
		{
			if (System.IO.Path.GetExtension(FileName) == ".pdf")
			{
				LoadPdf(FileName);

				ConvertAllPageToImages();
			}
			else
			{
				LoadImage(FileName);
			}
		}

		public PDFDocument(Context ctxt, String FileName, int PageIndex)
		{
			LoadPdf(FileName);

			Images = new List<Bitmap>() { ConvertSinglePageToImages(PageIndex) };
		}

		private void LoadPdf(String FileName)
		{
			Java.IO.File file = new Java.IO.File(FileName);

			renderer = new PdfRenderer(ParcelFileDescriptor.Open(file, ParcelFileMode.ReadOnly));

			Count = renderer.PageCount;

		}

		private void LoadImage(String FileName)
		{
			Images = new List<Bitmap>();
			Bitmap imageBackground = BitmapFactory.DecodeFile(FileName);
			Images.Add(imageBackground);
			Count = 1;
		}

		private void ConvertAllPageToImages()
		{
			Images = new List<Bitmap>();

			for (int PageIndex = 0; PageIndex < Count; PageIndex++)
			{
				Images.Add(ConvertSinglePageToImages(PageIndex));
			}
		}

		private Bitmap ConvertSinglePageToImages(int PageIndex) 
		{
			using (currentPage = renderer.OpenPage(PageIndex))
			{
				using (Bitmap bitmap = Bitmap.CreateBitmap(currentPage.Width, currentPage.Height, Bitmap.Config.Argb4444))
				{
					currentPage.Render(bitmap, null, null, PdfRenderMode.ForDisplay);

					//Double scale = 1f / 1f;

					//Bitmap resizedBitmap = Bitmap.CreateScaledBitmap(
					//	bitmap,
					//	(Int32)(bitmap.Width * scale),
					//	(Int32)(bitmap.Height * scale),
					//	false
					//);

					currentPage.Close();

					return bitmap;
				}
			}
		}
	}
}

