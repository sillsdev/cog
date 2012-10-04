using System;
using System.Windows.Media;

namespace SIL.Cog.Views
{
	public struct HslColor
	{
		public static HslColor FromHsl(double h, double s, double l)
		{
			return new HslColor {A = 1, H = h, S = s, L = l};
		}

		public static HslColor FromColor(Color color)
		{
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

			var hsl = new HslColor {A = color.A / 255.0};
			double v = Math.Max(r, g);
			v = Math.Max(v, b);
			double m = Math.Min(r, g);
			m = Math.Min(m,b);
			hsl.L = (m + v) / 2.0;
			if (hsl.L <= 0.0)
				return hsl;

			double vm = v - m;
			hsl.S = vm;
			if (hsl.S > 0.0)
				hsl.S /= (hsl.L <= 0.5) ? (v + m ) : (2.0 - v - m);
			else
				return hsl;

			double r2 = (v - r) / vm;
			double g2 = (v - g) / vm;
			double b2 = (v - b) / vm;
			if (Math.Abs(r - v) < double.Epsilon)
			{
				hsl.H = (Math.Abs(g - m) < double.Epsilon ? 5.0 + b2 : 1.0 - g2);
			}
			else if (Math.Abs(g - v) < double.Epsilon)
			{
				hsl.H = (Math.Abs(b - m) < double.Epsilon ? 1.0 + r2 : 3.0 - b2);
			}
			else
			{
				hsl.H = (Math.Abs(r - m) < double.Epsilon ? 3.0 + g2 : 5.0 - r2);
			}
			hsl.H /= 6.0;
			return hsl;
		}

		public double A { get; set; }

		public double H { get; set; }

		public double S { get; set; }

		public double L { get; set; }

		public Color ToColor()
		{
			double r, g, b;

			if (Math.Abs(L - 0) < double.Epsilon)
			{
				r = g = b = 0;
			}
			else
			{
				if (Math.Abs(S - 0) < double.Epsilon)
				{
					r = g = b = L;
				}
				else
				{
					double temp2 = ((L <= 0.5) ? L * (1.0 + S) : L + S - (L * S));
					double temp1 = 2.0 * L - temp2;
            
					var t3 = new [] {H + 1.0 / 3.0, H, H - 1.0 / 3.0};
					var clr = new double[]{0, 0, 0};
					for (int i = 0; i < 3; i++)
					{
						if (t3[i] < 0)
							t3[i] += 1.0;
						if (t3[i] > 1)
							t3[i] -= 1.0;
 
						if (6.0 * t3[i] < 1.0)
							clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
						else if(2.0 * t3[i] < 1.0)
							clr[i] = temp2;
						else if(3.0 * t3[i] < 2.0)
							clr[i] = (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);
						else
							clr[i] = temp1;
					}
					r = clr[0];
					g = clr[1];
					b = clr[2];
				}
			}
 
			return Color.FromArgb((byte) (255 * A), (byte) (255 * r), (byte) (255 * g), (byte) (255 * b));
		}
	}
}
