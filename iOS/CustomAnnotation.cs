using System;
using CoreLocation;
using MapKit;

namespace InterViewer.iOS
{
	public class CustomAnnotation : MKAnnotation
	{
		private String _title { set; get; }
		private CLLocationCoordinate2D _coord { set; get; }

		public Int32 Count { set; get; }

		public CLLocationCoordinate2D Location { 
			get
			{
				return _coord;
			}
			set
			{
				_coord = value;
			}
		}

		public CustomAnnotation()
		{

		}

		public CustomAnnotation(String title)
		{
			_title = title;
		}

		public override string Title
		{
			get
			{
				return _title;
			}
		}

		public override CLLocationCoordinate2D Coordinate
		{
			get
			{
				return _coord;
			}
		}

	}
}

