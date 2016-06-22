
using System;
using NUnit.Framework;
using System.IO;

using UIKit;

namespace InterViewer.iOS.Test
{
	[TestFixture]
	public class Page3Test: UIViewController
	{
		
		[Test]
		public void Pass()
		{
			String filename = Path.Combine(NibBundle.BundlePath, "0200B9.pdf");
			PDFDocument pdf = new PDFDocument(filename);
			Assert.True(2 ==pdf.Images.Count);
		}

		[Test]
		public void Fail()
		{
			String filename = Path.Combine(NibBundle.BundlePath, "0200B9.pdf");
			PDFDocument pdf = new PDFDocument(filename);
			Assert.False(2 != pdf.Images.Count);
		}

		[Test]
		[Ignore("another time")]
		public void Ignore()
		{
			Assert.True(false);
		}
	}
}
