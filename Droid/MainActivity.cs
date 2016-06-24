﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using Android.Runtime;
using System;
using Android.Gms.Maps.Model;
using Android.Gms.Maps;
using Android.Views;
using Android;
using Android.Views.Animations;
using System.Collections.Generic;
using Android.Graphics;
using Android.Content;
using Android.Util;
using Android.Content.PM;
using Geolocator.Plugin;

namespace InterViewer.Droid
{
	[Activity(Label = "Test", MainLauncher = true
	          , Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Landscape)]
	public class MainActivity : Activity
	, ILocationListener
	, GoogleMap.IOnCameraChangeListener
	{
		private LatLng CenterLocation { get; set; }

		private Boolean CanvasEnable { get; set; } = false;

		private Boolean SideViewIsOpen { get; set; } = false;

		private MapFragment _mapFragment;

		private GoogleMap _map;
		//private Location _currentLocation;

		//private bool currentLocationReveived;
		public String _locationProvider;

		public LocationManager _locationManager;

		private LatLng defaultLocation = new LatLng(23.9737437408, 120.981806398);

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Main);

			defaultLocation = new LatLng(22.6115547, 120.2912767);

			ViewInit();

			GridViewInit();

			MapViewInit();

		}

		public override void OnWindowFocusChanged(bool hasFocus)
		{
			base.OnWindowFocusChanged(hasFocus);

			RelativeLayout relativeLayout = FindViewById<RelativeLayout>(Resource.Id.test);
			{
				Int32 centerX = relativeLayout.Width / 2;
				Int32 centerY = relativeLayout.Height / 2;

				View triangleView = FindViewById<TriangleView>(Resource.Id.triangle_view);
				{
					triangleView.SetX(centerX - DpToPx(15));
					triangleView.SetY(centerY);
				}

				View rectangleView = FindViewById<LinearLayout>(Resource.Id.rectangle_view);
				{
					rectangleView.SetX(centerX - (DpToPx(100) / 2));
					rectangleView.SetY(centerY + DpToPx(22));
				}

				View sideView = FindViewById<RelativeLayout>(Resource.Id.side_view);
				{
					sideView.SetX(relativeLayout.Width);
					sideView.SetY(0);
				}
			}
		}

		public void ViewInit()
		{
			RelativeLayout relativeLayout = FindViewById<RelativeLayout>(Resource.Id.test);

			TriangleView triangleView = new TriangleView(this);
			{
				triangleView.Id = Resource.Id.triangle_view;
				triangleView.LayoutParameters = new ViewGroup.LayoutParams(DpToPx(100), DpToPx(100));

				relativeLayout.AddView(triangleView);
			}

			View rectangleView = LayoutInflater.Inflate(Resource.Layout.rectangle_view_layout, null);
			{
				rectangleView.Id = Resource.Id.rectangle_view;
				rectangleView.LayoutParameters = new ViewGroup.LayoutParams(DpToPx(100), DpToPx(100));

				relativeLayout.AddView(rectangleView);
			}

			View sideView = LayoutInflater.Inflate(Resource.Layout.side_view_layout, null);
			{
				sideView.Id = Resource.Id.side_view;
				sideView.LayoutParameters = new ViewGroup.LayoutParams(DpToPx(430), ViewGroup.LayoutParams.MatchParent);

				relativeLayout.AddView(sideView);
			}

			EditText ProjectNameEditText = FindViewById<EditText>(Resource.Id.ProjectNameEditText);

			RelativeLayout PlusButton = FindViewById<RelativeLayout>(Resource.Id.PlusButton);
			{
				PlusButton.Click += (object sender, EventArgs e) =>
				{

					Console.WriteLine("Plus Button Click");

					if (rectangleView.Width == DpToPx(100))
					{
						ProjectNameEditText.Text = "";

						RotateAnimation PlusButtonOpenAnimation = new RotateAnimation(0, 90, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f)
						{
							Duration = 250
						};

						PlusButtonOpenAnimation.SetAnimationListener(new CustomAnimationListener(this, "PlusButtonOpen"));

						PlusButton.StartAnimation(PlusButtonOpenAnimation);


						rectangleView.StartAnimation(new ResizeAnimation(rectangleView, new ViewGroup.LayoutParams(DpToPx(400), rectangleView.Height))
						{
							Duration = 1000
						});
					}
					else
					{
						RotateAnimation PlusButtonCloseAnimation = new RotateAnimation(0, -90, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f)
						{
							Duration = 250
						};

						PlusButtonCloseAnimation.SetAnimationListener(new CustomAnimationListener(this, "PlusButtonClose"));

						PlusButton.StartAnimation(PlusButtonCloseAnimation);


						rectangleView.StartAnimation(new ResizeAnimation(rectangleView, new ViewGroup.LayoutParams(DpToPx(100), rectangleView.Height))
						{
							Duration = 1000
						});
					}
				};
			}

			RelativeLayout AddButton = FindViewById<RelativeLayout>(Resource.Id.AddButton);
			{
				AddButton.Click += (object sender, EventArgs e) =>
				{

					Console.WriteLine("Add Button Click");

					Console.WriteLine("{0}, {1}", CenterLocation.Latitude, CenterLocation.Longitude);


					ListActivity.Doc = new Document()
					{
						Latitude = CenterLocation.Latitude,
						Longitude = CenterLocation.Longitude,
						Title = ProjectNameEditText.Text
					};

					StartActivity(typeof(ListActivity));
				};
			}

			ImageButton imageButton1 = FindViewById<ImageButton>(Resource.Id.imageButton1);
			{
				imageButton1.Click += (object sender, EventArgs e) =>
				{
					if (sideView.GetX().Equals(relativeLayout.Width))
					{
						TranslateAnimation SideViewSlideIn = new TranslateAnimation(0, -DpToPx(430), 0, 0)
						{
							Duration = 1000,
							FillEnabled = true
						};
						SideViewSlideIn.SetAnimationListener(new CustomAnimationListener(this, "SideViewSlideIn"));
						sideView.StartAnimation(SideViewSlideIn);

						SideViewIsOpen = true;
					}
					else
					{
						TranslateAnimation SideViewSlideOut = new TranslateAnimation(0, DpToPx(430), 0, 0)
						{
							Duration = 1000,
							FillEnabled = true
						};
						SideViewSlideOut.SetAnimationListener(new CustomAnimationListener(this, "SideViewSlideOut"));
						sideView.StartAnimation(SideViewSlideOut);

						SideViewIsOpen = false;
					}

					imageButton1.SetImageResource(SideViewIsOpen ? Resource.Drawable.icon_note_s : Resource.Drawable.icon_note);
				};
			}

			ImageButton imageButton5 = FindViewById<ImageButton>(Resource.Id.imageButton5);
			{
				imageButton5.Click += (object sender, EventArgs e) =>
				{
					if (!CanvasEnable)
					{
						MapCanvasView mapCanvasView = new MapCanvasView(this, _map);

						mapCanvasView.Id = Resource.Id.map_canvas_view;
						mapCanvasView.LayoutParameters = new ViewGroup.LayoutParams(
							ViewGroup.LayoutParams.MatchParent,
							ViewGroup.LayoutParams.MatchParent
						);

						relativeLayout.AddView(mapCanvasView);

						relativeLayout.FindViewById(Resource.Id.item_bar).BringToFront();
						relativeLayout.FindViewById(Resource.Id.triangle_view).BringToFront();
						relativeLayout.FindViewById(Resource.Id.rectangle_view).BringToFront();
						relativeLayout.FindViewById(Resource.Id.side_view).BringToFront();

						relativeLayout.RequestLayout();

						CanvasEnable = true;
					}
					else
					{
						relativeLayout.RemoveView(FindViewById(Resource.Id.map_canvas_view));

						CanvasEnable = false;
					}

					imageButton5.SetImageResource(CanvasEnable ? Resource.Drawable.icon_pencil_s : Resource.Drawable.icon_pencil);
				};
			}
		}

		public void GridViewInit()
		{
			List<Int32> temp = new List<Int32>();

			for (Int32 i = 0; i < 20; i++)
			{
				temp.Add(i);
			}

			GridView gridview = FindViewById<GridView>(Resource.Id.gridView);

			gridview.Adapter = new CustomAdapter(this, temp, new ViewGroup.LayoutParams(DpToPx(200), DpToPx(200)));

			gridview.ItemClick += (Object sender, AdapterView.ItemClickEventArgs e) =>
			{
				Console.WriteLine(e.Position.ToString());
			};
		}

		public void MapViewInit()
		{
			GoogleMapOptions mapOptions = new GoogleMapOptions()
				.InvokeMapType(GoogleMap.MapTypeNormal)
				.InvokeCamera(new CameraPosition(defaultLocation, 14.0f, 3.0f, 4.0f))
				.InvokeZoomControlsEnabled(true)
				.InvokeCompassEnabled(true);

			_mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

			if (null == _mapFragment)
			{
				FragmentTransaction fragTx = FragmentManager.BeginTransaction();

				_mapFragment = MapFragment.NewInstance(mapOptions);

				fragTx.Add(Resource.Id.mapWithOverlay, _mapFragment, "map");

				fragTx.Commit();
			}

			var mapReadyCallBack = new CustomOnMapReady();

			mapReadyCallBack.MapReady += async (object sender, MapReadyEventArgs e) =>
			{
				var locator = CrossGeolocator.Current;
				locator.DesiredAccuracy = 50;

				var position = await locator.GetPositionAsync(timeoutMilliseconds: 10000);

				defaultLocation = new LatLng(position.Latitude, position.Longitude);

				//Console.WriteLine("Position Status: {0}", position.Timestamp);
				//Console.WriteLine("Position Latitude: {0}", position.Latitude);
				//Console.WriteLine("Position Longitude: {0}", position.Longitude);

				_map = e.Map;
				_map.MyLocationEnabled = true;
				_map.UiSettings.MyLocationButtonEnabled = true;
				_map.SetOnCameraChangeListener(this);
				_map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(defaultLocation, 15.5f));


				//_map.AddMarker(new MarkerOptions().SetPosition(defaultLocation).SetTitle("Test"));

				AddMapMarker();

			};

			_mapFragment.GetMapAsync(mapReadyCallBack);
		}

		public void AddMapMarker() 
		{

			InterViewerService DocumentManager = new InterViewerService();

			List<Document> DocumentList = DocumentManager.GetDocumentsForMap();

			for (Int32 i = 0; i < DocumentList.Count; i++)
			{
				Document Doc = DocumentList[i];

				_map.AddMarker(new MarkerOptions()
				    .SetPosition(new LatLng(Doc.Latitude, Doc.Longitude))
				    .SetIcon(GetBitmapMarker(i.ToString(), 30))
				);
			}

			//_map.AddMarker(new MarkerOptions()
			//	.SetPosition(new LatLng(22.6115547, 120.2912767))
			//	.SetIcon(GetBitmapMarker("9", 30))
			//);

			//_map.AddMarker(new MarkerOptions()
			//	.SetPosition(new LatLng(22.6132832406979, 120.294432342052))
			//	.SetIcon(GetBitmapMarker("5", 30))
			//);	
		}

		public BitmapDescriptor GetBitmapMarker(String text, Int32 size)
		{
			Int32 px = DpToPx(size);

			using (Bitmap bitmap = Bitmap.CreateBitmap(px, px, Bitmap.Config.Argb8888))
			{
				using (Canvas canvas = new Canvas(bitmap))
				{
					using (Paint paint = new Paint())
					{
						View markerView = LayoutInflater.Inflate(Resource.Layout.marker_view_layout, null);

						markerView.LayoutParameters = new ViewGroup.LayoutParams(px, px);

						TextView textView = markerView.FindViewById<TextView>(Resource.Id.marker_count);
						textView.Text = text;

						canvas.DrawBitmap(CreateDrawableFromView(markerView), 0, 0, paint);
					}

				}
				return BitmapDescriptorFactory.FromBitmap(bitmap);
			}
		}

		public Int32 DpToPx(Int32 px)
		{
			return (Int32)TypedValue.ApplyDimension(ComplexUnitType.Dip, px, Resources.DisplayMetrics);
		}

		public Bitmap CreateDrawableFromView(View view)
		{
			Bitmap bitmap = Bitmap.CreateBitmap(view.LayoutParameters.Width, view.LayoutParameters.Height, Bitmap.Config.Argb8888);

			using (Canvas canvas = new Canvas(bitmap))
			{
				view.Measure(
					View.MeasureSpec.MakeMeasureSpec(view.LayoutParameters.Width, MeasureSpecMode.Exactly),
					View.MeasureSpec.MakeMeasureSpec(view.LayoutParameters.Height, MeasureSpecMode.Exactly)
				);
				view.Layout(0, 0, view.MeasuredWidth, view.MeasuredHeight);
				view.Draw(canvas);
			}

			return bitmap;
		}

		#region IOnCameraChangeListener
		public void OnCameraChange(CameraPosition cameraPos)
		{
			CenterLocation = cameraPos.Target;

			Console.WriteLine("{0}, {1}", cameraPos.Target.Latitude, cameraPos.Target.Longitude);
		}
		#endregion

		#region ILocationListener
		public void OnLocationChanged(Location location)
		{

		}

		public void OnProviderDisabled(String provider)
		{

		}

		public void OnProviderEnabled(String provider)
		{

		}

		public void OnStatusChanged(String provider, Availability status, Bundle extras)
		{

		}
		#endregion

		#region MapCallback
		private class CustomOnMapReady : Java.Lang.Object, IOnMapReadyCallback
		{
			public event EventHandler<MapReadyEventArgs> MapReady;

			public void OnMapReady(GoogleMap googleMap)
			{
				EventHandler<MapReadyEventArgs> handle = MapReady;

				if (null != handle && null != googleMap)
				{
					handle(this, new MapReadyEventArgs { Map = googleMap });
				}
			}

		}

		private class MapReadyEventArgs : EventArgs
		{
			public GoogleMap Map { get; set; }
		}

		#endregion

	}

	public class CustomAdapter : BaseAdapter
	{
		private List<Int32> _Source { get; set; }

		private Context _context { get; set; }

		private ViewGroup.LayoutParams _CellLayoutParams { get; set; }

		public CustomAdapter(Context context, List<Int32> Source, ViewGroup.LayoutParams CellLayoutParams)
		{
			_context = context;
			_Source = Source;
			_CellLayoutParams = CellLayoutParams;
		}

		public override Int32 Count
		{
			get { return _Source.Count; }
		}

		public override Java.Lang.Object GetItem(Int32 position)
		{
			return null;
		}

		public override long GetItemId(Int32 position)
		{
			return 0;
		}

		public override View GetView(Int32 position, View convertView, ViewGroup parent)
		{
			TextView textView;

			if (convertView == null)
			{
				textView = new TextView(_context);
				textView.SetBackgroundColor(Color.ParseColor("#008000"));
				textView.LayoutParameters = _CellLayoutParams;
				textView.Gravity = GravityFlags.Center;
			}
			else {
				textView = (TextView)convertView;
			}

			textView.Text = _Source[position].ToString();
			//imageView.SetImageResource(thumbIds[position]);
			return textView;
		}
	}

	public class CustomAnimationListener : Java.Lang.Object, Animation.IAnimationListener
	{
		private Activity _Activity { set; get; }
		private String _EventName { set; get; }

		public CustomAnimationListener(Activity activity, String EventName)
		{
			_Activity = activity;
			_EventName = EventName;
		}


		#region IAnimationListener
		public void OnAnimationEnd(Animation animation)
		{
			Console.WriteLine(_EventName + "Animation");


			switch (_EventName)
			{
				case "PlusButtonOpen":
				case "PlusButtonClose":

					ImageView PlusImageView = _Activity.FindViewById<ImageView>(Resource.Id.PlusImageView);
					{
						PlusImageView.SetImageResource(
							_EventName.Equals("PlusButtonOpen") ? Resource.Drawable.minus : Resource.Drawable.plus
						);
					}
					break;

				case "SideViewSlideIn":
				case "SideViewSlideOut":

					RelativeLayout relativeLayout = _Activity.FindViewById<RelativeLayout>(Resource.Id.test);
					View sideView = _Activity.FindViewById<RelativeLayout>(Resource.Id.side_view);
					{
						sideView.SetX(
							_EventName.Equals("SideViewSlideIn") ? relativeLayout.Width - sideView.Width : relativeLayout.Width
						);
					}
					break;
			}
		}

		public void OnAnimationRepeat(Animation animation)
		{
		}

		public void OnAnimationStart(Animation animation)
		{
		}
		#endregion
	}

	public class ResizeAnimation : Animation
	{
		private View _view;

		private ViewGroup.LayoutParams _startParams { get; set; }
		private ViewGroup.LayoutParams _endParams { get; set; }

		public ResizeAnimation(View v, ViewGroup.LayoutParams endParams)
		{
			_view = v;
			_startParams = v.LayoutParameters;
			_endParams = endParams;
		}

		protected override void ApplyTransformation(float interpolatedTime, Transformation t)
		{
			_view.LayoutParameters.Width = (int)(_startParams.Width + (_endParams.Width - _startParams.Width) * interpolatedTime);
			_view.LayoutParameters.Height = (int)(_startParams.Height + (_endParams.Height - _startParams.Height) * interpolatedTime);
			_view.RequestLayout();
		}

		public override bool WillChangeBounds()
		{
			return true;
		}
	}

	public class TriangleView : View
	{
		public TriangleView(Context context) : base(context) { }

		protected override void OnDraw(Canvas canvas)
		{
			//using (Paint paint = new Paint())
			//{
			//	paint.Color = Color.Blue;
			//	paint.StrokeWidth = 3;

			//	canvas.DrawRect(
			//		(canvas.Width / 2) - DpToPx(20),
			//		(canvas.Height / 2),
			//		(canvas.Width / 2) + DpToPx(20),
			//		(canvas.Height / 2) + DpToPx(40),
			//		paint
			//	);
			//}

			using (Paint paint = new Paint(PaintFlags.AntiAlias))
			{
				paint.Color = Color.White;
				paint.SetStyle(Paint.Style.FillAndStroke);

				Path path = new Path();

				//path.MoveTo(0, 0);
				//path.LineTo(15, 30);
				//path.LineTo(30, 0);
				//path.LineTo(0, 0);
				path.MoveTo(DpToPx(15), 0);
				path.LineTo(0, DpToPx(30));
				path.LineTo(DpToPx(30), DpToPx(30));
				path.LineTo(DpToPx(15), 0);
				path.Close();

				canvas.DrawPath(path, paint);
			}
		}

		public Int32 DpToPx(Int32 px)
		{
			return (Int32)TypedValue.ApplyDimension(ComplexUnitType.Dip, px, Resources.DisplayMetrics);
		}
	}
}


