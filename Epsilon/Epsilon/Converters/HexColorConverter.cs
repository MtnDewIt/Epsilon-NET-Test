using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Epsilon.Converters
{
	public class HexColorConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			string hex = value == null ? "#AA6611" : (string)value;

			Color color = (Color)ColorConverter.ConvertFromString(hex);

			var cs = new ColorState();
			cs.SetARGB(color.A / 255f, color.R / 255f, color.G / 255f, color.B / 255f);
			return cs;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			byte[] bytes = ColorStateToBytes((ColorState)value);
			string hex = "#";

			foreach (var b in bytes)
				hex += b.ToString("X2");

			return hex;
		}

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return this;
		}

		public float[] ByteToNormalizedFloat(byte[] byteValues) {
			float[] newVals = new float[byteValues.Length];

			for (int i = 0; i < byteValues.Length; i++)
				newVals[i] = System.Convert.ToSingle((byte)byteValues[i]) / 255;

			return newVals;
		}

		public byte[] ColorStateToBytes(ColorState cs) {
			double[] oldVals = new double[] { cs.RGB_R, cs.RGB_G, cs.RGB_B };
			byte[] newVals = new byte[3];

			for (int i = 0; i < 3; i++)
				newVals[i] = System.Convert.ToByte(oldVals[i] * 255);

			return newVals;
		}
	}

	public struct ColorState
	{
		private double _RGB_R;

		private double _RGB_G;

		private double _RGB_B;

		private double _A;

		private double _HSV_H;

		private double _HSV_S;

		private double _HSV_V;

		private double _HSL_H;

		private double _HSL_S;

		private double _HSL_L;

		public double A {
			get {
				return _A;
			}
			set {
				_A = value;
			}
		}

		public double RGB_R {
			get {
				return _RGB_R;
			}
			set {
				_RGB_R = value;
				RecalculateHSVFromRGB();
				RecalculateHSLFromRGB();
			}
		}

		public double RGB_G {
			get {
				return _RGB_G;
			}
			set {
				_RGB_G = value;
				RecalculateHSVFromRGB();
				RecalculateHSLFromRGB();
			}
		}

		public double RGB_B {
			get {
				return _RGB_B;
			}
			set {
				_RGB_B = value;
				RecalculateHSVFromRGB();
				RecalculateHSLFromRGB();
			}
		}

		public double HSV_H {
			get {
				return _HSV_H;
			}
			set {
				_HSV_H = value;
				RecalculateRGBFromHSV();
				RecalculateHSLFromHSV();
			}
		}

		public double HSV_S {
			get {
				return _HSV_S;
			}
			set {
				_HSV_S = value;
				RecalculateRGBFromHSV();
				RecalculateHSLFromHSV();
			}
		}

		public double HSV_V {
			get {
				return _HSV_V;
			}
			set {
				_HSV_V = value;
				RecalculateRGBFromHSV();
				RecalculateHSLFromHSV();
			}
		}

		public double HSL_H {
			get {
				return _HSL_H;
			}
			set {
				_HSL_H = value;
				RecalculateRGBFromHSL();
				RecalculateHSVFromHSL();
			}
		}

		public double HSL_S {
			get {
				return _HSL_S;
			}
			set {
				_HSL_S = value;
				RecalculateRGBFromHSL();
				RecalculateHSVFromHSL();
			}
		}

		public double HSL_L {
			get {
				return _HSL_L;
			}
			set {
				_HSL_L = value;
				RecalculateRGBFromHSL();
				RecalculateHSVFromHSL();
			}
		}

		public ColorState(double rGB_R, double rGB_G, double rGB_B, double a, double hSV_H, double hSV_S, double hSV_V, double hSL_h, double hSL_s, double hSL_l) {
			_RGB_R = rGB_R;
			_RGB_G = rGB_G;
			_RGB_B = rGB_B;
			_A = a;
			_HSV_H = hSV_H;
			_HSV_S = hSV_S;
			_HSV_V = hSV_V;
			_HSL_H = hSL_h;
			_HSL_S = hSL_s;
			_HSL_L = hSL_l;
		}

		public void SetARGB(double a, double r, double g, double b) {
			_A = a;
			_RGB_R = r;
			_RGB_G = g;
			_RGB_B = b;
			RecalculateHSVFromRGB();
			RecalculateHSLFromRGB();
		}

		private void RecalculateHSLFromRGB() {
			Tuple<double, double, double> tuple = ColorSpaceHelper.RgbToHsl(_RGB_R, _RGB_G, _RGB_B);
			double item = tuple.Item1;
			double item2 = tuple.Item2;
			double item3 = tuple.Item3;
			if (item != -1.0) {
				_HSL_H = item;
			}

			if (item2 != -1.0) {
				_HSL_S = item2;
			}

			_HSL_L = item3;
		}

		private void RecalculateHSLFromHSV() {
			Tuple<double, double, double> tuple = ColorSpaceHelper.HsvToHsl(_HSV_H, _HSV_S, _HSV_V);
			double item = tuple.Item1;
			double item2 = tuple.Item2;
			double item3 = tuple.Item3;
			_HSL_H = item;
			if (item2 != -1.0) {
				_HSL_S = item2;
			}

			_HSL_L = item3;
		}

		private void RecalculateHSVFromRGB() {
			Tuple<double, double, double> tuple = ColorSpaceHelper.RgbToHsv(_RGB_R, _RGB_G, _RGB_B);
			double item = tuple.Item1;
			double item2 = tuple.Item2;
			double item3 = tuple.Item3;
			if (item != -1.0) {
				_HSV_H = item;
			}

			if (item2 != -1.0) {
				_HSV_S = item2;
			}

			_HSV_V = item3;
		}

		private void RecalculateHSVFromHSL() {
			Tuple<double, double, double> tuple = ColorSpaceHelper.HslToHsv(_HSL_H, _HSL_S, _HSL_L);
			double item = tuple.Item1;
			double item2 = tuple.Item2;
			double item3 = tuple.Item3;
			_HSV_H = item;
			if (item2 != -1.0) {
				_HSV_S = item2;
			}

			_HSV_V = item3;
		}

		private void RecalculateRGBFromHSL() {
			Tuple<double, double, double> tuple = ColorSpaceHelper.HslToRgb(_HSL_H, _HSL_S, _HSL_L);
			_RGB_R = tuple.Item1;
			_RGB_G = tuple.Item2;
			_RGB_B = tuple.Item3;
		}

		private void RecalculateRGBFromHSV() {
			Tuple<double, double, double> tuple = ColorSpaceHelper.HsvToRgb(_HSV_H, _HSV_S, _HSV_V);
			_RGB_R = tuple.Item1;
			_RGB_G = tuple.Item2;
			_RGB_B = tuple.Item3;
		}
	}

	static class ColorSpaceHelper
	{
		public static Tuple<double, double, double> RgbToHsv(double r, double g, double b) {
			double num = Math.Min(r, Math.Min(g, b));
			double num2 = Math.Max(r, Math.Max(g, b));
			double item = num2;
			double num3 = num2 - num;
			double item2;
			double num4;
			if (num2 != 0.0) {
				item2 = num3 / num2;
				num4 = ( ( r == num2 ) ? ( ( g - b ) / num3 ) : ( ( g != num2 ) ? ( 4.0 + ( r - g ) / num3 ) : ( 2.0 + ( b - r ) / num3 ) ) );
				num4 *= 60.0;
				if (num4 < 0.0) {
					num4 += 360.0;
				}

				if (double.IsNaN(num4)) {
					num4 = -1.0;
				}

				return new Tuple<double, double, double>(num4, item2, item);
			}

			item2 = -1.0;
			num4 = -1.0;
			return new Tuple<double, double, double>(num4, item2, item);
		}

		public static Tuple<double, double, double> RgbToHsl(double r, double g, double b) {
			double num = Math.Min(Math.Min(r, g), b);
			double num2 = Math.Max(Math.Max(r, g), b);
			double num3 = num2 - num;
			double num4 = (num2 + num) / 2.0;
			if (num2 == 0.0) {
				return new Tuple<double, double, double>(-1.0, -1.0, 0.0);
			}

			if (num3 == 0.0) {
				return new Tuple<double, double, double>(-1.0, 0.0, num4);
			}

			double item = ((num4 <= 0.5) ? (num3 / (num2 + num)) : (num3 / (2.0 - num2 - num)));
			double num5 = ((r == num2) ? ((g - b) / 6.0 / num3) : ((g != num2) ? (0.66666668653488159 + (r - g) / 6.0 / num3) : (0.3333333432674408 + (b - r) / 6.0 / num3)));
			if (num5 < 0.0) {
				num5 += 1.0;
			}

			if (num5 > 1.0) {
				num5 -= 1.0;
			}

			num5 *= 360.0;
			return new Tuple<double, double, double>(num5, item, num4);
		}

		public static Tuple<double, double, double> HsvToRgb(double h, double s, double v) {
			if (s == 0.0) {
				return new Tuple<double, double, double>(v, v, v);
			}

			if (h >= 360.0) {
				h = 0.0;
			}

			h /= 60.0;
			int num = (int)h;
			double num2 = h - (double)num;
			double num3 = v * (1.0 - s);
			double num4 = v * (1.0 - s * num2);
			double num5 = v * (1.0 - s * (1.0 - num2));
			switch (num) {
				case 0: return new Tuple<double, double, double>(v, num5, num3);
				case 1: return new Tuple<double, double, double>(num4, v, num3);
				case 2: return new Tuple<double, double, double>(num3, v, num5);
				case 3: return new Tuple<double, double, double>(num3, num4, v);
				case 4: return new Tuple<double, double, double>(num5, num3, v);
				default: return new Tuple<double, double, double>(v, num3, num4);
			}
		}

		public static Tuple<double, double, double> HsvToHsl(double h, double s, double v) {
			double num = v * (1.0 - s / 2.0);
			double item = ((num != 0.0 && num != 1.0) ? ((v - num) / Math.Min(num, 1.0 - num)) : (-1.0));
			return new Tuple<double, double, double>(h, item, num);
		}

		public static Tuple<double, double, double> HslToRgb(double h, double s, double l) {
			int num = (int)(h / 60.0);
			double num2 = (h - (double)(60 * num)) / 60.0;
			double num3 = ((l < 0.5) ? (l * (1.0 + s)) : (l + s - l * s));
			double num4 = 2.0 * l - num3;
			double num5 = num3 - num4;
			switch (num) {
				case 0: return new Tuple<double, double, double>(num3, num5 * num2 + num4, num4);
				case 1: return new Tuple<double, double, double>(num5 * ( 1.0 - num2 ) + num4, num3, num4);
				case 2: return new Tuple<double, double, double>(num4, num3, num5 * num2 + num4);
				case 3: return new Tuple<double, double, double>(num4, num5 * ( 1.0 - num2 ) + num4, num3);
				case 4: return new Tuple<double, double, double>(num5 * num2 + num4, num4, num3);
				default: return new Tuple<double, double, double>(num3, num4, num5 * ( 1.0 - num2 ) + num4);
			}
		}

		public static Tuple<double, double, double> HslToHsv(double h, double s, double l) {
			double num = l + s * Math.Min(l, 1.0 - l);
			double item = ((num != 0.0) ? (2.0 * (1.0 - l / num)) : (-1.0));
			return new Tuple<double, double, double>(h, item, num);
		}
	}

}
