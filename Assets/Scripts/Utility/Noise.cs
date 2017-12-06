using System;
using UnityEngine;

public static class Noise {

	public static int Seed { get; set; }

	public static double Frequency { get; set; }
	public static double Amplitude { get; set; }
	public static double Lacunarity { get; set; }
	public static double Persistance { get; set; }
	public static int Octaves { get; set; }

	static Noise() {
		Seed = unchecked((int)(DateTime.Now.Ticks));
		Frequency = 1;
		Amplitude = 1;
		Lacunarity = 2;
		Persistance = 0.5;
		Octaves = 2;
	}

	public static double Value2D(double x, double y) {
		double ret = 0.0;
		double freq = Frequency;
		double ampl = Amplitude;

		for (int i = 0; i < Octaves; i++) {
			ret += Interpolated2D(x * freq, y * freq) * ampl;
			freq *= Lacunarity;
			ampl *= Persistance;
		}

		return ret;
	}

	private static double Interpolated2D(double x, double y) {
		// find grid cell coordinates
		int x0 = Mathf.FloorToInt((float)x);
		int x1 = x0 + 1;
		int y0 = Mathf.FloorToInt((float)y);
		int y1 = y0 + 1;

		// interpolation weights
		double sx = x - x0;
		double sy = y - y0;

		// sample some noise
		double n0 = Smooth2D(x0, y0);
		double n1 = Smooth2D(x1, y0);
		double n2 = Smooth2D(x0, y1);
		double n3 = Smooth2D(x1, y1);
		// and inteprolate between the corners
		double ix0 = InterpolateLinear(n0, n1, sx);
		double ix1 = InterpolateLinear(n2, n3, sx);
		return InterpolateLinear(ix0, ix1, sy);
	}

	private static double Smooth2D(double x, double y) {
		double x0 = x - 1;
		double x1 = x + 1;
		double y0 = y - 1;
		double y1 = y + 1;

		double corners = (Sample2D(x0, y0) + Sample2D(x1, y0) + Sample2D(x0, y1) + Sample2D(x1, y1)) / 16.0;
		double sides = (Sample2D(x0, y) + Sample2D(x1, y) + Sample2D(x, y0) + Sample2D(x, y1)) / 8.0;
		double center = Sample2D(x, y) / 4.0;

		return corners + sides + center;
	}

	private static double Sample2D(double x, double y) {
		int N = ((int)x * 1619 + (int)y * 31337 * 1013 * Seed) & 0x7fffffff;
		N = (N << 13) ^ N;
		return 1.0 - ((N * (N * N * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0;
	}

	private static double InterpolateLinear(double a, double b, double t) {
		return a * (1.0 - t) + b * t;
	}
}
