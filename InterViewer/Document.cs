using System;
using System.Collections.Generic;

namespace InterViewer
{
	public class Document
	{
		public string Title { get; set; }

		public string Name { get; set; }

		public string Reference { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public List<Attachment> Attachments { get; set; }
	}
}