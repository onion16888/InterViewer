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
			LoadPdf(FileName);

			ConvertAllPageToImages();
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
			currentPage = renderer.OpenPage(PageIndex);
		
			Bitmap bitmap = Bitmap.CreateBitmap(currentPage.Width, currentPage.Height, Bitmap.Config.Argb8888);

			currentPage.Render(bitmap, null, null, PdfRenderMode.ForDisplay);

			currentPage.Close();

			return bitmap;
		}


	}
}

