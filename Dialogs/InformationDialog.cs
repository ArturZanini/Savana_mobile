using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;

namespace SistemaColeta.Dialogs
{
    public class InformationDialog : AppCompatDialogFragment
    {
        #region Private fields
        private static readonly string ARG_TITLE = "title";
        private static readonly string ARG_MESSAGE = "message";
        private static System.EventHandler<DialogClickEventArgs> Positive;
        private static AppCompatActivity _activity;
        #endregion

        #region Builders
        private InformationDialog()
        {
        }
        #endregion

        #region Public methods
        public static InformationDialog NewInstance(string title, string message)
        {
            Positive = null;

            var args = new Bundle();
            args.PutString(ARG_TITLE, title);
            args.PutString(ARG_MESSAGE, message);
            return new InformationDialog { Arguments = args };
        }
        public static InformationDialog NewInstance(string title, string message, System.EventHandler<DialogClickEventArgs> positive)
        {
            Positive = positive;

            var args = new Bundle();
            args.PutString(ARG_TITLE, title);
            args.PutString(ARG_MESSAGE, message);
            return new InformationDialog { Arguments = args };
        }
        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            _activity = Activity as AppCompatActivity;
            return new AlertDialog.Builder(_activity)
                .SetTitle(Arguments.GetString(ARG_TITLE))
                .SetMessage(Arguments.GetString(ARG_MESSAGE))
                .SetPositiveButton(Android.Resource.String.Ok, Positive ?? ((s, e) => { return; }))
                .Create();
        }
        #endregion

        #region Private methods
        private class PositiveListener : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            public void OnClick(IDialogInterface dialog, int which)
            {
                return;
            }
        }
        #endregion
    }
}