using System;
using System.Linq;
using System.Windows;

namespace BloodBankPro.Database
{
    public static class LocalizationManager
    {
        private const string LangPath = "Resources/Languages/";

        public static void SetLanguage(string code)
        {
            var uri  = new Uri($"{LangPath}{code}.xaml", UriKind.Relative);
            var dict = new ResourceDictionary { Source = uri };

            var existing = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains(LangPath) == true);

            if (existing != null)
                Application.Current.Resources.MergedDictionaries.Remove(existing);

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
