using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;

namespace SistemaColeta.Dialogs
{
    public class ConfirmationDialog : AppCompatDialogFragment
    {
        #region Private fields
        private static readonly string ARG_TITLE = "title";
        private static readonly string ARG_MESSAGE = "message";
        private static System.EventHandler<DialogClickEventArgs> Positive;
        private static System.EventHandler<DialogClickEventArgs> Negative;
        private static AppCompatActivity _activity;
        #endregion

        #region Builders
        private ConfirmationDialog()
        {
        }
        #endregion

        #region Public methods
        public static ConfirmationDialog NewInstance(string title, string message, System.EventHandler<DialogClickEventArgs> positive)
        {
            Positive = positive;
            Negative = null;

            var args = new Bundle();
            args.PutString(ARG_TITLE, title);
            args.PutString(ARG_MESSAGE, message);
            return new ConfirmationDialog { Arguments = args };
        }
        public static ConfirmationDialog NewInstance(string title, string message, System.EventHandler<DialogClickEventArgs> positive, System.EventHandler<DialogClickEventArgs> negative)
        {
            Positive = positive;
            Negative = negative;

            var args = new Bundle();
            args.PutString(ARG_TITLE, title);
            args.PutString(ARG_MESSAGE, message);
            return new ConfirmationDialog { Arguments = args };
        }
        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            _activity = Activity as AppCompatActivity;
            return new AlertDialog.Builder(_activity)
                .SetTitle(Arguments.GetString(ARG_TITLE))
                .SetMessage(Arguments.GetString(ARG_MESSAGE))
                .SetPositiveButton(Positive == null? Android.Resource.String.Ok: Resource.String.yes, Positive ?? ((s, e) => { return; }))
                .SetNegativeButton(Negative == null ? Android.Resource.String.Cancel: Resource.String.no, Negative ?? ((s, e) => { return; }))
                .Create();
        }
        #endregion
    }
}