using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;

namespace SistemaColeta.Dialogs
{
    public class LoadingDialog : AppCompatDialogFragment
    {
        #region Private fields
        private static readonly string ARG_MESSAGE = "message";
        private static AppCompatActivity _activity;
        private static LoadingDialog _loadingDialog;
        #endregion

        #region Builders
        private LoadingDialog()
        {
            Cancelable = false;
        }
        #endregion

        #region Public methods
        public static LoadingDialog NewInstance(string message)
        {
            var args = new Bundle();
            args.PutString(ARG_MESSAGE, message);
            _loadingDialog = new LoadingDialog { Arguments = args };
            return _loadingDialog;
        }
        public static void DismissLoading()
        {
            _loadingDialog.Dismiss();
            _loadingDialog.Dispose();
        }
        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            _activity = Activity as AppCompatActivity;

            ViewGroup viewGroup = _activity.FindViewById<ViewGroup>(Resource.Id.content);
            View view = LayoutInflater.From(_activity).Inflate(Resource.Layout.layout_loading, viewGroup, false);

            AppCompatTextView loading_msg = view.FindViewById<AppCompatTextView>(Resource.Id.tvLoading);
            loading_msg.Text = Arguments.GetString(ARG_MESSAGE);

            return new AlertDialog.Builder(_activity)
                .SetView(view)
                .Create();
        }
        #endregion
    }
}