using System;
using UIKit;
using CoreGraphics;
using CoreAnimation;
using Foundation;
using System.Drawing;
using System.Collections.Generic;
using MapKit;
using CoreLocation;

namespace InterViewer.iOS
{
	public class MapCanvasView : UIView
	{
		private CGPath path;
		private CGPoint initialPoint;
		private CGPoint latestPoint;
		private List<CGPoint> pointlist = new List<CGPoint>();

		public MKMapView MapView;

		protected string DrawType { get; set; }

		public MapCanvasView()
		{
			BackgroundColor = UIColor.Clear;

			Opaque = false;

			path = new CGPath();	
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			UITouch touch = touches.AnyObject as UITouch;

			if (touch != null)
			{
				initialPoint = touch.LocationInView(this);
				pointlist.Add(initialPoint);
			}
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);

			UITouch touch = touches.AnyObject as UITouch;

			if (touch != null)
			{
				latestPoint = touch.LocationInView(this);
				pointlist.Add(latestPoint);
				SetNeedsDisplay();
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			List<CLLocationCoordinate2D> CoordinateList = new List<CLLocationCoordinate2D>();

			pointlist.ForEach(Point =>
			{
				CoordinateList.Add(MapView.ConvertPoint(Point, MapView));
			});

			if (CoordinateList.Count > 0)
			{
				MapView.AddOverlay(MKPolyline.FromCoordinates(CoordinateList.ToArray()));
			}

			pointlist = new List<CGPoint>();
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			using (CGContext g = UIGraphics.GetCurrentContext())
			{
				g.SetStrokeColor(UIColor.Black.CGColor);
				g.SetFillColor(UIColor.Clear.CGColor);
				g.SetLineWidth(2);

				path.AddLines(pointlist.ToArray());

				g.AddPath(path);
				g.DrawPath(CGPathDrawingMode.Stroke);
				g.SaveState();
			}
		}
	}
}

