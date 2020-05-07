using System;
using System.ComponentModel;
using System.Globalization;

namespace App.Core.Models
{
    [TypeConverter(typeof(KenticoVersionConverter))]
    public class KenticoVersion
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Hotfix { get; set; }

        public KenticoVersion(string version)
        {
            if (Version.TryParse(version, out var result))
            {
                Major = result.Major;
                Minor = result.Minor;
                Hotfix = result.Build;
            }
            else if (Version.TryParse($"{version}.0", out result))
            {
                Major = result.Major;
                Minor = result.Minor;
                Hotfix = result.Build;
            }
            else
            {
                throw new ArgumentException($"'{version}' is not a valid version or partial version.");
            }
        }

        public KenticoVersion(int major) : this(major.ToString())
        {
        }

        public override string ToString() => $"{Major}.{Minor}.{Hotfix}";
    }

    public class KenticoVersionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                return new KenticoVersion(stringValue);
            }

            throw new ArgumentException($"'{value}' is not a string.");
        }
    }
}