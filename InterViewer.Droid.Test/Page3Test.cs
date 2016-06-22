using System;
using System.IO;
using NUnit.Framework;
using Android.App;
using Android.Content;
using Android.OS;

namespace InterViewer.Droid.Test
{
	[TestFixture]
	public class Page3Test : Activity
	{

	/*	[SetUp]
		public void Setup() { }


		[TearDown]
		public void Tear() { }
*/
		[Test]
		public void Pass()
		{
			var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			string pdfFilepath = System.IO.Path.Combine(dir, "0200B9.pdf");

			if (!File.Exists(pdfFilepath))
			{
				using (var source = Assets.Open(@"0200B9.pdf"))
				using (var dest = OpenFileOutput("0200B9.pdf", FileCreationMode.WorldReadable | FileCreationMode.WorldWriteable))
				{
					source.CopyTo(dest);
				}
			}

			PDFDocument pdf = new PDFDocument(this, pdfFilepath);
			var count = pdf.Count;
			//Console.WriteLine("test1");
			Assert.True(count == 2);
		}

		[Test]
		public void Fail()
		{
			Assert.False(true);
		}

		[Test]
		[Ignore("another time")]
		public void Ignore()
		{
			Assert.True(false);
		}

		[Test]
		public void Inconclusive()
		{
			Assert.Inconclusive("Inconclusive");
		}
	}
}

