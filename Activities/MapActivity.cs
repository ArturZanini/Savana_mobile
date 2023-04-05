using Android.App;
using Android.Gms.Maps;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Google.Android.Material.FloatingActionButton;
using SistemaColeta.Map;
using SistemaColeta.Model;
using System.Collections.Generic;
using System.Timers;
using static Android.Views.View;

namespace SistemaColeta.Activities
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
	public class MapActivity: BaseActivity, IOnClickListener
	{
		#region Public fields
		public static bool isTracking;
		#endregion

		#region Private fields
		private static MapActivity _mapActivity;
		private static SistemaColetaMap _SistemaColetaMap;
		private static FloatingActionButton _fabPlay;
		private static FloatingActionButton _fabPause;
		private static Timer _timer;
		#endregion

		#region Protected methods
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_map);

			_mapActivity = this;
			_SistemaColetaMap = new SistemaColetaMap(this);

#pragma warning disable CS0618 // O tipo ou membro é obsoleto
			MapFragment mapFragment = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
			mapFragment.GetMapAsync(_SistemaColetaMap);

			FindViewById<AppCompatImageButton>(Resource.Id.ibImportExport).SetOnClickListener(this);
			FindViewById<AppCompatImageButton>(Resource.Id.ibMapWebView).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnMapTypeSatelliteView).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnMapTypeNormalView).SetOnClickListener(this);

			_fabPlay = FindViewById<FloatingActionButton>(Resource.Id.fabPlay);
			_fabPause = FindViewById<FloatingActionButton>(Resource.Id.fabPause);
			_fabPlay.SetOnClickListener(this);
			_fabPause.SetOnClickListener(this);

			FindViewById<AppCompatTextView>(Resource.Id.tvVersion).Text = $"Versão {Xamarin.Essentials.VersionTracking.CurrentVersion}";
		}
		protected override void OnStop ()
		{
			base.OnStop();

			if(isTracking)
			{
				TrackingStop();
			}
		}
		#endregion

		#region Public methods        
		public void OnClick (View v)
		{
			switch(v.Id)
			{
				case Resource.Id.ibImportExport:
					if(MapActivity.isTracking)
					{
						ShowInformationDialog(GetString(Resource.String.open_course), GetString(Resource.String.end_current_route));

						return;
					}

					StartActivity(typeof(ImportExportActivity));
					FinishAffinity();
					break;
				case Resource.Id.ibMapWebView:
					if(MapActivity.isTracking)
					{
						ShowInformationDialog(GetString(Resource.String.open_course), GetString(Resource.String.end_current_route));

						return;
					}

					StartActivity(typeof(Map2Activity));
					break;
				case Resource.Id.btnMapTypeSatelliteView:
					DataBase.MapView = GoogleMap.MapTypeSatellite;
					
					StartActivity(typeof(MapActivity));
					FinishAffinity();
					break;
				case Resource.Id.btnMapTypeNormalView:
					DataBase.MapView = GoogleMap.MapTypeNormal;
					
					StartActivity(typeof(MapActivity));
					FinishAffinity();
					break;
				case Resource.Id.fabPlay:
					ShowConfirmationDialog(Resource.String.start_route, Resource.String.retrieve_the_previous_route, (s, e) => TrackingStart(true), (s, e) => TrackingStart(false));
					break;
				case Resource.Id.fabPause:
					TrackingStop();
					break;
			}
		}
		#endregion

		#region Private Methods        
		private static void TrackingStart (bool isCache)
		{
			Toast.MakeText(_mapActivity, _mapActivity.GetString(Resource.String.starting_route), ToastLength.Long).Show();
			isTracking = true;

			Tracking tracking = isCache ? DataBase.CachedTracking : new Tracking { Coordinates = new List<double[]>() };
			tracking.Start = System.DateTime.Now;
			DataBase.CachedTracking = tracking;

			_fabPlay.Visibility = ViewStates.Gone;
			_fabPause.Visibility = ViewStates.Visible;

			_timer = new Timer(System.TimeSpan.FromSeconds(1).TotalMilliseconds);
			_timer.Elapsed += _SistemaColetaMap.Tracking;
			_timer.AutoReset = true;
			_timer.Enabled = true;
		}
		private static void TrackingStop ()
		{
			Toast.MakeText(_mapActivity, _mapActivity.GetString(Resource.String.closing_Course), ToastLength.Long).Show();
			isTracking = false;

			Tracking tracking = DataBase.CachedTracking;
			tracking.Stop = System.DateTime.Now;
			DataBase.CachedTracking = tracking;

			_fabPlay.Visibility = ViewStates.Visible;
			_fabPause.Visibility = ViewStates.Gone;

			SaveTracking();

			_timer.Stop();
			_timer.Dispose();
		}
		private static void SaveTracking ()
		{
			try
			{
				Tracking cache = DataBase.CachedTracking;

				Feature feature = new Feature(null);
				if(cache.Coordinates.Count > 1)
				{
					LineString line = new LineString(cache.Coordinates);
					feature = new Feature(line, new
					{
						stroke = RandomColor(),
						Usuário = $"{DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}",
						Start = cache.Start.ToString("dd/MM/yyyy HH:mm:ss"),
						Stop = cache.Stop.ToString("dd/MM/yyyy HH:mm:ss")
					});
				}

				var temp = DataBase.SavedTracking;
				temp.Add(feature);
				DataBase.SavedTracking = temp;

				_mapActivity.StartActivity(typeof(MapActivity));
				_mapActivity.FinishAffinity();
			}
			catch(System.Exception) { }
		}
		private static string RandomColor ()
		{
			System.Random random = new System.Random();
			int r = int.Parse(random.Next(255).ToString());
			int g = int.Parse(random.Next(255).ToString());
			int b = int.Parse(random.Next(255).ToString());
			int a = int.Parse(random.Next(255).ToString());
			string cor = string.Format("#{0}", System.Drawing.Color.FromArgb(a, r, g, b).Name.ToUpper().Substring(0, 6));
			return cor;
		}
		#endregion
	}
}