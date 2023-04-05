using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using AndroidX.AppCompat.Widget;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SistemaColeta.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class RequirementsActivity : BaseActivity
    {
        #region Private fields
        private AppCompatImageView icon;
        private AppCompatTextView message;
        private AppCompatButton button;

        private static bool _havePermissions;
        private static bool _haveGpsEnabled;
        #endregion

        #region Protected methods
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_requirements);

            icon = FindViewById<AppCompatImageView>(Resource.Id.ivExclamation);
            message = FindViewById<AppCompatTextView>(Resource.Id.tvMessage);
            button = FindViewById<AppCompatButton>(Resource.Id.btnAction);

            if (HaveGpsEnabled())
            {
                await HavePermissions();
            }
        }
        #endregion

        #region Public methods
        protected override void OnResume()
        {
            if (_havePermissions && _haveGpsEnabled)
            {
                StartActivity(typeof(MapActivity));
                Finish();
            }

            base.OnResume();
        }
        #endregion

        #region Private Methods
        private async Task<bool> HavePermissions()
        {
            PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>() != PermissionStatus.Granted ?
                  await Permissions.RequestAsync<Permissions.LocationAlways>() :
                  await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                icon.SetImageDrawable(GetDrawable(Resource.Drawable.ic_warning));
                message.Text = GetString(Resource.String.without_permission);
                button.Text = GetString(Resource.String.reload);
                button.Click += async (s, e) => await HavePermissions();

                _havePermissions = false;
            }
            else
            {
                _havePermissions = true;
            }

            return _havePermissions;
        }
        private bool HaveGpsEnabled()
        {
            LocationManager locationManager = (LocationManager)GetSystemService(Context.LocationService);
            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider) == false)
            {
                icon.SetImageDrawable(GetDrawable(Resource.Drawable.ic_warning));
                message.Text = GetString(Resource.String.gps_disabled);
                button.Text = GetString(Resource.String.setting);
                button.Click += delegate
                {
                    Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                    StartActivity(intent);
                    Finish();
                };

                _haveGpsEnabled = false;
            }
            else
            {
                _haveGpsEnabled = true;
            }

            return _haveGpsEnabled;
        }
        #endregion
    }
}