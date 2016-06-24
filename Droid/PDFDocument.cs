using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using System;
using Com.Artifex.Mupdfdemo;
using System.IO;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;

namespace InterViewer.Droid
{
	public class PDFDocument
	{
		public MuPDFCore Doc;

		public Int32 Count { get; set; }

		public List<Bitmap> Images { get; set; }

		public PDFDocument(Context ctxt, String FileName)
		{
			Doc = new MuPDFCore(ctxt, FileName);
			Count = Doc.CountPages();
			ConvertToImages();
		}

		public PDFDocument(Context ctxt, String FileName, int PageIndex)
		{
			Doc = new MuPDFCore(ctxt, FileName);
			Count = Doc.CountPages();
			ConvertToImages(PageIndex);
		}

		private List<Bitmap> ConvertToImages()
		{
			Images = new List<Bitmap>();

			if (Doc == null)
			{
				throw new Exception("Could not load document");
			}
			var cookie = new MuPDFCore.Cookie(Doc);
			for (Int32 i = 0; i < Count; i++)
			{
				var size = Doc.GetPageSize(i);

				int pageWidth = (int)size.X;
				int pageHeight = (int)size.Y;

				int ScreenWidth = pageWidth;
				int ScreenHeight = pageHeight;

				Bitmap bitmap = Bitmap.CreateBitmap(ScreenWidth, ScreenHeight, Bitmap.Config.Argb8888);
				Doc.DrawPage(bitmap, i, pageWidth, pageHeight, 0, 0, ScreenWidth, ScreenHeight, cookie);
				Images.Add(bitmap);
			}
			return Images;
		}

		private List<Bitmap> ConvertToImages(int i)
		{
			Images = new List<Bitmap>();

			if (Doc == null)
			{
				throw new Exception("Could not load document");
			}
			var cookie = new MuPDFCore.Cookie(Doc);
				var size = Doc.GetPageSize(i);

				int pageWidth = (int)size.X;
				int pageHeight = (int)size.Y;

				int ScreenWidth = pageWidth;
				int ScreenHeight = pageHeight;

				Bitmap bitmap = Bitmap.CreateBitmap(ScreenWidth, ScreenHeight, Bitmap.Config.Argb8888);
				Doc.DrawPage(bitmap, i, pageWidth, pageHeight, 0, 0, ScreenWidth, ScreenHeight, cookie);
				Images.Add(bitmap);
			return Images;
		}
	}
}

