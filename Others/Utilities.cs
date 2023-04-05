using Android.Content;
using Android.Graphics;
using Java.IO;
using Java.Lang;
using System.Threading.Tasks;

namespace SistemaColeta.Others
{
    public class Utilities
    {
        public static async System.Threading.Tasks.Task<string> ReadStreamTextFileAsync(InputStream inputStream)
        {
            try
            {
                ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();

                int i = inputStream.Read();
                while (i != -1)
                {
                    await Task.Run(() => byteArrayOutputStream.Write(i));
                    await Task.Run(() => i = inputStream.Read());
                }
                inputStream.Close();

                return byteArrayOutputStream.ToString();
            }
            catch (IOException)
            {
                return null;
            }
        }
        public static string ReadRawTextFile(Context context, int resId)
        {
            try
            {
                InputStream inputStream = ((Android.Runtime.InputStreamInvoker)context.Resources.OpenRawResource(resId)).BaseInputStream;
                ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();

                int i = inputStream.Read();
                while (i != -1)
                {
                    byteArrayOutputStream.Write(i);
                    i = inputStream.Read();
                }
                inputStream.Close();

                return byteArrayOutputStream.ToString();
            }
            catch (IOException)
            {
                return null;
            }
        }
        public static int AdjustAlpha(int color, float factor)
        {
            int alpha = Math.Round(Color.GetAlphaComponent(color) * factor);
            int red = Color.GetRedComponent(color);
            int green = Color.GetGreenComponent(color);
            int blue = Color.GetBlueComponent(color);

            return Color.Argb(alpha, red, green, blue);
        }
    }
}