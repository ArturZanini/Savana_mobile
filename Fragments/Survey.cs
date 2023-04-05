using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Android.Webkit;

namespace SistemaColeta.Fragments
{
    public class Survey : Fragment
    {
        #region Private fields
        private View _view;
        #endregion

        #region Public methods
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = inflater.Inflate(Resource.Layout.layout_survey, container, false);

            WebView webView = _view.FindViewById<WebView>(Resource.Id.webViewSistema);

            webView.LoadUrl("https://www.gaussgeo.com.br/");

            webView.Settings.JavaScriptEnabled = true;
            webView.SetWebViewClient(new WebViewClient());

            return _view;
        }
        #endregion
    }
}