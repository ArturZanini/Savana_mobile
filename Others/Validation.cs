using AndroidX.AppCompat.App;
using Google.Android.Material.TextField;
using Java.Util.Regex;
using System;

namespace SistemaColeta.Others
{
    public class Validation
    {
        private AppCompatActivity _activity;

        public Validation(AppCompatActivity activity)
        {
            _activity = activity;
        }

        public bool ValidateUser(TextInputLayout til)
        {
            Pattern pattern = Pattern.Compile("^[A-Za-záàâãéèêíïóôõöúçñÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ 0-9]+$");
            if (!pattern.Matcher(til.EditText.Text).Matches())
            {
                til.Error = _activity.GetString(Resource.String.invalid_user);
                return false;
            }
            else
            {
                til.Error = null;
                return true;
            }
        }
        public bool ValidatePassword(TextInputLayout til)
        {
            if (til.EditText.Text.Length < 8)
            {
                til.Error = _activity.GetString(Resource.String.invalid_password);
                return false;
            }
            else
            {
                til.Error = null;
                return true;
            }
        }
        public bool ValidateField(TextInputLayout til)
        {
            if (string.IsNullOrWhiteSpace(til.EditText.Text))
            {
                til.Error = _activity.GetString(Resource.String.required_field);
                return false;
            }
            else
            {
                til.Error = null;
                return true;
            }
        }
        public bool ValidateDate(TextInputLayout til)
        {
            if (!DateTime.TryParse(til.EditText.Text, out DateTime dateTime))
            {
                til.Error = _activity.GetString(Resource.String.invalid_date);
                return false;
            }
            else
            {
                til.Error = null;
                return true;
            }
        }
    }
}