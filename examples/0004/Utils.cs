using System;
using Avalonia.Data.Converters;
using System.Globalization;
using System.Numerics;

namespace SearchAThing.SciExamples
{
    public class MatrixDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            return ((Matrix4x4)value).Fmt(3);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Constants
    {
        public static Matrix4x4 IdentityMatrix { get; set; } = Matrix4x4.Identity;
    }

}