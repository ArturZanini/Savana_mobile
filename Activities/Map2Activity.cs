using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;
using AndroidX.AppCompat.Widget;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace SistemaColeta.Activities
{
    [Activity(Label = "@string/auxiliary_map", ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class Map2Activity : BaseActivity
    {
        #region Private fields
        private WebView web_view;
        #endregion

        #region Protected methods
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_map2);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            InitWebView();
        }
        #endregion

        #region Public methods        
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
        public override bool OnKeyDown(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
        {
            if (keyCode == Keycode.Back && web_view.CanGoBack())
            {
                web_view.GoBack();
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
        #endregion

        #region Private Methods        
        private async void InitWebView()
        {
            string path = await Task.Run(() => $"{GetExternalMediaDirs()[0].Path}/webmaps_savana/index.html");
            if (System.IO.File.Exists(path))
            {
                FindViewById<AppCompatTextView>(Resource.Id.tvMessage).Visibility = ViewStates.Gone;
            }
            else
            {
                FindViewById<AppCompatTextView>(Resource.Id.tvMessage).Visibility = ViewStates.Visible;
                return;
            }

            web_view = FindViewById<WebView>(Resource.Id.webview);
            web_view.Settings.JavaScriptEnabled = true;
            web_view.SetWebChromeClient(new MapWebChromeClient());
            web_view.LoadUrl($"file://{path}");
        }
        #endregion
    }
    public class MapWebChromeClient : WebChromeClient
    {
        public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback)
        {
            callback.Invoke(origin, true, true);

            base.OnGeolocationPermissionsShowPrompt(origin, callback);
        }
    }
}