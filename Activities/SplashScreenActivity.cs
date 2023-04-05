using Android.App;
using Android.OS;
using MonkeyCache.SQLite;
using Newtonsoft.Json;
using SistemaColeta.Model;
using System;
using System.IO;

namespace SistemaColeta.Activities
{
    [Activity(MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class SplashScreenActivity : BaseActivity, Java.Lang.IRunnable
    {
        #region Protected methods
        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_splash_screen);

            Barrel.ApplicationId = Constants.ApplicationId;
            DataBase.CurrentUser = new User();

            //Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            new Handler().PostDelayed(this, 1000);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string path = GetExternalMediaDirs()[0].Path;

            if (!File.Exists(path)) Directory.CreateDirectory(path);

            using (StreamWriter writer = new StreamWriter(path: $"{path}/debuglog.txt", append: true))
            {
                writer.Write(JsonConvert.SerializeObject(new
                {
                    Version = Xamarin.Essentials.VersionTracking.CurrentVersion,
                    Date = DateTime.Now,
                    Exception = e
                }, Formatting.Indented));
            }
        }
        #endregion

        #region Public methods
        public void Run()
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }
        #endregion        
    }
}