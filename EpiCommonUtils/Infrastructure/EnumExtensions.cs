using System;
using EPiServer.Framework.Localization;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public static class EnumExtensions
    {
        public static string GetLocalizedName<TEnum>(this TEnum value) where TEnum : struct, IConvertible
        {
            return GetLocalizedName(value, LocalizationService.Current);
        }
    
        public static string GetLocalizedName<TEnum>(this TEnum value, LocalizationService localizationService) where TEnum : struct, IConvertible
        {
            if (typeof(TEnum).IsEnum == false)
            {
                throw new ArgumentException($"{nameof(TEnum)} must be an enumerated type");
            }

            var staticName = Enum.GetName(typeof(TEnum), value);
            
            var localizationPath = $"/enum/{typeof(TEnum).Name}/{staticName}";
            if (localizationService.TryGetString(localizationPath, out var localizedString))
            {
                return localizedString;
            }

            return string.Empty;
        }
    }
}
