using Android.Gms.Maps;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using GeoJSON.Net.Feature;
using Google.Android.Material.TextField;
using Newtonsoft.Json;
using SistemaColeta.Map;
using SistemaColeta.Model;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Point = GeoJSON.Net.Geometry.Point;

namespace SistemaColeta.Dialogs
{
    public class SavePoint : AppCompatDialogFragment
    {
        #region Private fields
        private static AppCompatActivity _activity;

        private static GoogleMap _googleMap;
        private static FeatureCollection _featureCollection;
        private static Point _newPoint;

        TextInputLayout _tilName, _tilVariety, _tilStand, _tilPhenologicalStadium, _observations;
        #endregion

        #region Builders
        private SavePoint()
        {
        }
        #endregion

        #region Public methods
        public static SavePoint NewInstance(GoogleMap googleMap, FeatureCollection featureCollection, Point newPoint)
        {
            _googleMap = googleMap;
            _featureCollection = featureCollection;
            _newPoint = newPoint;

            return new SavePoint();
        }
        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            _activity = Activity as AppCompatActivity;

            ViewGroup viewGroup = _activity.FindViewById<ViewGroup>(Resource.Id.content);
            View view = LayoutInflater.From(_activity).Inflate(Resource.Layout.layout_save_point, viewGroup, false);

            view.FindViewById<AppCompatTextView>(Resource.Id.tvTitle).Text = $"{_activity.GetString(Resource.String.add_collection_point)} { _featureCollection.Features.Count + 1}";
            _tilName = view.FindViewById<TextInputLayout>(Resource.Id.tilField);
            _tilVariety = view.FindViewById<TextInputLayout>(Resource.Id.tilVariety);
            _tilStand = view.FindViewById<TextInputLayout>(Resource.Id.tilStand);
            _tilPhenologicalStadium = view.FindViewById<TextInputLayout>(Resource.Id.tilPhenologicalStadium);
            _observations = view.FindViewById<TextInputLayout>(Resource.Id.tilObservations);

            view.FindViewById<AppCompatButton>(Resource.Id.btnCancel).Click += (s, e) =>
            {
                Dismiss();
            };
            view.FindViewById<AppCompatButton>(Resource.Id.btnYes).Click += (s, e) =>
            {
                if (Validate())
                {
                    PointProperties pointProperties = new PointProperties()
                    {
                        Id = _featureCollection.Features.Count + 1,
                        Coletado = false,
                        Talhao = view.FindViewById<TextInputLayout>(Resource.Id.tilField).EditText.Text,
                        Variedade = view.FindViewById<TextInputLayout>(Resource.Id.tilVariety).EditText.Text,
                        Stand = view.FindViewById<TextInputLayout>(Resource.Id.tilStand).EditText.Text,
                        Estadio_Fenologico = view.FindViewById<TextInputLayout>(Resource.Id.tilPhenologicalStadium).EditText.Text,
                        Observacoes = view.FindViewById<TextInputLayout>(Resource.Id.tilObservations).EditText.Text,
                    };
                    _featureCollection.Features.Add(new Feature(_newPoint, pointProperties));

                    DataBase.GeoJsonPoint = _featureCollection;
                    SistemaColetaMap.LoadGeoJsonOnMap(_googleMap, JsonConvert.SerializeObject(_featureCollection));

                    Dismiss();
                }
            };

            return new AlertDialog.Builder(_activity)
                .SetView(view)
                .Create();
        }
        #endregion

        #region Private
        private bool Validate()
        {
            bool talhão = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilName);
            bool variedade = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilVariety);
            bool stand = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilStand);
            bool estágio_Fenológico = new Others.Validation((AppCompatActivity)Activity).ValidateField(_tilPhenologicalStadium);

            return talhão && variedade && stand && estágio_Fenológico;
        }
        #endregion
    }
}
