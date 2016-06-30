using Android.App;
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

using AndroidHUD;

using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Clustering.View;
using System.Linq;

namespace InterViewer.Droid
{

	[Activity(Label = "Test", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Landscape)]

	public class MainActivity : Activity
	, ILocationListener
	, GoogleMap.IOnCameraChangeListener
	{
		private LatLng CenterLocation { get; set; }

		private Boolean CanvasEnable { get; set; } = false;

		private Boolean SideViewIsOpen { get; set; } = false;

		private Boolean _OnCreate { get; set; } = true;

		private List<Document> DocumentList { get; set; }

		private CustomAdapter customAdapter { get; set; }

		private MapFragment _mapFragment;

		private ClusterManager _clusterManager;

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

			Util.context = this;

			defaultLocation = new LatLng(22.6115547, 120.2912767);

			ViewInit();

			MapViewInit();

		}

		protected override void OnResume()
		{
			base.OnResume();

			LoadDocument();

			GridViewInit();

			AddMapMarker();
		}

		public override void OnWindowFocusChanged(bool hasFocus)
		{
			if (_OnCreate)
			{
				RelativeLayout relativeLayout = FindViewById<RelativeLayout>(Resource.Id.test);
				{
					Int32 centerX = relativeLayout.Width / 2;
					Int32 centerY = relativeLayout.Height / 2;

					View triangleView = FindViewById<TriangleView>(Resource.Id.triangle_view);
					{
						triangleView.SetX(centerX - Util.DpToPx(15));
						triangleView.SetY(centerY);
						triangleView.Visibility = ViewStates.Visible;
					}

					View rectangleView = FindViewById<LinearLayout>(Resource.Id.rectangle_view);
					{
						rectangleView.SetX(centerX - (Util.DpToPx(100) / 2));
						rectangleView.SetY(centerY + Util.DpToPx(22));
						rectangleView.Visibility = ViewStates.Visible;
					}

					View sideView = FindViewById<RelativeLayout>(Resource.Id.side_view);
					{
						sideView.SetX(relativeLayout.Width);
						sideView.SetY(0);
						sideView.Visibility = ViewStates.Visible;
					}
				}

				_OnCreate = false;
			}

			base.OnWindowFocusChanged(hasFocus);
		}

		public void ViewInit()
		{
			RelativeLayout relativeLayout = FindViewById<RelativeLayout>(Resource.Id.test);

			TriangleView triangleView = new TriangleView(this);
			{
				triangleView.Id = Resource.Id.triangle_view;
				triangleView.LayoutParameters = new ViewGroup.LayoutParams(Util.DpToPx(100), Util.DpToPx(100));
				triangleView.Visibility = ViewStates.Gone;

				relativeLayout.AddView(triangleView);
			}

			View rectangleView = LayoutInflater.Inflate(Resource.Layout.rectangle_view_layout, null);
			{
				rectangleView.Id = Resource.Id.rectangle_view;
				rectangleView.LayoutParameters = new RelativeLayout.LayoutParams(Util.DpToPx(100), Util.DpToPx(100));
				rectangleView.Visibility = ViewStates.Gone;

				relativeLayout.AddView(rectangleView);
			}

			View sideView = LayoutInflater.Inflate(Resource.Layout.side_view_layout, null);
			{
				sideView.Id = Resource.Id.side_view;
				sideView.LayoutParameters = new ViewGroup.LayoutParams(Util.DpToPx(430), ViewGroup.LayoutParams.MatchParent);
				sideView.Visibility = ViewStates.Gone;

				relativeLayout.AddView(sideView);
			}

			EditText ProjectNameEditText = FindViewById<EditText>(Resource.Id.ProjectNameEditText);

			RelativeLayout PlusButton = FindViewById<RelativeLayout>(Resource.Id.PlusButton);
			{
				PlusButton.Click += (object sender, EventArgs e) =>
				{

					Console.WriteLine("Plus Button Click");

					if (rectangleView.Width == Util.DpToPx(100))
					{
						ProjectNameEditText.Text = "";

						RotateAnimation PlusButtonOpenAnimation = new RotateAnimation(0, 90, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f)
						{
							Duration = 250
						};

						PlusButtonOpenAnimation.SetAnimationListener(new CustomAnimationListener(this, "PlusButtonOpen"));

						PlusButton.StartAnimation(PlusButtonOpenAnimation);


						rectangleView.StartAnimation(new ResizeAnimation(rectangleView, new ViewGroup.LayoutParams(Util.DpToPx(400), rectangleView.Height))
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


						rectangleView.StartAnimation(new ResizeAnimation(rectangleView, new ViewGroup.LayoutParams(Util.DpToPx(100), rectangleView.Height))
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

					rectangleView.LayoutParameters = new RelativeLayout.LayoutParams(Util.DpToPx(100), Util.DpToPx(100));

					ImageView PlusImageView = rectangleView.FindViewById<ImageView>(Resource.Id.PlusImageView);
					{
						PlusImageView.SetImageResource(Resource.Drawable.plus);
					}				
				};
			}

			ImageButton imageButton1 = FindViewById<ImageButton>(Resource.Id.imageButton1);
			{
				imageButton1.Click += (object sender, EventArgs e) =>
				{
					if (!SideViewIsOpen)
					{
						TranslateAnimation SideViewSlideIn = new TranslateAnimation(0, -Util.DpToPx(430), 0, 0)
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
						TranslateAnimation SideViewSlideOut = new TranslateAnimation(0, Util.DpToPx(430), 0, 0)
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

		public void LoadDocument()
		{
			InterViewerService DocumentManager = new InterViewerService(new IOService());

			if (null != CenterLocation)
			{
				DocumentList = DocumentManager.GetDocumentsOrderBy(CenterLocation.Latitude, CenterLocation.Longitude);
			}
			else 
			{
				DocumentList = DocumentManager.GetDocuments();
			}

			if (customAdapter != null)
			{
				customAdapter.UpdateData(DocumentList);

				customAdapter.NotifyDataSetChanged();
			}
		}

		public void GridViewInit()
		{

			GridView gridview = FindViewById<GridView>(Resource.Id.gridView);

			customAdapter = new CustomAdapter(this, DocumentList, new ViewGroup.LayoutParams(Util.DpToPx(200), Util.DpToPx(200)));

			gridview.Adapter = customAdapter;

			gridview.ItemClick += (Object sender, AdapterView.ItemClickEventArgs e) =>
			{
				//Console.WriteLine(e.Position.ToString());

				DetailActivity.PDF_Type = "Edit";
				DetailActivity.Doc = DocumentList[e.Position];

				StartActivity(typeof(DetailActivity));
			};
		}

		public async void MapViewInit()
		{
			RunOnUiThread(() =>
			{
				AndHUD.Shared.Show(this, "", -1, MaskType.Clear);
			});

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

			mapReadyCallBack.MapReady += (object sender, MapReadyEventArgs e) =>
			{

				_map = e.Map;
				_map.MyLocationEnabled = true;
				_map.UiSettings.MyLocationButtonEnabled = true;
				_map.SetOnCameraChangeListener(this);
				_map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(defaultLocation, 15.5f));

				_clusterManager = new ClusterManager(this, _map);

				_clusterManager.SetRenderer(new CustomClusterRenderer(this, _map, _clusterManager));

				//_map.SetOnCameraChangeListener(_clusterManager);

				LoadDocument();

				AddMapMarker();

				RunOnUiThread(() =>
				{
				    AndHUD.Shared.Dismiss(this);
				});
			};

			var locator = CrossGeolocator.Current;

			locator.DesiredAccuracy = 50;

			var position = await locator.GetPositionAsync(timeoutMilliseconds: 20000);

			CenterLocation = defaultLocation = new LatLng(position.Latitude, position.Longitude);

			Console.WriteLine("Position Status: {0}", position.Timestamp);
			Console.WriteLine("Position Latitude: {0}", position.Latitude);
			Console.WriteLine("Position Longitude: {0}", position.Longitude);

			_mapFragment.GetMapAsync(mapReadyCallBack);
		}

		public void AddMapMarker()
		{
			if (null != _map)
			{
				_clusterManager.ClearItems();

				List<IClusterItem> items = new List<IClusterItem>();

				for (Int32 i = 0; i < DocumentList.Count; i++)
				{
					Document Doc = DocumentList[i];

					//_map.AddMarker(new MarkerOptions()
					//	.SetPosition(new LatLng(Doc.Latitude, Doc.Longitude))
					//	.SetIcon(GetBitmapMarker(i.ToString(), 30))
					//);

					items.Add(new ClusterItem()
					{
						Position = new LatLng(Doc.Latitude, Doc.Longitude)
					});
				}

				//Random rnd = new Random();
				//for (int i = 1; i <= 500; i++)
				//{
				//	double lat = CenterLocation.Latitude + rnd.NextDouble();
				//	double lon = CenterLocation.Longitude + rnd.NextDouble();

				//	items.Add(new ClusterItem()
				//	{
				//		Position = new LatLng(lat, lon)
				//	});
				//}

				_clusterManager.AddItems(items);

				_clusterManager.Cluster();
			}
		}

		#region IOnCameraChangeListener
		public void OnCameraChange(CameraPosition cameraPos)
		{
			CenterLocation = cameraPos.Target;

			//Console.WriteLine("{0}, {1}", cameraPos.Target.Latitude, cameraPos.Target.Longitude);

			_clusterManager.OnCameraChange(cameraPos);

			DocumentList = DocumentList.OrderBy(doc => doc.GetDistance(CenterLocation.Latitude, CenterLocation.Longitude)).ToList();

			customAdapter.UpdateData(DocumentList);

			customAdapter.NotifyDataSetChanged();

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
		private List<Document> _Source { get; set; }

		private Context _context { get; set; }

		private ViewGroup.LayoutParams _CellLayoutParams { get; set; }

		public CustomAdapter(Context context, List<Document> Source, ViewGroup.LayoutParams CellLayoutParams)
		{
			_context = context;
			_Source = Source;
			_CellLayoutParams = CellLayoutParams;
		}

		public void UpdateData(List<Document> Source)
		{
			_Source = Source;
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
			return position;
		}

		public class Holder
		{
			public TextView textView;
			public ImageView imageView;
		}

		public override View GetView(Int32 position, View convertView, ViewGroup parent)
		{
			Document doc = _Source[position];

			View view;

			Holder holder = new Holder();

			if (convertView == null)
			{
				view = LayoutInflater.From(_context).Inflate(Resource.Layout.side_view_cell_layout, null);

				view.LayoutParameters = _CellLayoutParams;
			}
			else 
			{
				view = (View)convertView;
			}

			holder.textView = view.FindViewById<TextView>(Resource.Id.doc_title);
			holder.imageView = view.FindViewById<ImageView>(Resource.Id.doc_image_view);

			holder.imageView.SetImageBitmap(
				BitmapFactory.DecodeFile(doc.Thumbnail, GetBitmapOptions(4))
			);

			holder.textView.Text = doc.Title;
	
			return view;
		}

		public BitmapFactory.Options GetBitmapOptions(Int32 scale)
		{
			BitmapFactory.Options options = new BitmapFactory.Options();
			options.InPurgeable = true;
			options.InInputShareable = true;
			options.InSampleSize = scale;
			return options;
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
				path.MoveTo(Util.DpToPx(15), 0);
				path.LineTo(0, Util.DpToPx(30));
				path.LineTo(Util.DpToPx(30), Util.DpToPx(30));
				path.LineTo(Util.DpToPx(15), 0);
				path.Close();

				canvas.DrawPath(path, paint);
			}
		}
	}

	public class ClusterItem : Java.Lang.Object, IClusterItem
	{
		public LatLng Position { get; set; }

		public ClusterItem()
		{
		}

		public ClusterItem(double lat, double lng)
		{
			Position = new LatLng(lat, lng);
		}
	}

	public class CustomClusterRenderer : DefaultClusterRenderer
	{
		private Context _context { get; set; }

		public CustomClusterRenderer(Context context, GoogleMap map, ClusterManager clusterManager) : base(context, map, clusterManager)
		{
			_context = context;
		}

		protected override void OnBeforeClusterItemRendered(Java.Lang.Object item, MarkerOptions markerOptions)
		{
			markerOptions.SetIcon(GetBitmapMarker("1", 30));
		}

		protected override void OnBeforeClusterRendered(ICluster cluster, MarkerOptions markerOptions)
		{
			markerOptions.SetIcon(GetBitmapMarker(cluster.Size.ToString(), 30));
		}

		protected override bool ShouldRenderAsCluster(ICluster cluster)
		{
			return cluster.Size > 1;
		}

		public BitmapDescriptor GetBitmapMarker(String text, Int32 size)
		{
			Int32 px = Util.DpToPx(size);

			using (Bitmap bitmap = Bitmap.CreateBitmap(px, px, Bitmap.Config.Argb8888))
			{
				using (Canvas canvas = new Canvas(bitmap))
				{
					using (Paint paint = new Paint())
					{
						View markerView = LayoutInflater.From(_context).Inflate(Resource.Layout.marker_view_layout, null);

						markerView.LayoutParameters = new ViewGroup.LayoutParams(px, px);

						TextView textView = markerView.FindViewById<TextView>(Resource.Id.marker_count);
						textView.Text = text;

						canvas.DrawBitmap(CreateDrawableFromView(markerView), 0, 0, paint);
					}

				}
				return BitmapDescriptorFactory.FromBitmap(bitmap);
			}
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
	}

	public static class Util 
	{
		public static Context context { get; set; }

		public static Int32 DpToPx(Int32 px)
		{
			return (Int32)TypedValue.ApplyDimension(ComplexUnitType.Dip, px, context.Resources.DisplayMetrics);
		}
	}

}


