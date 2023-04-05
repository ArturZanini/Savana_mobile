using Android.Graphics;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace SistemaColeta.Dialogs
{
    public class PreviewPhoto : AppCompatDialogFragment
    {
        #region Private fields
        private static string _path;
        private static System.EventHandler _edit;
        private static System.EventHandler _remove;

        private static AppCompatActivity _activity;
        #endregion

        #region Builders
        private PreviewPhoto()
        {
        }
        #endregion

        #region Public methods
        public static PreviewPhoto NewInstance(string path, System.EventHandler edit, System.EventHandler remove)
        {
            _path = path;
            _edit = edit;
            _remove = remove;

            return new PreviewPhoto();
        }
        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            _activity = Activity as AppCompatActivity;

            ViewGroup viewGroup = _activity.FindViewById<ViewGroup>(Resource.Id.content);
            View view = LayoutInflater.From(_activity).Inflate(Resource.Layout.layout_photo, viewGroup, false);

            view.FindViewById<AppCompatImageView>(Resource.Id.ivPreview).SetImageBitmap(BitmapFactory.DecodeFile(_path));
            view.FindViewById<AppCompatButton>(Resource.Id.btnChange).Click += (s, e) =>
            {
                _edit(s, e);
                Dismiss();
            };
            view.FindViewById<AppCompatButton>(Resource.Id.btnDelete).Click += (s, e) =>
            {
                _remove(s, e);
                Dismiss();
            };

            return new AlertDialog.Builder(_activity)
                .SetView(view)
                .Create();
        }
        #endregion
    }
}
