
using System;
using System.IO;
using Foundation;
using QuickLook;
using UIKit;

namespace InterViewer.iOS
{
	public class PDFItem : QLPreviewItem
	{
		string title;
		string uri;

		public PDFItem(string title, string uri)
		{
			this.title = title;
			this.uri = uri;
		}

		public override string ItemTitle
		{
			get { return title; }
		}

		public override NSUrl ItemUrl
		{
			get { return NSUrl.FromFilename(uri); }
		}
	}

	public class PDFPreviewControllerDataSource : QLPreviewControllerDataSource
	{
		string url = "";
		string filename = "";

		public PDFPreviewControllerDataSource(string url, string filename)
		{
			this.url = url;
			this.filename = filename;
		}

		public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
		{
			return new PDFItem(filename, url);
		}

		public override nint PreviewItemCount(QLPreviewController controller)
		{
			return 1;
		}
	}

	public class PdfUtils
	{
		public static void DisplayPdf(string path, UINavigationController parentController)
		{
			QLPreviewController previewController = new QLPreviewController();
			previewController.DataSource = new PDFPreviewControllerDataSource(path, Path.GetFileName(path));

			parentController.PushViewController(previewController, true);
		}
	}
}

