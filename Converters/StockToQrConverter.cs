using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using BloodBankPro.Models;
using ZXing;

namespace BloodBankPro.Converters
{
    public class StockToQrConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is BloodStock stock)
            {
                var writer = new ZXing.Windows.Compatibility.BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 150,
                        Width = 150,
                        Margin = 0
                    }
                };

                string content = $"BloodBankPro | Unit ID: {stock.Id} | Type: {stock.BloodType}";
                
                using (var bitmap = writer.Write(content))
                {
                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        stream.Position = 0;
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        return bitmapImage;
                    }
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
