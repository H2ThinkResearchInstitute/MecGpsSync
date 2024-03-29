﻿using System.Numerics;

namespace PlotDrift;

public static class CalculateDelay
{
    public static List<double> UsingCrossCorrelation(double[] channel1, double[] channel2, int binSize)
    {
        // Ensure the length of both channels is a power of 2
        var fft1 = FFT(channel1, binSize);
        var fft2 = FFT(channel2, binSize);

        // Conjugate and multiply
        var crossCorrelation = new Complex[binSize];
        for (int index = 0; index < binSize; index++)
        {
            crossCorrelation[index] = fft1[index] * Complex.Conjugate(fft2[index]);
        }

        // Inverse FFT to get the cross-correlation series
        FftSharp.FFT.Inverse(crossCorrelation);
        var maxValue = crossCorrelation.Max(value => value.Magnitude);
        return crossCorrelation.Select(value => value.Magnitude / maxValue).ToList();
    }

    public static double TimeValue(List<double> values, int binSize, double sampleRate)
    {
        // Find the index of the maximum value in cross-correlation
        var maxIndex = values.IndexOf(values.Max());

        // Convert index to time delay (consider FFT shift for correct direction)
        var shift = maxIndex > binSize / 2 ? maxIndex - binSize : maxIndex;
        return -shift / (double)sampleRate;
    }

    private static Complex[] FFT(double[] data, int fftLength)
    {
        // Extend or truncate the array to the desired length, filled with zeros if needed
        var complexData = new Complex[fftLength];
        for (int index = 0; index < fftLength; index++)
        {
            if (index < data.Length)
            {
                complexData[index] = new Complex(data[index], 0);
            }
            else
            {
                complexData[index] = new Complex(0, 0);
            }
        }

        // Perform the FFT operation using FftSharp
        FftSharp.FFT.Forward(complexData);
        return complexData;
    }
}
