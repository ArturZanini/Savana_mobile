using GeoJSON.Net.Feature;
using MonkeyCache.SQLite;
using SistemaColeta.Model;
using System.Collections.Generic;

namespace SistemaColeta
{
	public static class DataBase
	{
		public static int MapView
		{
			get { return Barrel.Current.Get<int>(Constants.MapView) == 0 ? 1 : Barrel.Current.Get<int>(Constants.MapView); }
			set { Barrel.Current.Add<int>(Constants.MapView, value, System.TimeSpan.FromMilliseconds(-1)); }
		}

		public static List<User> LoginAdmin
		{
			get
			{
				return new List<User>() {
					new User(){ Id = Constants.AdminId,  Name="Savana", Password="savana01" },
					new User(){ Id = Constants.AdminId,    Name="Campo01", Password="savana01" },
					new User(){ Id = Constants.AdminId,  Name="Campo02", Password="savana01" },
					new User(){ Id = Constants.AdminId,  Name="Master", Password="Savana@22" }
				};
			}
		}

		public static List<User> Login
		{
			get { return Barrel.Current.Get<List<User>>(Constants.Login) ?? new List<User>(); }
			set { Barrel.Current.Add<List<User>>(Constants.Login, value, System.TimeSpan.FromMilliseconds(-1)); }
		}

		public static User CurrentUser
		{
			get { return Barrel.Current.Get<User>(Constants.CurrentUser) ?? new User(); }
			set { Barrel.Current.Add<User>(Constants.CurrentUser, value, System.TimeSpan.FromMilliseconds(-1)); }
		}

		public static FeatureCollection GeoJsonArea
		{
			get { return Barrel.Current.Get<FeatureCollection>(Constants.GeoJsonArea) ?? new FeatureCollection(); }
			set { Barrel.Current.Add<FeatureCollection>(Constants.GeoJsonArea, value, System.TimeSpan.FromMilliseconds(-1)); }
		}
		public static List<ImageOverlay> ImageOverlay
		{
			get { return Barrel.Current.Get<List<ImageOverlay>>(Constants.ImageOverlay) ?? new List<ImageOverlay>(); }
			set { Barrel.Current.Add<List<ImageOverlay>>(Constants.ImageOverlay, value, System.TimeSpan.FromMilliseconds(-1)); }
		}
		public static FeatureCollection GeoJsonPoint
		{
			get { return Barrel.Current.Get<FeatureCollection>(Constants.GeoJsonPoint) ?? new FeatureCollection(); }
			set { Barrel.Current.Add<FeatureCollection>(Constants.GeoJsonPoint, value, System.TimeSpan.FromMilliseconds(-1)); }
		}
		public static List<Feature> SavedTracking
		{
			get { return Barrel.Current.Get<List<Feature>>(Constants.SavedTracking) ?? new List<Feature>(); }
			set { Barrel.Current.Add<List<Feature>>(Constants.SavedTracking, value, System.TimeSpan.FromMilliseconds(-1)); }
		}
		public static Tracking CachedTracking
		{
			get { return Barrel.Current.Get<Tracking>(Constants.CachedTracking) ?? new Tracking { Coordinates = new List<double[]>() }; }
			set { Barrel.Current.Add<Tracking>(Constants.CachedTracking, value, System.TimeSpan.FromMilliseconds(-1)); }
		}

		public static List<CollectPoint> SavedPoints => Barrel.Current.Get<List<CollectPoint>>(Constants.SavedPoint) ?? new List<CollectPoint>();
		public static void SetSavedPoint (CollectPoint obj)
		{
			if(obj is null) return;
			if(obj.IdPoint == 0) return;

			var savedPoints = DataBase.CachedPoints;
			if(savedPoints.Exists(x => x.IdPoint == obj.IdPoint))
			{
				savedPoints.RemoveAll(x => x.IdPoint == obj.IdPoint);
			}

			savedPoints.Add(obj);

			Barrel.Current.Add<List<CollectPoint>>(Constants.SavedPoint, savedPoints, System.TimeSpan.FromMilliseconds(-1));
		}
		public static void SavedPointRemoveItem (CollectPoint obj)
		{
			if(obj is null) return;
			if(obj.IdPoint == 0) return;

			List<CollectPoint> savedPoints = DataBase.SavedPoints;
			savedPoints.RemoveAll(x => x.IdPoint == obj.IdPoint);

			Barrel.Current.Add<List<CollectPoint>>(Constants.SavedPoint, savedPoints, System.TimeSpan.FromMilliseconds(-1));
		}

		public static List<CollectPoint> CachedPoints => Barrel.Current.Get<List<CollectPoint>>(Constants.CachedPoints) ?? new List<CollectPoint>();
		public static void SetCachedPoint (CollectPoint obj)
		{
			if(obj is null) return;
			if(obj.IdPoint == 0) return;

			var cachedPoints = DataBase.CachedPoints;
			if(cachedPoints.Exists(x => x.IdPoint == obj.IdPoint))
			{
				cachedPoints.RemoveAll(x => x.IdPoint == obj.IdPoint);
			}

			cachedPoints.Add(obj);

			Barrel.Current.Add<List<CollectPoint>>(Constants.CachedPoints, cachedPoints, System.TimeSpan.FromMilliseconds(-1));
		}
		public static void CachedPointsRemoveItem (CollectPoint obj)
		{
			if(obj is null) return;
			if(obj.IdPoint == 0) return;

			List<CollectPoint> savedPoints = DataBase.SavedPoints;
			savedPoints.RemoveAll(x => x.IdPoint == obj.IdPoint);

			Barrel.Current.Add<List<CollectPoint>>(Constants.CachedPoints, savedPoints, System.TimeSpan.FromMilliseconds(-1));
		}

		public static void ClearUsers ()
		{
			Barrel.Current.Empty(Constants.Login);
		}
		public static void ClearAreas ()
		{
			Barrel.Current.Empty(Constants.GeoJsonArea);
			Barrel.Current.Empty(Constants.ImageOverlay);
		}
		public static void ClearPoints ()
		{
			DataBase.ClearCollectedData();
			Barrel.Current.Empty(Constants.GeoJsonPoint);
		}
		public static void ClearCollectedData ()
		{
			Barrel.Current.Empty(Constants.CachedPoints);
			Barrel.Current.Empty(Constants.SavedPoint);

			var featureCollection = DataBase.GeoJsonPoint;
			foreach(var item in featureCollection.Features)
			{
				item.Properties["Coletado"] = false;
			}
			DataBase.GeoJsonPoint = featureCollection;
		}
		public static void ClearTracking ()
		{
			Barrel.Current.Empty(Constants.CachedTracking);
			Barrel.Current.Empty(Constants.SavedTracking);
		}
	}
}
