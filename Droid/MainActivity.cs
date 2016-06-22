using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using Android.Runtime;
using System;
using Android.Gms.Maps.Model;
using Android.Gms.Maps;

namespace InterViewer.Droid
{
	[Activity(Label = "InterViewer", Icon = "@mipmap/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class MainActivity : Activity
	, Android.Locations.ILocationListener
	, Android.Gms.Maps.GoogleMap.IOnMyLocationButtonClickListener

	{
		private MapFragment _mapFragment;
		private GoogleMap _map;
		private Location _currentLocation;

		private bool currentLocationRevelived;
		public string _locationProvider;

		public LocationManager _locationManager;

		private LatLng defaultLocation;


		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			//StartActivity(typeof(DetailActivity));

			// 預設座標
			defaultLocation = new LatLng(23.9737437408, 120.981806398);

			// Google Map
			GoogleMapOptions mapOption = new GoogleMapOptions()
				.InvokeMapType(GoogleMap.MapTypeNormal)
				.InvokeCamera(new CameraPosition(defaultLocation, 14.0f, 3.0f, 4.0f))
				.InvokeZoomControlsEnabled(true)
				.InvokeCompassEnabled(true);

			// MapFragment
			_mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

			if (null == _mapFragment)
			{

				FragmentTransaction fragTx = FragmentManager.BeginTransaction(); //need FrameLayout

				_mapFragment = MapFragment.NewInstance(mapOption);

				fragTx.Add(Resource.Id.mapWithOverlay, _mapFragment, "map");

				fragTx.Commit();
			}

			// MapReady Callback
			var mapReadyCallback = new MyOnMapReady();
			mapReadyCallback.MapReady += (object sender, MapReadyEventArgs e) =>
			{
				_map = e.Map;
				_map.MyLocationEnabled = true;
				_map.UiSettings.MyLocationButtonEnabled = true;
				_map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(defaultLocation, 12.0f));
			};

			_mapFragment.GetMapAsync(mapReadyCallback);
		}



		#region ILocationListener

		public void OnLocationChanged(Location location)
		{
			throw new NotImplementedException();
		}

		public void OnProviderDisabled(string provider)
		{
			throw new NotImplementedException();
		}

		public void OnProviderEnabled(string provider)
		{
			throw new NotImplementedException();
		}

		public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IOnMyLocationButtonClickListener

		public bool OnMyLocationButtonClick()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region MapCallback

		private class MyOnMapReady : Java.Lang.Object, IOnMapReadyCallback
		{
			public event EventHandler<MapReadyEventArgs> MapReady;

			public void OnMapReady(GoogleMap googleMap)
			{
				EventHandler<MapReadyEventArgs> handle = MapReady;

				if (null != handle)
				{

					handle(this, new MapReadyEventArgs()
					{
						Map = googleMap
					});
				}
			}

		}

		private class MapReadyEventArgs : EventArgs
		{
			public GoogleMap Map { get; set; }
		}

		#endregion
	}
}


