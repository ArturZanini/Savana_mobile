using Android.App;
using Android.Content;
using Android.Locations;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextField;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SistemaColeta.Activities
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
	public class LoginActivity: BaseActivity
	{
		#region Private fields
		private TextInputLayout _tilUser, _tilPassword;
		#endregion

		#region Protected methods
		protected override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_login);

			_tilUser = FindViewById<TextInputLayout>(Resource.Id.tilUser);
			_tilPassword = FindViewById<TextInputLayout>(Resource.Id.tilPassword);

			FindViewById<AppCompatButton>(Resource.Id.btnLogin).Click += Login;
		}
		#endregion

		#region Private methods
		private async void Login (object sender, System.EventArgs e)
		{
			if(ValidateData())
			{
				try
				{
					ShowLoadingDialog(Resource.String.loading);

					Model.User user;
					List<Model.User> usersAdmin = await Task.Run(() => DataBase.LoginAdmin);
					user = usersAdmin.Find(x =>
						x.Name == _tilUser.EditText.Text &&
						x.Password == _tilPassword.EditText.Text
					);

					if(user == null)
					{
						List<Model.User> users = await Task.Run(() => DataBase.Login);
						user = users.Find(x =>
							x.Name == _tilUser.EditText.Text &&
							x.Password == _tilPassword.EditText.Text
						);

						if(user == null)
						{
							ShowInformationDialog(Resource.String.unable_to_login, Resource.String.username_or_password_is_invalid);
							DismissLoadingDialog();
							return;
						}
					}

					DataBase.CurrentUser = user;
					if(await HavePermissions() && HaveGpsEnabled())
					{
						StartActivity(typeof(MapActivity));
						FinishAffinity();
					}
					else
					{
						StartActivity(typeof(RequirementsActivity));
						FinishAffinity();
					}
				}
				catch(System.Exception ex)
				{
					Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
				}
			}
		}
		private bool ValidateData ()
		{
			bool user = new Others.Validation(this).ValidateUser(_tilUser);
			bool password = DataBase.LoginAdmin.Any(x => x.Name == _tilPassword.EditText.Text) || new Others.Validation(this).ValidatePassword(_tilPassword);

			return (user && password);
		}
		private async Task<bool> HavePermissions ()
		{
			PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>() != PermissionStatus.Granted ?
				  await Permissions.RequestAsync<Permissions.LocationAlways>() :
				  await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

			return permissionStatus == PermissionStatus.Granted;
		}
		private bool HaveGpsEnabled ()
		{
			LocationManager locationManager = (LocationManager)GetSystemService(Context.LocationService);

			return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
		}
		#endregion
	}
}