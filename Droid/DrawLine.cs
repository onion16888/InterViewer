
using Android.Content;
using Android.Graphics;
using Android.Views;
using System.Collections.Generic;
using System;
using Debug = System.Diagnostics.Debug;

namespace InterViewer.Droid
{

	public class PencilDrawLine : View
	{
		Bitmap bitmap;
		Bitmap newbitmap;
	 	Canvas canvasview;
		FindRect findRect;
		public PencilDrawLine(Context context)
			: base(context)
		{
			bitmap = Bitmap.CreateBitmap(context.WallpaperDesiredMinimumWidth, context.WallpaperDesiredMinimumHeight, Bitmap.Config.Argb8888);
			canvasview = new Canvas(bitmap);
			findRect = new FindRect();
			//canvasview.ClipRect(new Rect(200, 200, 555, 555));
			//var paint = new Paint
			//{
			//	Color = Color.Red
			//};
			//canvasview = new Canvas(bitmap);
			//paint.SetStyle(Paint.Style.Stroke);

			//canvasview.DrawBitmap(bitmap, new Rect(200, 200, 333, 440), new Rect(200, 200, 300, 300),paint);
			//canvasview.DrawColor(Color.Yellow);
			//canvasview.Restore();
		}

		public Bitmap GetRectBitmap(View viewContent)
		{
			Bitmap rectBitmap = Bitmap.CreateBitmap(CreateDrawableFromView(viewContent), findRect.leftX, findRect.topY, findRect.RectWidth, findRect.RectHight);
			return rectBitmap;
		}

		public float GetRectLeftX()
		{
			return findRect.leftX;
		}

		public float GetRectTopY()
		{
			return findRect.topY;
		}

		Path path = new Path();
		public override bool OnTouchEvent(MotionEvent e)
		{
			Point point = new Point((Int32)e.GetX(), (Int32)e.GetY());
			Debug.WriteLine("x");
			var paint = new Paint
			{
				Color = Color.Red
			};
			findRect.FindBestRect(point.X, point.Y);
			paint.SetStyle(Paint.Style.Stroke);
			paint.StrokeWidth = 5;
			switch (e.ActionMasked)
			{
				case MotionEventActions.Down:
					{
						path.MoveTo(point.X, point.Y);
						//canvasview.DrawPath(path, paint);
						break;
					}
				case MotionEventActions.Move:
					{
						path.LineTo(point.X, point.Y);
						//canvasview.DrawPath(path, paint);
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
			base.OnDraw(canvas);

			var paint = new Paint
			{
				Color = Color.Red
			};

			paint.SetStyle(Paint.Style.Stroke);
			paint.StrokeWidth = 5;
			canvas.DrawPath(path, paint);

		}

		public Bitmap CreateDrawableFromView(View view)
		{
			Bitmap bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);

			using (Canvas canvas = new Canvas(bitmap))
			{
				view.Measure(
					View.MeasureSpec.MakeMeasureSpec(view.Width, MeasureSpecMode.Exactly),
					View.MeasureSpec.MakeMeasureSpec(view.Height, MeasureSpecMode.Exactly)
				);
				view.Layout(0, 0, view.MeasuredWidth, view.MeasuredHeight);
				view.Draw(canvas);
			}

			return bitmap;
		}
	}
}

