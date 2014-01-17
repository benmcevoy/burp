using System;
using System.Windows.Data;

namespace Burp.Client
{
    class DefaultAvatarValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                
                var stream = App.GetResourceStream(new Uri("avatar.png", UriKind.Relative)).Stream;
                byte[] output = new byte[stream.Length];
                stream.Read(output, 0, (int)stream.Length);
                return output;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}