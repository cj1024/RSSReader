using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace RSSReader.Converters
{

    /// <summary>
    /// 日期转字符串
    /// Parameter表StringFormat，默认Format为 DefaultStringFormat = "yyyy-MM-dd"
    /// 支持ConvertBack
    /// </summary>
    public class DateToStringConverter : IValueConverter
    {

        public const string DefaultStringFormat = "yyyy-MM-dd";
        // Define the Convert method to change a DateTime object to 
        // a month string.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var stringformat = string.Format("{{0:{0}}}", string.IsNullOrEmpty(parameter as string) ? DefaultStringFormat : parameter as string);
            return string.Format(stringformat, value);
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var stringformat = string.IsNullOrEmpty(parameter as string) ? DefaultStringFormat : parameter as string;
            var thisdate = (string) value;
            DateTime result;
            DateTime.TryParseExact(thisdate, stringformat, null, DateTimeStyles.None, out result);
            return result;
        }

    }

    /// <summary>
    /// Value取反操作，如true => false， Visible => Collapsed， 9 => -9
    /// Parameter表取反前乘以的倍数，默认为1，只对int，long等数字起作用，如Parameter = 2，则 9 => -18
    /// 支持ConvertBack
    /// </summary>
    public class ValueReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double rate = 1;
            if (null != parameter)
            {
                if (!double.TryParse(parameter.ToString(), out rate))
                {
                    rate = 1;
                }
            }
            try
            {
                if (value is bool)
                {
                    return !(bool) value;
                }
                if (value is double)
                {
                    return 0 - (double) value*rate;
                }
                if (value is int)
                {
                    return 0 - (int) value*rate;
                }
                if (value is decimal)
                {
                    return 0 - (decimal) value*(decimal) rate;
                }
                if (value is float)
                {
                    return 0 - (float) value*rate;
                }
                if (value is Visibility)
                {
                    return ((Visibility) value) == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                }
                return value;
            }
            catch (Exception)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double rate = 1;
            if (null != parameter)
            {
                if (!double.TryParse(parameter.ToString(), out rate))
                {
                    rate = 1;
                }
            }
            try
            {
                if (value is bool)
                {
                    return !(bool) value;
                }
                if (value is double)
                {
                    return 0 - (double) value/rate;
                }
                if (value is int)
                {
                    return 0 - (int) value/rate;
                }
                if (value is decimal)
                {
                    return 0 - (decimal) value/(decimal) rate;
                }
                if (value is float)
                {
                    return 0 - (float) value/rate;
                }
                if (value is Visibility)
                {
                    return ((Visibility) value) == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                }
                return value;
            }
            catch (Exception)
            {
                return value;
            }
        }

    }

    /// <summary>
    /// 根据各种数据类型取得bool值
    /// bool：value
    /// Visibility：value == Visibility.Visible
    /// string：!string.IsNullOrEmpty(value)
    /// int，long等数字：value > 0
    /// 其余：value != null
    /// Parameter表是否取反，Parameter != null表需要取反，默认为null
    /// 不支持ConvertBack
    /// </summary>
    public class BooleanConverterBase : IValueConverter
    {

        protected bool GetBooleanValue(object value, Type targetType, object parameter, string language)
        {
            bool result;
            if (value is bool)
            {
                result = (bool) value;
            }
            else if (value is Visibility)
            {
                result = ((Visibility) value) == Visibility.Visible;
            }
            else if (value is string)
            {
                result = !string.IsNullOrEmpty(string.Format("{0}", value));
            }
            else
            {
                if (!(value is ValueType))
                {
                    result = value != null;
                }
                else
                {
                    try
                    {
                        result = System.Convert.ToDecimal(value) > 0;
                    }
                    catch (InvalidCastException)
                    {
                        result = true;
                    }
                }
            }
            return parameter == null ? result : !result;
        }

        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            return GetBooleanValue(value, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return 0;
        }

    }

    /// <summary>
    /// 封装BooleanConverterBase，Convert结果为true返回TrueValue，否则返回FalseValue
    /// 不支持ConvertBack
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    public abstract class BooleanConverter<T> : BooleanConverterBase
    {

        public T TrueValue { get; set; }

        public T FalseValue { get; set; }

        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            return GetBooleanValue(value, targetType, parameter, language) ? TrueValue : FalseValue;
        }

    }

    /// <summary>
    /// 封装BooleanConverterBase，Convert结果为true返回TrueValue，否则返回FalseValue
    /// 不支持ConvertBack
    /// </summary>
    public class BooleanConverter : BooleanConverter<object>
    {

        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            return GetBooleanValue(value, targetType, parameter, language) ? TrueValue : FalseValue;
        }

    }

}