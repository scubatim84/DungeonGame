﻿using System;
using System.ComponentModel;
using System.Globalization;

namespace DungeonGame.Coordinates {
	public class CoordinateConverter : TypeConverter {
		// Overrides the CanConvertFrom method of TypeConverter.
		// The ITypeDescriptorContext interface provides the context for the
		// conversion. Typically, this interface is used at design time to 
		// provide information about the design-time container.
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}
		// Overrides the ConvertFrom method of TypeConverter.
		public override object ConvertFrom(ITypeDescriptorContext context,
			CultureInfo culture, object value) {
			if (value is string str) {
				string[] v = str.Split(new char[] { ',' });
				return new Coordinate(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]));
			}
			return base.ConvertFrom(context, culture, value);
		}
		// Overrides the ConvertTo method of TypeConverter.
		public override object ConvertTo(ITypeDescriptorContext context,
			CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				return ((Coordinate)value).X + "," + ((Coordinate)value).Y + "," + ((Coordinate)value).Z;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}