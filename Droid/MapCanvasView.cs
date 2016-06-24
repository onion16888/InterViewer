using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Views;

namespace InterViewer.Droid
{
	public class MapCanvasView : View
	{
		private Path path = new Path();

		private Paint drawPaint;

		private GoogleMap _map;

		private Queue<LatLng> PointQueue; 

		public MapCanvasView(Context context, GoogleMap map) : base(context)
		{
			_map = map;
			setupPaint();
		}

		private void setupPaint()
		{
			drawPaint = new Paint();
			drawPaint.Color = Color.Black;
			drawPaint.AntiAlias = true;
			drawPaint.StrokeWidth = 5;
			drawPaint.SetStyle(Paint.Style.Stroke);
			drawPaint.StrokeJoin = Paint.Join.Round;
			drawPaint.StrokeCap = Paint.Cap.Round;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			Point point = new Point((Int32)e.GetX(), (Int32)e.GetY());

			switch (e.ActionMasked)
			{
				case MotionEventActions.Down:
					{
						path.MoveTo(point.X, point.Y);

						PointQueue = new Queue<LatLng>();

						PointQueue.Enqueue(_map.Projection.FromScreenLocation(point));

						break;
					}
				case MotionEventActions.Move:
					{
						path.LineTo(point.X, point.Y);

						PointQueue.Enqueue(_map.Projection.FromScreenLocation(point));

						Polyline polyline = _map.AddPolyline(
							new PolylineOptions().Add(PointQueue.ToArray())
						);

						polyline.Width = 5;
						polyline.Color = Color.Black;

						PointQueue.Dequeue();

						break;
					}
				default:
					{
						return false;
					}
			}

			PostInvalidate();
			return true;
		}

		protected override void OnDraw(Canvas canvas)
		{
			canvas.DrawPath(path, drawPaint);
		}
	}
}
