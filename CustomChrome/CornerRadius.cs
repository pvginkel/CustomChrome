using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    [Serializable]
    [TypeConverter(typeof(CornerRadiusConverter))]
    public struct CornerRadius : IEquatable<CornerRadius>
    {
        public static readonly CornerRadius Empty = new CornerRadius(0);

        private readonly float _topLeft;
        private readonly float _topRight;
        private readonly float _bottomLeft;
        private readonly float _bottomRight;

        public float TopLeft { get { return _topLeft; } }
        public float TopRight { get { return _topRight; } }
        public float BottomLeft { get { return _bottomLeft; } }
        public float BottomRight { get { return _bottomRight; } }

        public float All
        {
            get
            {
                if (ShouldSerializeAll())
                    return _topLeft;
                else
                    return float.NaN;
            }
        }

        public CornerRadius(float uniformRadius)
            : this(uniformRadius, uniformRadius, uniformRadius, uniformRadius)
        {
        }

        public CornerRadius(float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            _topLeft = topLeft;
            _topRight = topRight;
            _bottomLeft = bottomLeft;
            _bottomRight = bottomRight;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CornerRadius))
                return false;

            return Equals((CornerRadius)obj);
        }

        public bool Equals(CornerRadius other)
        {
            return
                _topLeft == other._topLeft &&
                _topRight == other._topRight &&
                _bottomLeft == other._bottomLeft &&
                _bottomRight == other._bottomRight;
        }

        public override int GetHashCode()
        {
            return ObjectUtil.CombineHashCodes(
                _topLeft.GetHashCode(),
                _topRight.GetHashCode(),
                _bottomLeft.GetHashCode(),
                _bottomRight.GetHashCode()
            );
        }

        public static bool operator ==(CornerRadius a, CornerRadius b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CornerRadius a, CornerRadius b)
        {
            return !(a == b);
        }

        internal bool ShouldSerializeAll()
        {
            return
                _topLeft == _topRight &&
                _topLeft == _bottomLeft &&
                _topLeft == _bottomRight;
        }

        public class CornerRadiusConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(InstanceDescriptor))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string valueStr = value as string;

                if (valueStr != null)
                {
                    valueStr = valueStr.Trim();

                    if (valueStr.Length == 0)
                    {
                        return null;
                    }
                    else
                    {
                        if (culture == null)
                        {
                            culture = CultureInfo.CurrentCulture;
                        }

                        char sep = culture.TextInfo.ListSeparator[0];
                        string[] tokens = valueStr.Split(new[] { sep });
                        float[] values = new float[tokens.Length];
                        
                        var floatConverter = TypeDescriptor.GetConverter(typeof(float));

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = (float)floatConverter.ConvertFromString(context, culture, tokens[i]);
                        }

                        if (values.Length == 4)
                            return new CornerRadius(values[0], values[1], values[2], values[3]);
                        else
                            return new CornerRadius(values[0]);
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw new ArgumentNullException("destinationType");
                }

                if (!(value is CornerRadius) && value.GetType().FullName == typeof(CornerRadius).FullName)
                    value = ReflectionCornerRadius(value);

                if (value is CornerRadius)
                {
                    if (destinationType == typeof(string))
                    {
                        CornerRadius cornerRadius = (CornerRadius)value;

                        if (culture == null)
                            culture = CultureInfo.CurrentCulture;

                        string sep = culture.TextInfo.ListSeparator + " ";
                        TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
                        string[] args = new string[4];
                        int nArg = 0;

                        // Note: ConvertToString will raise exception if value cannot be converted.
                        args[nArg++] = floatConverter.ConvertToString(context, culture, cornerRadius.TopLeft);
                        args[nArg++] = floatConverter.ConvertToString(context, culture, cornerRadius.TopRight);
                        args[nArg++] = floatConverter.ConvertToString(context, culture, cornerRadius.BottomLeft);
                        args[nArg] = floatConverter.ConvertToString(context, culture, cornerRadius.BottomRight);

                        return string.Join(sep, args);
                    }
                    else if (destinationType == typeof(InstanceDescriptor))
                    {
                        var cornerRadius = (CornerRadius)value;

                        if (cornerRadius.ShouldSerializeAll())
                        {
                            return new InstanceDescriptor(
                                typeof(CornerRadius).GetConstructor(new[] { typeof(float) }),
                                new object[] { cornerRadius.All }
                            );
                        }
                        else
                        {
                            return new InstanceDescriptor(
                                typeof(CornerRadius).GetConstructor(new[] { typeof(float), typeof(float), typeof(float), typeof(float) }),
                                new object[] { cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomLeft, cornerRadius.BottomRight }
                            );
                        }
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            private CornerRadius ReflectionCornerRadius(object value)
            {
                return new CornerRadius(
                    (float)value.GetType().GetProperty("TopLeft").GetValue(value, null),
                    (float)value.GetType().GetProperty("TopRight").GetValue(value, null),
                    (float)value.GetType().GetProperty("BottomLeft").GetValue(value, null),
                    (float)value.GetType().GetProperty("BottomRight").GetValue(value, null)
                );
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
                if (propertyValues == null)
                {
                    throw new ArgumentNullException("propertyValues");
                }

                CornerRadius original = (CornerRadius)context.PropertyDescriptor.GetValue(context.Instance);

                float all = (float)propertyValues["All"];

                bool allEquals =
                    (double.IsNaN(original.All) && double.IsNaN(all)) ||
                    (!double.IsNaN(original.All) && !double.IsNaN(all) && original.All == all);

                if (!allEquals)
                {
                    return new CornerRadius(all);
                }
                else
                {
                    return new CornerRadius(
                        (float)propertyValues["TopLeft"],
                        (float)propertyValues["TopRight"],
                        (float)propertyValues["BottomLeft"],
                        (float)propertyValues["BottomRight"]
                    );
                }
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                var props = TypeDescriptor.GetProperties(typeof(CornerRadius), attributes);

                return props.Sort(new[] { "All", "TopLeft", "TopRight", "BottomLeft", "BottomRight" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
