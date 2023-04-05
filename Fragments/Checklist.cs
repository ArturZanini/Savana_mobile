using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using AndroidX.Fragment.App;
using GeoJSON.Net.Feature;
using Google.Android.Material.TextField;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SistemaColeta.Activities;
using SistemaColeta.Dialogs;
using SistemaColeta.Model;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;

namespace SistemaColeta.Fragments
{
    public class Checklist : Fragment, View.IOnClickListener, RadioGroup.IOnCheckedChangeListener
    {
        #region Private fields
        private int CurrentId => System.Convert.ToInt32(Activity.Intent.GetStringExtra("Id"));
        private string CurrentLocation => $"{Activity.Intent.GetStringExtra("Latitude")}, {Activity.Intent.GetStringExtra("Longitude")}";
        private static CollectPoint _collectPoint;

        private static Timer _timer;

        private View _view;
        private static View _btnImage;
        private static TextInputLayout _tilField, _tilDate, _tilVariety, _tilStand, _tilPhenologicalStadium, _tilObservations;
        private static TextInputLayout _tilObservationsRootSystem, _tilObservationsAerialPart, _tilObservationsPlantability, _tilObservationsCompaction, _tilObservationsNematodes, _tilObservationsOthers;
        private AppCompatImageButton _ibRootSystem1, _ibRootSystem2, _ibRootSystem3, _ibRootSystem4, _ibRootSystem5;
        private AppCompatImageButton _ibAerialPart1, _ibAerialPart2, _ibAerialPart3, _ibAerialPart4, _ibAerialPart5;
        private AppCompatImageButton _ibPlantability1, _ibPlantability2, _ibPlantability3, _ibPlantability4, _ibPlantability5;
        private AppCompatImageButton _ibCompaction1, _ibCompaction2, _ibCompaction3, _ibCompaction4, _ibCompaction5;
        private AppCompatImageButton _ibNematodes1, _ibNematodes2, _ibNematodes3, _ibNematodes4, _ibNematodes5;
        private AppCompatImageButton _ibOthers1, _ibOthers2, _ibOthers3, _ibOthers4, _ibOthers5;
        private RadioGroup _rgRootSystem, _rgAerialPart, _rgPlantability, _rgCompaction, _rgNematodes, _rgOthers;
        #endregion

        #region Public methods
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = inflater.Inflate(Resource.Layout.layout_checklist, container, false);
            _view.FindViewById<AppCompatTextView>(Resource.Id.tvCoordinates).Text = CurrentLocation;
            _collectPoint = new CollectPoint
            {
                IdPoint = CurrentId,
                Location = CurrentLocation
            };

            LoadImageButtons();
            LoadTextInputs();
            LoadRadioGroups();

            _view.FindViewById<AppCompatButton>(Resource.Id.btnSave).Click += SavePoint;

            return _view;
        }
        public override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                LoadingDialog.NewInstance(GetString(Resource.String.loading)).Show(Activity.SupportFragmentManager, "LoadingDialog");
                await Task.Run(() => LoadFields());
            }
            finally
            {
                LoadingDialog.DismissLoading();
            }
        }
        public void OnClick(View v)
        {
            _view.FindViewById<NestedScrollView>(Resource.Id.nsvLocation).RequestFocus();

            var temp = GetPhoto(v);
            if (temp != null)
            {
                PreviewPhoto.NewInstance(temp,
                    (s, e) => TakePhoto(_btnImage),
                    (s, e) => RemovePhoto(_btnImage))
                    .Show(Activity.SupportFragmentManager, "PreviewPhoto");

                _btnImage = v;
            }
            else
            {
                TakePhoto(v);
            }
        }
        public void OnCheckedChanged(RadioGroup group, int checkedId)
        {
            _view.FindViewById<RadioGroup>(group.Id).RequestFocus();
            switch (group.Id)
            {
                case Resource.Id.rgRootSystem:
                    _collectPoint.RootSystemRating = System.Convert.ToInt32(_view.FindViewById<RadioButton>(checkedId).Text);
                    break;
                case Resource.Id.rgAerialPart:
                    _collectPoint.AerialPartRating = System.Convert.ToInt32(_view.FindViewById<RadioButton>(checkedId).Text);
                    break;
                case Resource.Id.rgPlantability:
                    _collectPoint.PlantabilityRating = System.Convert.ToInt32(_view.FindViewById<RadioButton>(checkedId).Text);
                    break;
                case Resource.Id.rgCompaction:
                    _collectPoint.CompactionRating = System.Convert.ToInt32(_view.FindViewById<RadioButton>(checkedId).Text);
                    break;
                case Resource.Id.rgNematodes:
                    _collectPoint.NematodesRating = System.Convert.ToInt32(_view.FindViewById<RadioButton>(checkedId).Text);
                    break;
                case Resource.Id.rgOthers:
                    _collectPoint.OthersRating = System.Convert.ToInt32(_view.FindViewById<RadioButton>(checkedId).Text);
                    break;
            }
        }
        public override void OnResume()
        {
            base.OnResume();

            TimerStart();
        }
        public override void OnStop()
        {
            base.OnStop();

            TimerStop();
        }
        #endregion

        #region Private Methods
        private void LoadImageButtons()
        {
            _ibRootSystem1 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibRootSystem1);
            _ibRootSystem2 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibRootSystem2);
            _ibRootSystem3 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibRootSystem3);
            _ibRootSystem4 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibRootSystem4);
            _ibRootSystem5 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibRootSystem5);

            _ibRootSystem1.SetOnClickListener(this);
            _ibRootSystem2.SetOnClickListener(this);
            _ibRootSystem3.SetOnClickListener(this);
            _ibRootSystem4.SetOnClickListener(this);
            _ibRootSystem5.SetOnClickListener(this);

            _ibAerialPart1 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibAerialPart1);
            _ibAerialPart2 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibAerialPart2);
            _ibAerialPart3 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibAerialPart3);
            _ibAerialPart4 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibAerialPart4);
            _ibAerialPart5 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibAerialPart5);

            _ibAerialPart1.SetOnClickListener(this);
            _ibAerialPart2.SetOnClickListener(this);
            _ibAerialPart3.SetOnClickListener(this);
            _ibAerialPart4.SetOnClickListener(this);
            _ibAerialPart5.SetOnClickListener(this);

            _ibPlantability1 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibPlantability1);
            _ibPlantability2 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibPlantability2);
            _ibPlantability3 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibPlantability3);
            _ibPlantability4 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibPlantability4);
            _ibPlantability5 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibPlantability5);

            _ibPlantability1.SetOnClickListener(this);
            _ibPlantability2.SetOnClickListener(this);
            _ibPlantability3.SetOnClickListener(this);
            _ibPlantability4.SetOnClickListener(this);
            _ibPlantability5.SetOnClickListener(this);

            _ibCompaction1 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibCompaction1);
            _ibCompaction2 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibCompaction2);
            _ibCompaction3 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibCompaction3);
            _ibCompaction4 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibCompaction4);
            _ibCompaction5 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibCompaction5);

            _ibCompaction1.SetOnClickListener(this);
            _ibCompaction2.SetOnClickListener(this);
            _ibCompaction3.SetOnClickListener(this);
            _ibCompaction4.SetOnClickListener(this);
            _ibCompaction5.SetOnClickListener(this);

            _ibNematodes1 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibNematodes1);
            _ibNematodes2 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibNematodes2);
            _ibNematodes3 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibNematodes3);
            _ibNematodes4 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibNematodes4);
            _ibNematodes5 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibNematodes5);

            _ibNematodes1.SetOnClickListener(this);
            _ibNematodes2.SetOnClickListener(this);
            _ibNematodes3.SetOnClickListener(this);
            _ibNematodes4.SetOnClickListener(this);
            _ibNematodes5.SetOnClickListener(this);

            _ibOthers1 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibOthers1);
            _ibOthers2 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibOthers2);
            _ibOthers3 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibOthers3);
            _ibOthers4 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibOthers4);
            _ibOthers5 = _view.FindViewById<AppCompatImageButton>(Resource.Id.ibOthers5);

            _ibOthers1.SetOnClickListener(this);
            _ibOthers2.SetOnClickListener(this);
            _ibOthers3.SetOnClickListener(this);
            _ibOthers4.SetOnClickListener(this);
            _ibOthers5.SetOnClickListener(this);
        }
        private void LoadTextInputs()
        {
            _tilField = _view.FindViewById<TextInputLayout>(Resource.Id.tilField);
            _tilDate = _view.FindViewById<TextInputLayout>(Resource.Id.tilDate);
            _tilVariety = _view.FindViewById<TextInputLayout>(Resource.Id.tilVariety);
            _tilStand = _view.FindViewById<TextInputLayout>(Resource.Id.tilStand);
            _tilPhenologicalStadium = _view.FindViewById<TextInputLayout>(Resource.Id.tilPhenologicalStadium);
            _tilObservations = _view.FindViewById<TextInputLayout>(Resource.Id.tilObservations);
            _tilObservationsRootSystem = _view.FindViewById<TextInputLayout>(Resource.Id.tilObservationsRootSystem);
            _tilObservationsAerialPart = _view.FindViewById<TextInputLayout>(Resource.Id.tilObservationsAerialPart);
            _tilObservationsPlantability = _view.FindViewById<TextInputLayout>(Resource.Id.tilObservationsPlantability);
            _tilObservationsCompaction = _view.FindViewById<TextInputLayout>(Resource.Id.tilObservationsCompaction);
            _tilObservationsNematodes = _view.FindViewById<TextInputLayout>(Resource.Id.tilObservationsNematodes);
            _tilObservationsOthers = _view.FindViewById<TextInputLayout>(Resource.Id.tilObservationsOthers);

            _tilDate.EditText.FocusChange += DatePicker_Click;
        }
        private void LoadRadioGroups()
        {
            _rgRootSystem = _view.FindViewById<RadioGroup>(Resource.Id.rgRootSystem);
            _rgAerialPart = _view.FindViewById<RadioGroup>(Resource.Id.rgAerialPart);
            _rgPlantability = _view.FindViewById<RadioGroup>(Resource.Id.rgPlantability);
            _rgCompaction = _view.FindViewById<RadioGroup>(Resource.Id.rgCompaction);
            _rgNematodes = _view.FindViewById<RadioGroup>(Resource.Id.rgNematodes);
            _rgOthers = _view.FindViewById<RadioGroup>(Resource.Id.rgOthers);

            _rgRootSystem.SetOnCheckedChangeListener(this);
            _rgAerialPart.SetOnCheckedChangeListener(this);
            _rgPlantability.SetOnCheckedChangeListener(this);
            _rgCompaction.SetOnCheckedChangeListener(this);
            _rgNematodes.SetOnCheckedChangeListener(this);
            _rgOthers.SetOnCheckedChangeListener(this);
        }
        private void LoadFields()
        {
            var point = DataBase.CachedPoints.FirstOrDefault(x => x.IdPoint == CurrentId);

            MainThread.BeginInvokeOnMainThread(delegate
            {
                if (point == null)
                {
                    try
                    {
                        FeatureCollection featureCollection = DataBase.GeoJsonPoint;
                        Feature point = featureCollection.Features.Find(x => $"{x.Properties["Id"]}" == $"{CurrentId}");

                        _tilField.EditText.Text = $"{point.Properties["Talhao"]}";
                        _tilDate.EditText.Text = $"{System.DateTime.Now}";
                        _tilVariety.EditText.Text = $"{point.Properties["Variedade"]}";
                        _tilStand.EditText.Text = $"{point.Properties["Stand"]}";
                        _tilPhenologicalStadium.EditText.Text = $"{point.Properties["Estadio_Fenologico"]}";
                        _tilObservations.EditText.Text = $"{point.Properties["Observacoes"]}";
                    }
                    catch (System.Collections.Generic.KeyNotFoundException) { }
                }
                else
                {
                    _collectPoint = point;
                    _tilField.EditText.Text = point.Name;
                    _tilDate.EditText.Text = point.Date == System.DateTime.MinValue ? null : point.Date.ToString();
                    _tilVariety.EditText.Text = point.Variety;
                    _tilStand.EditText.Text = point.Stand;
                    _tilPhenologicalStadium.EditText.Text = point.PhenologicalStadium;
                    _tilObservations.EditText.Text = point.Observation;

                    switch (point.RootSystemRating)
                    {
                        case 1: _view.FindViewById<RadioButton>(Resource.Id.rbRootSystem1).Checked = true; break;
                        case 2: _view.FindViewById<RadioButton>(Resource.Id.rbRootSystem2).Checked = true; break;
                        case 3: _view.FindViewById<RadioButton>(Resource.Id.rbRootSystem3).Checked = true; break;
                        case 4: _view.FindViewById<RadioButton>(Resource.Id.rbRootSystem4).Checked = true; break;
                        case 5: _view.FindViewById<RadioButton>(Resource.Id.rbRootSystem5).Checked = true; break;
                    }

                    _tilObservationsRootSystem.EditText.Text = point.RootSystemObservation;

                    if (point.RootSystemPhoto01 != null) { if (System.IO.File.Exists(point.RootSystemPhoto01)) { _ibRootSystem1.SetImageBitmap(BitmapFactory.DecodeFile(point.RootSystemPhoto01)); } }
                    if (point.RootSystemPhoto02 != null) { if (System.IO.File.Exists(point.RootSystemPhoto02)) { _ibRootSystem2.SetImageBitmap(BitmapFactory.DecodeFile(point.RootSystemPhoto02)); } }
                    if (point.RootSystemPhoto03 != null) { if (System.IO.File.Exists(point.RootSystemPhoto03)) { _ibRootSystem3.SetImageBitmap(BitmapFactory.DecodeFile(point.RootSystemPhoto03)); } }
                    if (point.RootSystemPhoto04 != null) { if (System.IO.File.Exists(point.RootSystemPhoto04)) { _ibRootSystem4.SetImageBitmap(BitmapFactory.DecodeFile(point.RootSystemPhoto04)); } }
                    if (point.RootSystemPhoto05 != null) { if (System.IO.File.Exists(point.RootSystemPhoto05)) { _ibRootSystem5.SetImageBitmap(BitmapFactory.DecodeFile(point.RootSystemPhoto05)); } }

                    switch (point.AerialPartRating)
                    {
                        case 1: _view.FindViewById<RadioButton>(Resource.Id.rbAerialPart1).Checked = true; break;
                        case 2: _view.FindViewById<RadioButton>(Resource.Id.rbAerialPart2).Checked = true; break;
                        case 3: _view.FindViewById<RadioButton>(Resource.Id.rbAerialPart3).Checked = true; break;
                        case 4: _view.FindViewById<RadioButton>(Resource.Id.rbAerialPart4).Checked = true; break;
                        case 5: _view.FindViewById<RadioButton>(Resource.Id.rbAerialPart5).Checked = true; break;
                    }

                    _tilObservationsAerialPart.EditText.Text = point.AerialPartObservation;

                    if (point.AerialPartPhoto01 != null) { if (System.IO.File.Exists(point.AerialPartPhoto01)) { _ibAerialPart1.SetImageBitmap(BitmapFactory.DecodeFile(point.AerialPartPhoto01)); } }
                    if (point.AerialPartPhoto02 != null) { if (System.IO.File.Exists(point.AerialPartPhoto02)) { _ibAerialPart2.SetImageBitmap(BitmapFactory.DecodeFile(point.AerialPartPhoto02)); } }
                    if (point.AerialPartPhoto03 != null) { if (System.IO.File.Exists(point.AerialPartPhoto03)) { _ibAerialPart3.SetImageBitmap(BitmapFactory.DecodeFile(point.AerialPartPhoto03)); } }
                    if (point.AerialPartPhoto04 != null) { if (System.IO.File.Exists(point.AerialPartPhoto04)) { _ibAerialPart4.SetImageBitmap(BitmapFactory.DecodeFile(point.AerialPartPhoto04)); } }
                    if (point.AerialPartPhoto05 != null) { if (System.IO.File.Exists(point.AerialPartPhoto05)) { _ibAerialPart5.SetImageBitmap(BitmapFactory.DecodeFile(point.AerialPartPhoto05)); } }

                    switch (point.PlantabilityRating)
                    {
                        case 1: _view.FindViewById<RadioButton>(Resource.Id.rbPlantability1).Checked = true; break;
                        case 2: _view.FindViewById<RadioButton>(Resource.Id.rbPlantability2).Checked = true; break;
                        case 3: _view.FindViewById<RadioButton>(Resource.Id.rbPlantability3).Checked = true; break;
                        case 4: _view.FindViewById<RadioButton>(Resource.Id.rbPlantability4).Checked = true; break;
                        case 5: _view.FindViewById<RadioButton>(Resource.Id.rbPlantability5).Checked = true; break;
                    }

                    _tilObservationsPlantability.EditText.Text = point.PlantabilityObservation;

                    if (point.PlantabilityPhoto01 != null) { if (System.IO.File.Exists(point.PlantabilityPhoto01)) { _ibPlantability1.SetImageBitmap(BitmapFactory.DecodeFile(point.PlantabilityPhoto01)); } }
                    if (point.PlantabilityPhoto02 != null) { if (System.IO.File.Exists(point.PlantabilityPhoto02)) { _ibPlantability2.SetImageBitmap(BitmapFactory.DecodeFile(point.PlantabilityPhoto02)); } }
                    if (point.PlantabilityPhoto03 != null) { if (System.IO.File.Exists(point.PlantabilityPhoto03)) { _ibPlantability3.SetImageBitmap(BitmapFactory.DecodeFile(point.PlantabilityPhoto03)); } }
                    if (point.PlantabilityPhoto04 != null) { if (System.IO.File.Exists(point.PlantabilityPhoto04)) { _ibPlantability4.SetImageBitmap(BitmapFactory.DecodeFile(point.PlantabilityPhoto04)); } }
                    if (point.PlantabilityPhoto05 != null) { if (System.IO.File.Exists(point.PlantabilityPhoto05)) { _ibPlantability5.SetImageBitmap(BitmapFactory.DecodeFile(point.PlantabilityPhoto05)); } }

                    switch (point.CompactionRating)
                    {
                        case 1: _view.FindViewById<RadioButton>(Resource.Id.rbCompaction1).Checked = true; break;
                        case 2: _view.FindViewById<RadioButton>(Resource.Id.rbCompaction2).Checked = true; break;
                        case 3: _view.FindViewById<RadioButton>(Resource.Id.rbCompaction3).Checked = true; break;
                        case 4: _view.FindViewById<RadioButton>(Resource.Id.rbCompaction4).Checked = true; break;
                        case 5: _view.FindViewById<RadioButton>(Resource.Id.rbCompaction5).Checked = true; break;
                    }

                    _tilObservationsCompaction.EditText.Text = point.CompactionObservation;

                    if (point.CompactionPhoto01 != null) { if (System.IO.File.Exists(point.CompactionPhoto01)) { _ibCompaction1.SetImageBitmap(BitmapFactory.DecodeFile(point.CompactionPhoto01)); } }
                    if (point.CompactionPhoto02 != null) { if (System.IO.File.Exists(point.CompactionPhoto02)) { _ibCompaction2.SetImageBitmap(BitmapFactory.DecodeFile(point.CompactionPhoto02)); } }
                    if (point.CompactionPhoto03 != null) { if (System.IO.File.Exists(point.CompactionPhoto03)) { _ibCompaction3.SetImageBitmap(BitmapFactory.DecodeFile(point.CompactionPhoto03)); } }
                    if (point.CompactionPhoto04 != null) { if (System.IO.File.Exists(point.CompactionPhoto04)) { _ibCompaction4.SetImageBitmap(BitmapFactory.DecodeFile(point.CompactionPhoto04)); } }
                    if (point.CompactionPhoto05 != null) { if (System.IO.File.Exists(point.CompactionPhoto05)) { _ibCompaction5.SetImageBitmap(BitmapFactory.DecodeFile(point.CompactionPhoto05)); } }

                    switch (point.NematodesRating)
                    {
                        case 1: _view.FindViewById<RadioButton>(Resource.Id.rbNematodes1).Checked = true; break;
                        case 2: _view.FindViewById<RadioButton>(Resource.Id.rbNematodes2).Checked = true; break;
                        case 3: _view.FindViewById<RadioButton>(Resource.Id.rbNematodes3).Checked = true; break;
                        case 4: _view.FindViewById<RadioButton>(Resource.Id.rbNematodes4).Checked = true; break;
                        case 5: _view.FindViewById<RadioButton>(Resource.Id.rbNematodes5).Checked = true; break;
                    }

                    _tilObservationsNematodes.EditText.Text = point.NematodesObservation;

                    if (point.NematodesPhoto01 != null) { if (System.IO.File.Exists(point.NematodesPhoto01)) { _ibNematodes1.SetImageBitmap(BitmapFactory.DecodeFile(point.NematodesPhoto01)); } }
                    if (point.NematodesPhoto02 != null) { if (System.IO.File.Exists(point.NematodesPhoto02)) { _ibNematodes2.SetImageBitmap(BitmapFactory.DecodeFile(point.NematodesPhoto02)); } }
                    if (point.NematodesPhoto03 != null) { if (System.IO.File.Exists(point.NematodesPhoto03)) { _ibNematodes3.SetImageBitmap(BitmapFactory.DecodeFile(point.NematodesPhoto03)); } }
                    if (point.NematodesPhoto04 != null) { if (System.IO.File.Exists(point.NematodesPhoto04)) { _ibNematodes4.SetImageBitmap(BitmapFactory.DecodeFile(point.NematodesPhoto04)); } }
                    if (point.NematodesPhoto05 != null) { if (System.IO.File.Exists(point.NematodesPhoto05)) { _ibNematodes5.SetImageBitmap(BitmapFactory.DecodeFile(point.NematodesPhoto05)); } }

                    switch (point.OthersRating)
                    {
                        case 1: _view.FindViewById<RadioButton>(Resource.Id.rbOthers1).Checked = true; break;
                        case 2: _view.FindViewById<RadioButton>(Resource.Id.rbOthers2).Checked = true; break;
                        case 3: _view.FindViewById<RadioButton>(Resource.Id.rbOthers3).Checked = true; break;
                        case 4: _view.FindViewById<RadioButton>(Resource.Id.rbOthers4).Checked = true; break;
                        case 5: _view.FindViewById<RadioButton>(Resource.Id.rbOthers5).Checked = true; break;
                    }

                    _tilObservationsOthers.EditText.Text = point.OthersObservation;

                    if (point.OthersPhoto01 != null) { if (System.IO.File.Exists(point.OthersPhoto01)) { _ibOthers1.SetImageBitmap(BitmapFactory.DecodeFile(point.OthersPhoto01)); } }
                    if (point.OthersPhoto02 != null) { if (System.IO.File.Exists(point.OthersPhoto02)) { _ibOthers2.SetImageBitmap(BitmapFactory.DecodeFile(point.OthersPhoto02)); } }
                    if (point.OthersPhoto03 != null) { if (System.IO.File.Exists(point.OthersPhoto03)) { _ibOthers3.SetImageBitmap(BitmapFactory.DecodeFile(point.OthersPhoto03)); } }
                    if (point.OthersPhoto04 != null) { if (System.IO.File.Exists(point.OthersPhoto04)) { _ibOthers4.SetImageBitmap(BitmapFactory.DecodeFile(point.OthersPhoto04)); } }
                    if (point.OthersPhoto05 != null) { if (System.IO.File.Exists(point.OthersPhoto05)) { _ibOthers5.SetImageBitmap(BitmapFactory.DecodeFile(point.OthersPhoto05)); } }
                }
            });
        }
        private void DatePicker_Click(object sender, System.EventArgs eventArgs)
        {
            if (((View.FocusChangeEventArgs)eventArgs).HasFocus == false) return;

            Android.App.DatePickerDialog datePickerDialog = new Android.App.DatePickerDialog(Activity);
            datePickerDialog.DateSet += (object objSender, Android.App.DatePickerDialog.DateSetEventArgs dateSetEventArgs) =>
            {
                _tilDate.EditText.Text = dateSetEventArgs.Date.ToString("dd/MM/yyyy");

                Android.App.TimePickerDialog timePickerDialog = new Android.App.TimePickerDialog(Activity, (s, e) =>
                {
                    string hour = e.HourOfDay < 10 ? $"0{e.HourOfDay}" : e.HourOfDay.ToString();
                    string minute = e.Minute < 10 ? $"0{e.Minute}" : e.Minute.ToString();
                    _tilDate.EditText.Text += $" {hour}:{minute}";

                    _tilVariety.RequestFocus();
                }, System.DateTime.Now.Hour, System.DateTime.Now.Minute, true);
                timePickerDialog.CancelEvent += delegate
                {
                    _tilDate.EditText.Text = null;
                };

                timePickerDialog.Show();
            };
            datePickerDialog.CancelEvent += delegate
            {
                _tilDate.EditText.Text = null;
            };
            datePickerDialog.Show();
        }
        private async Task<bool> HavePermissions()
        {
            PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>() != PermissionStatus.Granted ?
                  await Permissions.RequestAsync<Permissions.Camera>() :
                  await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permissionStatus != PermissionStatus.Granted) return false;

            permissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>() != PermissionStatus.Granted ?
                await Permissions.RequestAsync<Permissions.StorageWrite>() :
                await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (permissionStatus != PermissionStatus.Granted) return false;

            return true;
        }
        private async void TakePhoto(View v)
        {
            try
            {
                if (!await HavePermissions()) return;

                MediaFile photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Small
                });

                if (photo == null) return;

                switch (v.Id)
                {
                    case Resource.Id.ibRootSystem1:
                        if (System.IO.File.Exists(_collectPoint.RootSystemPhoto01)) { System.IO.File.Delete(_collectPoint.RootSystemPhoto01); }
                        _collectPoint.RootSystemPhoto01 = photo.Path;
                        _ibRootSystem1.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibRootSystem2:
                        if (System.IO.File.Exists(_collectPoint.RootSystemPhoto02)) { System.IO.File.Delete(_collectPoint.RootSystemPhoto02); }
                        _collectPoint.RootSystemPhoto02 = photo.Path;
                        _ibRootSystem2.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibRootSystem3:
                        if (System.IO.File.Exists(_collectPoint.RootSystemPhoto03)) { System.IO.File.Delete(_collectPoint.RootSystemPhoto03); }
                        _collectPoint.RootSystemPhoto03 = photo.Path;
                        _ibRootSystem3.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibRootSystem4:
                        if (System.IO.File.Exists(_collectPoint.RootSystemPhoto04)) { System.IO.File.Delete(_collectPoint.RootSystemPhoto04); }
                        _collectPoint.RootSystemPhoto04 = photo.Path;
                        _ibRootSystem4.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibRootSystem5:
                        if (System.IO.File.Exists(_collectPoint.RootSystemPhoto05)) { System.IO.File.Delete(_collectPoint.RootSystemPhoto05); }
                        _collectPoint.RootSystemPhoto05 = photo.Path;
                        _ibRootSystem5.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibAerialPart1:
                        if (System.IO.File.Exists(_collectPoint.AerialPartPhoto01)) { System.IO.File.Delete(_collectPoint.AerialPartPhoto01); }
                        _collectPoint.AerialPartPhoto01 = photo.Path;
                        _ibAerialPart1.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibAerialPart2:
                        if (System.IO.File.Exists(_collectPoint.AerialPartPhoto02)) { System.IO.File.Delete(_collectPoint.AerialPartPhoto02); }
                        _collectPoint.AerialPartPhoto02 = photo.Path;
                        _ibAerialPart2.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibAerialPart3:
                        if (System.IO.File.Exists(_collectPoint.AerialPartPhoto03)) { System.IO.File.Delete(_collectPoint.AerialPartPhoto03); }
                        _collectPoint.AerialPartPhoto03 = photo.Path;
                        _ibAerialPart3.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibAerialPart4:
                        if (System.IO.File.Exists(_collectPoint.AerialPartPhoto04)) { System.IO.File.Delete(_collectPoint.AerialPartPhoto04); }
                        _collectPoint.AerialPartPhoto04 = photo.Path;
                        _ibAerialPart4.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibAerialPart5:
                        if (System.IO.File.Exists(_collectPoint.AerialPartPhoto05)) { System.IO.File.Delete(_collectPoint.AerialPartPhoto05); }
                        _collectPoint.AerialPartPhoto05 = photo.Path;
                        _ibAerialPart5.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibPlantability1:
                        if (System.IO.File.Exists(_collectPoint.PlantabilityPhoto01)) { System.IO.File.Delete(_collectPoint.PlantabilityPhoto01); }
                        _collectPoint.PlantabilityPhoto01 = photo.Path;
                        _ibPlantability1.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibPlantability2:
                        if (System.IO.File.Exists(_collectPoint.PlantabilityPhoto02)) { System.IO.File.Delete(_collectPoint.PlantabilityPhoto02); }
                        _collectPoint.PlantabilityPhoto02 = photo.Path;
                        _ibPlantability2.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibPlantability3:
                        if (System.IO.File.Exists(_collectPoint.PlantabilityPhoto03)) { System.IO.File.Delete(_collectPoint.PlantabilityPhoto03); }
                        _collectPoint.PlantabilityPhoto03 = photo.Path;
                        _ibPlantability3.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibPlantability4:
                        if (System.IO.File.Exists(_collectPoint.PlantabilityPhoto04)) { System.IO.File.Delete(_collectPoint.PlantabilityPhoto04); }
                        _collectPoint.PlantabilityPhoto04 = photo.Path;
                        _ibPlantability4.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibPlantability5:
                        if (System.IO.File.Exists(_collectPoint.PlantabilityPhoto05)) { System.IO.File.Delete(_collectPoint.PlantabilityPhoto05); }
                        _collectPoint.PlantabilityPhoto05 = photo.Path;
                        _ibPlantability5.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibCompaction1:
                        if (System.IO.File.Exists(_collectPoint.CompactionPhoto01)) { System.IO.File.Delete(_collectPoint.CompactionPhoto01); }
                        _collectPoint.CompactionPhoto01 = photo.Path;
                        _ibCompaction1.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibCompaction2:
                        if (System.IO.File.Exists(_collectPoint.CompactionPhoto02)) { System.IO.File.Delete(_collectPoint.CompactionPhoto02); }
                        _collectPoint.CompactionPhoto02 = photo.Path;
                        _ibCompaction2.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibCompaction3:
                        if (System.IO.File.Exists(_collectPoint.CompactionPhoto03)) { System.IO.File.Delete(_collectPoint.CompactionPhoto03); }
                        _collectPoint.CompactionPhoto03 = photo.Path;
                        _ibCompaction3.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibCompaction4:
                        if (System.IO.File.Exists(_collectPoint.CompactionPhoto04)) { System.IO.File.Delete(_collectPoint.CompactionPhoto04); }
                        _collectPoint.CompactionPhoto04 = photo.Path;
                        _ibCompaction4.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibCompaction5:
                        if (System.IO.File.Exists(_collectPoint.CompactionPhoto05)) { System.IO.File.Delete(_collectPoint.CompactionPhoto05); }
                        _collectPoint.CompactionPhoto05 = photo.Path;
                        _ibCompaction5.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibNematodes1:
                        if (System.IO.File.Exists(_collectPoint.NematodesPhoto01)) { System.IO.File.Delete(_collectPoint.NematodesPhoto01); }
                        _collectPoint.NematodesPhoto01 = photo.Path;
                        _ibNematodes1.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibNematodes2:
                        if (System.IO.File.Exists(_collectPoint.NematodesPhoto02)) { System.IO.File.Delete(_collectPoint.NematodesPhoto02); }
                        _collectPoint.NematodesPhoto02 = photo.Path;
                        _ibNematodes2.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibNematodes3:
                        if (System.IO.File.Exists(_collectPoint.NematodesPhoto03)) { System.IO.File.Delete(_collectPoint.NematodesPhoto03); }
                        _collectPoint.NematodesPhoto03 = photo.Path;
                        _ibNematodes3.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibNematodes4:
                        if (System.IO.File.Exists(_collectPoint.NematodesPhoto04)) { System.IO.File.Delete(_collectPoint.NematodesPhoto04); }
                        _collectPoint.NematodesPhoto04 = photo.Path;
                        _ibNematodes4.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibNematodes5:
                        if (System.IO.File.Exists(_collectPoint.NematodesPhoto05)) { System.IO.File.Delete(_collectPoint.NematodesPhoto05); }
                        _collectPoint.NematodesPhoto05 = photo.Path;
                        _ibNematodes5.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibOthers1:
                        if (System.IO.File.Exists(_collectPoint.OthersPhoto01)) { System.IO.File.Delete(_collectPoint.OthersPhoto01); }
                        _collectPoint.OthersPhoto01 = photo.Path;
                        _ibOthers1.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibOthers2:
                        if (System.IO.File.Exists(_collectPoint.OthersPhoto02)) { System.IO.File.Delete(_collectPoint.OthersPhoto02); }
                        _collectPoint.OthersPhoto02 = photo.Path;
                        _ibOthers2.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibOthers3:
                        if (System.IO.File.Exists(_collectPoint.OthersPhoto03)) { System.IO.File.Delete(_collectPoint.OthersPhoto03); }
                        _collectPoint.OthersPhoto03 = photo.Path;
                        _ibOthers3.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibOthers4:
                        if (System.IO.File.Exists(_collectPoint.OthersPhoto04)) { System.IO.File.Delete(_collectPoint.OthersPhoto04); }
                        _collectPoint.OthersPhoto04 = photo.Path;
                        _ibOthers4.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                    case Resource.Id.ibOthers5:
                        if (System.IO.File.Exists(_collectPoint.OthersPhoto05)) { System.IO.File.Delete(_collectPoint.OthersPhoto05); }
                        _collectPoint.OthersPhoto05 = photo.Path;
                        _ibOthers5.SetImageBitmap(BitmapFactory.DecodeFile(photo.Path));
                        break;
                }
            }
            catch (System.Exception) { }
        }
        private void RemovePhoto(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.ibRootSystem1:
                    System.IO.File.Delete(_collectPoint.RootSystemPhoto01);
                    _collectPoint.RootSystemPhoto01 = null;
                    _ibRootSystem1.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibRootSystem2:
                    System.IO.File.Delete(_collectPoint.RootSystemPhoto02);
                    _collectPoint.RootSystemPhoto02 = null;
                    _ibRootSystem2.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibRootSystem3:
                    System.IO.File.Delete(_collectPoint.RootSystemPhoto03);
                    _collectPoint.RootSystemPhoto03 = null;
                    _ibRootSystem3.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibRootSystem4:
                    System.IO.File.Delete(_collectPoint.RootSystemPhoto04);
                    _collectPoint.RootSystemPhoto04 = null;
                    _ibRootSystem4.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibRootSystem5:
                    System.IO.File.Delete(_collectPoint.RootSystemPhoto05);
                    _collectPoint.RootSystemPhoto05 = null;
                    _ibRootSystem5.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibAerialPart1:
                    System.IO.File.Delete(_collectPoint.AerialPartPhoto01);
                    _collectPoint.AerialPartPhoto01 = null;
                    _ibAerialPart1.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibAerialPart2:
                    System.IO.File.Delete(_collectPoint.AerialPartPhoto02);
                    _collectPoint.AerialPartPhoto02 = null;
                    _ibAerialPart2.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibAerialPart3:
                    System.IO.File.Delete(_collectPoint.AerialPartPhoto03);
                    _collectPoint.AerialPartPhoto03 = null;
                    _ibAerialPart3.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibAerialPart4:
                    System.IO.File.Delete(_collectPoint.AerialPartPhoto04);
                    _collectPoint.AerialPartPhoto04 = null;
                    _ibAerialPart4.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibAerialPart5:
                    System.IO.File.Delete(_collectPoint.AerialPartPhoto05);
                    _collectPoint.AerialPartPhoto05 = null;
                    _ibAerialPart5.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibPlantability1:
                    System.IO.File.Delete(_collectPoint.PlantabilityPhoto01);
                    _collectPoint.PlantabilityPhoto01 = null;
                    _ibPlantability1.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibPlantability2:
                    System.IO.File.Delete(_collectPoint.PlantabilityPhoto02);
                    _collectPoint.PlantabilityPhoto02 = null;
                    _ibPlantability2.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibPlantability3:
                    System.IO.File.Delete(_collectPoint.PlantabilityPhoto03);
                    _collectPoint.PlantabilityPhoto03 = null;
                    _ibPlantability3.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibPlantability4:
                    System.IO.File.Delete(_collectPoint.PlantabilityPhoto04);
                    _collectPoint.PlantabilityPhoto04 = null;
                    _ibPlantability4.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibPlantability5:
                    System.IO.File.Delete(_collectPoint.PlantabilityPhoto05);
                    _collectPoint.PlantabilityPhoto05 = null;
                    _ibPlantability5.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibCompaction1:
                    System.IO.File.Delete(_collectPoint.CompactionPhoto01);
                    _collectPoint.CompactionPhoto01 = null;
                    _ibCompaction1.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibCompaction2:
                    System.IO.File.Delete(_collectPoint.CompactionPhoto02);
                    _collectPoint.CompactionPhoto02 = null;
                    _ibCompaction2.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibCompaction3:
                    System.IO.File.Delete(_collectPoint.CompactionPhoto03);
                    _collectPoint.CompactionPhoto03 = null;
                    _ibCompaction3.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibCompaction4:
                    System.IO.File.Delete(_collectPoint.CompactionPhoto04);
                    _collectPoint.CompactionPhoto04 = null;
                    _ibCompaction4.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibCompaction5:
                    System.IO.File.Delete(_collectPoint.CompactionPhoto05);
                    _collectPoint.CompactionPhoto05 = null;
                    _ibCompaction5.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibNematodes1:
                    System.IO.File.Delete(_collectPoint.NematodesPhoto01);
                    _collectPoint.NematodesPhoto01 = null;
                    _ibNematodes1.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibNematodes2:
                    System.IO.File.Delete(_collectPoint.NematodesPhoto02);
                    _collectPoint.NematodesPhoto02 = null;
                    _ibNematodes2.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibNematodes3:
                    System.IO.File.Delete(_collectPoint.NematodesPhoto03);
                    _collectPoint.NematodesPhoto03 = null;
                    _ibNematodes3.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibNematodes4:
                    System.IO.File.Delete(_collectPoint.NematodesPhoto04);
                    _collectPoint.NematodesPhoto04 = null;
                    _ibNematodes4.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibNematodes5:
                    System.IO.File.Delete(_collectPoint.NematodesPhoto05);
                    _collectPoint.NematodesPhoto05 = null;
                    _ibNematodes5.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibOthers1:
                    System.IO.File.Delete(_collectPoint.OthersPhoto01);
                    _collectPoint.OthersPhoto01 = null;
                    _ibOthers1.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibOthers2:
                    System.IO.File.Delete(_collectPoint.OthersPhoto02);
                    _collectPoint.OthersPhoto02 = null;
                    _ibOthers2.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibOthers3:
                    System.IO.File.Delete(_collectPoint.OthersPhoto03);
                    _collectPoint.OthersPhoto03 = null;
                    _ibOthers3.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibOthers4:
                    System.IO.File.Delete(_collectPoint.OthersPhoto04);
                    _collectPoint.OthersPhoto04 = null;
                    _ibOthers4.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
                case Resource.Id.ibOthers5:
                    System.IO.File.Delete(_collectPoint.OthersPhoto05);
                    _collectPoint.OthersPhoto05 = null;
                    _ibOthers5.SetImageDrawable(Activity.GetDrawable(Resource.Drawable.ic_add_a_photo));
                    break;
            }
        }
        private string GetPhoto(View v)
        {
            var cachedPoints = DataBase.CachedPoints;
            var temp = cachedPoints.FirstOrDefault(x => x.IdPoint == CurrentId);

            return v.Id switch
            {
                Resource.Id.ibRootSystem1 => temp.RootSystemPhoto01,
                Resource.Id.ibRootSystem2 => temp.RootSystemPhoto02,
                Resource.Id.ibRootSystem3 => temp.RootSystemPhoto03,
                Resource.Id.ibRootSystem4 => temp.RootSystemPhoto04,
                Resource.Id.ibRootSystem5 => temp.RootSystemPhoto05,
                Resource.Id.ibAerialPart1 => temp.AerialPartPhoto01,
                Resource.Id.ibAerialPart2 => temp.AerialPartPhoto02,
                Resource.Id.ibAerialPart3 => temp.AerialPartPhoto03,
                Resource.Id.ibAerialPart4 => temp.AerialPartPhoto04,
                Resource.Id.ibAerialPart5 => temp.AerialPartPhoto05,
                Resource.Id.ibPlantability1 => temp.PlantabilityPhoto01,
                Resource.Id.ibPlantability2 => temp.PlantabilityPhoto02,
                Resource.Id.ibPlantability3 => temp.PlantabilityPhoto03,
                Resource.Id.ibPlantability4 => temp.PlantabilityPhoto04,
                Resource.Id.ibPlantability5 => temp.PlantabilityPhoto05,
                Resource.Id.ibCompaction1 => temp.CompactionPhoto01,
                Resource.Id.ibCompaction2 => temp.CompactionPhoto02,
                Resource.Id.ibCompaction3 => temp.CompactionPhoto03,
                Resource.Id.ibCompaction4 => temp.CompactionPhoto04,
                Resource.Id.ibCompaction5 => temp.CompactionPhoto05,
                Resource.Id.ibNematodes1 => temp.NematodesPhoto01,
                Resource.Id.ibNematodes2 => temp.NematodesPhoto02,
                Resource.Id.ibNematodes3 => temp.NematodesPhoto03,
                Resource.Id.ibNematodes4 => temp.NematodesPhoto04,
                Resource.Id.ibNematodes5 => temp.NematodesPhoto05,
                Resource.Id.ibOthers1 => temp.OthersPhoto01,
                Resource.Id.ibOthers2 => temp.OthersPhoto02,
                Resource.Id.ibOthers3 => temp.OthersPhoto03,
                Resource.Id.ibOthers4 => temp.OthersPhoto04,
                Resource.Id.ibOthers5 => temp.OthersPhoto05,
                _ => null
            };
        }
        private async void SavePoint(object sender, System.EventArgs e)
        {
            if (ValidateData())
            {
                try
                {
                    LoadingDialog.NewInstance(GetString(Resource.String.loading)).Show(Activity.SupportFragmentManager, "LoadingDialog");

                    await Task.Run(() =>
                    {
                        SaveToCache(sender, null);
                        DataBase.SetSavedPoint(_collectPoint);

                        FeatureCollection featureCollection = DataBase.GeoJsonPoint;
                        Feature temp = featureCollection.Features.Find(x => System.Convert.ToInt32(x.Properties["Id"]) == CurrentId);
                        if (temp != null)
                        {
                            temp.Properties["Coletado"] = true;
                            DataBase.GeoJsonPoint = featureCollection;
                        }

                        Activity.StartActivity(typeof(MapActivity));
                        Activity.FinishAffinity();
                    });
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(Activity, ex.Message, ToastLength.Long).Show();
                    LoadingDialog.DismissLoading();
                }
            }
            else
            {
                InformationDialog.NewInstance(GetString(Resource.String.required_fields_title),
                    GetString(Resource.String.required_fields_review)).Show(Activity.SupportFragmentManager, "InformationDialog");
            }
        }
        private bool ValidateRootSystem()
        {
            if (_rgRootSystem.CheckedRadioButtonId == -1)
            {
                _view.FindViewById<AppCompatTextView>(Resource.Id.tvRootSystem).SetTextColor(Color.Red);
                return false;
            }
            else
            {
                _view.FindViewById<AppCompatTextView>(Resource.Id.tvRootSystem).SetTextColor(Color.Black);
                return true;
            }
        }
        private bool ValidateAerialPart()
        {
            if (_rgAerialPart.CheckedRadioButtonId == -1)
            {
                _view.FindViewById<AppCompatTextView>(Resource.Id.tvAerialPart).SetTextColor(Color.Red);
                return false;
            }
            else
            {
                _view.FindViewById<AppCompatTextView>(Resource.Id.tvAerialPart).SetTextColor(Color.Black);
                return true;
            }
        }
        private bool ValidatePlantability()
        {
            if (_rgPlantability.CheckedRadioButtonId == -1)
            {
                _view.FindViewById<AppCompatTextView>(Resource.Id.tvPlantability).SetTextColor(Color.Red);
                return false;
            }
            else
            {
                _view.FindViewById<AppCompatTextView>(Resource.Id.tvPlantability).SetTextColor(Color.Black);
                return true;
            }
        }
        private bool ValidateData()
        {
            bool field = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilField);
            bool date = new Others.Validation((AppCompatActivity)Activity).ValidateDate(_tilDate);
            bool variety = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilVariety);
            bool stand = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilStand);
            bool phenologicalStadium = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilPhenologicalStadium);
            bool rootSystem = ValidateRootSystem();
            bool aerialPart = ValidateAerialPart();
            bool plantability = ValidatePlantability();

            return (field && date && variety && stand && phenologicalStadium && rootSystem && aerialPart && plantability);
        }
        private static void TimerStart()
        {
            _timer = new Timer(System.TimeSpan.FromSeconds(5).TotalMilliseconds);
            _timer.Elapsed += SaveToCache;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
        private static void TimerStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
        private static void SaveToCache(object sender, ElapsedEventArgs e)
        {
            if (_collectPoint is null) return;

            System.DateTime.TryParse(_tilDate.EditText.Text, out System.DateTime date);

            _collectPoint.Name = _tilField.EditText.Text;
            _collectPoint.Date = date;
            _collectPoint.Variety = _tilVariety.EditText.Text;
            _collectPoint.Stand = _tilStand.EditText.Text;
            _collectPoint.PhenologicalStadium = _tilPhenologicalStadium.EditText.Text;
            _collectPoint.Observation = _tilObservations.EditText.Text;

            _collectPoint.RootSystemObservation = _tilObservationsRootSystem.EditText.Text;
            _collectPoint.AerialPartObservation = _tilObservationsAerialPart.EditText.Text;
            _collectPoint.PlantabilityObservation = _tilObservationsPlantability.EditText.Text;
            _collectPoint.CompactionObservation = _tilObservationsCompaction.EditText.Text;
            _collectPoint.NematodesObservation = _tilObservationsNematodes.EditText.Text;
            _collectPoint.OthersObservation = _tilObservationsOthers.EditText.Text;

            DataBase.SetCachedPoint(_collectPoint);
        }
        #endregion
    }
}