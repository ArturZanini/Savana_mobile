using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Com.Google.Maps.Android.Data.Geojson;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Java.Lang;
using Newtonsoft.Json;
using Org.Json;
using SistemaColeta.Activities;
using SistemaColeta.Dialogs;
using SistemaColeta.Model;
using System.Linq;
using System.Timers;
using Xamarin.Essentials;
using static Android.Gms.Maps.GoogleMap;
using Point = GeoJSON.Net.Geometry.Point;

namespace SistemaColeta.Map
{
	public class SistemaColetaMap: Java.Lang.Object, IOnMapReadyCallback, IInfoWindowAdapter, IOnInfoWindowClickListener, IOnMapLongClickListener
	{
		#region Private fields
		private readonly MapActivity _mapActivity;
		private static GoogleMap _googleMap;
		#endregion

		#region Builders
		public SistemaColetaMap (MapActivity Context)
		{
			_mapActivity = Context;
		}
		#endregion

		
		public View GetInfoContents (Marker marker)
		{
			View view = LayoutInflater.FromContext(_mapActivity).Inflate(Resource.Layout.layout_marker_info, null, false);
			AppCompatTextView tvTitle = view.FindViewById<AppCompatTextView>(Resource.Id.tvTitle);

			tvTitle.Text = $"Ponto de coleta: {marker.Title}";

			return view;
		}
		public View GetInfoWindow (Marker marker) => null;
		public void OnInfoWindowClick (Marker marker)
		{
			if(MapActivity.isTracking)
			{
				InformationDialog.NewInstance(_mapActivity.GetString(Resource.String.open_course), _mapActivity.GetString(Resource.String.end_current_route))
					.Show(_mapActivity.SupportFragmentManager, "InformationDialog");

				return;
			}

			Intent intent = new Intent(_mapActivity, typeof(CollectPointActivity));
			intent.PutExtra("Id", marker.Title);
			intent.PutExtra("Latitude", $"{marker.Position.Latitude}");
			intent.PutExtra("Longitude", $"{marker.Position.Longitude}");
			_mapActivity.StartActivity(intent);
		}
		public void OnMapLongClick (LatLng point)
		{
			try
			{
				FeatureCollection featureCollection = DataBase.GeoJsonPoint;
				Point newPoint = new Point(new Position(point.Latitude, point.Longitude));
				SavePoint.NewInstance(_googleMap, featureCollection, newPoint).Show(_mapActivity.SupportFragmentManager, "SavePoint");
			}
			catch(System.Exception ex)
			{
				Toast.MakeText(_mapActivity, ex.Message, ToastLength.Long).Show();
			}
		}
		public async void Tracking (object sender, ElapsedEventArgs e)
		{
			Location location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
			lock(this)
			{
				try
				{
					if(location != null)
					{
						Tracking tracking = DataBase.CachedTracking;

						tracking.Coordinates.Add(new double[]
						{
							location.Longitude, location.Latitude
						});

						if(tracking.Coordinates.Count > 1)
						{
							LineString line = new LineString(tracking.Coordinates);
							string json = JsonConvert.SerializeObject(line);

							GeoJsonLayer layer = new GeoJsonLayer(_googleMap, new JSONObject(json));
							MainThread.BeginInvokeOnMainThread(() =>
							{
								layer.AddLayerToMap();
							});
						}

						DataBase.CachedTracking = tracking;
					}
				}
				catch(System.Exception ex)
				{
					Toast.MakeText(_mapActivity, ex.Message, ToastLength.Long).Show();
				}
			}
		}
		#endregion

		#region Private methods
		public static void LoadGeoJsonOnMap (GoogleMap googleMap, string argJson)
		{
			try
			{
				GeoJsonLayer layer = new GeoJsonLayer(googleMap, new JSONObject(argJson));

				foreach(GeoJsonFeature feature in layer.Features.ToEnumerable())
				{
					switch(feature.Geometry.GeometryType)
					{
						case "MultiLineString":
						case "LineString":
							{
								float zIndex = 1;
								int color = Color.Black;
								float width = 5;

								if(feature.HasProperty("z-index"))
								{
									zIndex = Float.ParseFloat(feature.GetProperty("z-index"));
								}
								if(feature.HasProperty("stroke"))
								{
									color = Color.ParseColor(feature.GetProperty("stroke"));
								}
								if(feature.HasProperty("stroke-width"))
								{
									width = Float.ParseFloat(feature.GetProperty("stroke-width"));
								}
								if(feature.HasProperty("stroke-opacity"))
								{
									color = Others.Utilities.AdjustAlpha(color, Float.ParseFloat(feature.GetProperty("stroke-opacity")));
								}

								feature.LineStringStyle = new GeoJsonLineStringStyle()
								{
									ZIndex = zIndex,
									Color = color,
									Width = width
								};
							}
							break;
						case "MultiPolygon":
						case "Polygon":
							{
								float zIndex = 1;
								int strokeColor = Color.Black;
								float strokeWidth = 5;
								int fillColor = Color.Transparent;

								if(feature.HasProperty("z-index"))
								{
									zIndex = Float.ParseFloat(feature.GetProperty("z-index"));
								}
								if(feature.HasProperty("stroke"))
								{
									strokeColor = Color.ParseColor(feature.GetProperty("stroke"));
								}
								if(feature.HasProperty("stroke-width"))
								{
									strokeWidth = Float.ParseFloat(feature.GetProperty("stroke-width"));
								}
								if(feature.HasProperty("stroke-opacity"))
								{
									strokeColor = Others.Utilities.AdjustAlpha(strokeColor, Float.ParseFloat(feature.GetProperty("stroke-opacity")));
								}
								if(feature.HasProperty("fill"))
								{
									fillColor = Color.ParseColor(feature.GetProperty("fill"));
								}
								if(feature.HasProperty("fill-opacity"))
								{
									fillColor = Others.Utilities.AdjustAlpha(fillColor, Float.ParseFloat(feature.GetProperty("fill-opacity")));
								}

								feature.PolygonStyle = new GeoJsonPolygonStyle
								{
									ZIndex = zIndex,
									StrokeColor = strokeColor,
									StrokeWidth = strokeWidth,
									FillColor = fillColor
								};
							}
							break;
						case "Point":
							{
								string id = string.Empty;
								bool WasColetado = false;

								if(feature.HasProperty("Id"))
								{
									id = feature.GetProperty("Id");
								}
								if(feature.HasProperty("Coletado"))
								{
									WasColetado = System.Convert.ToBoolean(feature.GetProperty("Coletado"));
								}

								feature.PointStyle = new GeoJsonPointStyle()
								{
									Title = id,
									Icon = WasColetado ? BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen) : BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed)
								};
							}
							break;
					}
				}

				MainThread.InvokeOnMainThreadAsync(() =>
				{
					layer.AddLayerToMap();
				});
			}
			catch(System.Exception ex)
			{

			}
		}
		#endregion
	}
}