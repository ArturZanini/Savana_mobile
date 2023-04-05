using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using SistemaColeta.Dialogs;

namespace SistemaColeta.Activities
{
    public class BaseActivity : AppCompatActivity
    {
        #region Public methods
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        #endregion

        #region Protected methods
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        }
        protected void ShowInformationDialog(int resIdTitle, int resIdMessage)
        {
            InformationDialog.NewInstance(GetString(resIdTitle), GetString(resIdMessage)).Show(SupportFragmentManager, "InformationDialog");
        }
        protected void ShowInformationDialog(string title, string message)
        {
            InformationDialog.NewInstance(title, message).Show(SupportFragmentManager, "InformationDialog");
        }
        protected void ShowInformationDialog(int resIdMessage, System.EventHandler<DialogClickEventArgs> positive)
        {
            InformationDialog.NewInstance(null, GetString(resIdMessage), positive).Show(SupportFragmentManager, "InformationDialog");
        }
        protected void ShowInformationDialog(int resIdTitle, int resIdMessage, System.EventHandler<DialogClickEventArgs> positive)
        {
            InformationDialog.NewInstance(GetString(resIdTitle), GetString(resIdMessage), positive).Show(SupportFragmentManager, "InformationDialog");
        }
        protected void ShowConfirmationDialog(int resIdTitle, int resIdMessage, System.EventHandler<DialogClickEventArgs> positive)
        {
            ConfirmationDialog.NewInstance(GetString(resIdTitle), GetString(resIdMessage), positive).Show(SupportFragmentManager, "ConfirmationDialog");
        }
        protected void ShowConfirmationDialog(int resIdTitle, string resIdMessage, System.EventHandler<DialogClickEventArgs> positive)
        {
            ConfirmationDialog.NewInstance(GetString(resIdTitle), resIdMessage, positive).Show(SupportFragmentManager, "ConfirmationDialog");
        }
        protected void ShowConfirmationDialog(int resIdTitle, int resIdMessage, System.EventHandler<DialogClickEventArgs> positive, System.EventHandler<DialogClickEventArgs> negative)
        {
            ConfirmationDialog.NewInstance(GetString(resIdTitle), GetString(resIdMessage), positive, negative).Show(SupportFragmentManager, "ConfirmationDialog");
        }
        protected void ShowLoadingDialog(int resIdMessage)
        {
            LoadingDialog.NewInstance(GetString(resIdMessage)).Show(SupportFragmentManager, "LoadingDialog");
        }
        protected void DismissLoadingDialog()
        {
            LoadingDialog.DismissLoading();
        }
        protected bool CheckNetworkAccess()
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet)
            {
                return true;
            }
            else
            {
                ShowInformationDialog(Resource.String.no_internet_connection, Resource.String.connect_to_a_network);
                return false;
            }
        }
        protected void Connectivity_ConnectivityChanged(object sender, Xamarin.Essentials.ConnectivityChangedEventArgs e)
        {
            var access = e.NetworkAccess;
            var profiles = e.ConnectionProfiles;
        }
        #endregion
    }
}