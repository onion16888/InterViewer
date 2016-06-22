
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace InterViewer.iOS.Test
{
	[TestFixture]
	public class DocumentTest
	{
		List<Document> documents { get; set; }

		public DocumentTest()
		{
			documents = new InterViewerService().GetDocuments();
		}

		[Test]
		public void Pass()
		{
			Assert.True(documents.Count == 12);
		}

		[Test]
		public void Fail()
		{
			Assert.False(documents.Count == 10);
		}

		[Test]
		[Ignore("another time")]
		public void Ignore()
		{
			Assert.True(false);
		}
	}
}
