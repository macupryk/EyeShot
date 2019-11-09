using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using devDept.Eyeshot;
using devDept.Graphics;

namespace WpfApplication1
{
    public class ColorTableToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retVal = string.Empty;
            if (value is ObservableCollection<Brush>)
            {
                switch (((ObservableCollection<Brush>)value).Count)
                {
                    case 9:
                        {
                            retVal = "Red to Blue 9";
                            break;
                        }
                    case 17:
                        {
                            retVal = "Red to Blue 17";
                            break;
                        }
                    case 33:
                        {
                            retVal = "Red to Blue 33";
                            break;
                        }
                }
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }        

    public class ViewportEnumsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is backgroundStyleType)
            {
                return ((backgroundStyleType)value).ToString();
            }

            if (value is coordinateSystemPositionType)
            {
                return ((coordinateSystemPositionType)value).ToString();
            }

            if (value is originSymbolStyleType)
            {
                return ((originSymbolStyleType)value).ToString();
            }

            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Windows.Media.Color)
            {
                return Helper.ConvertColor((System.Windows.Media.Color)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public static class Helper
    {
        public static Color ConvertColor(Brush brush)
        {
            SolidColorBrush newBrush = (SolidColorBrush)brush;
            return newBrush.Color;
        }

        public static Brush ConvertColor(Color color)
        {
            return new SolidColorBrush(color);
        }
    }
}
