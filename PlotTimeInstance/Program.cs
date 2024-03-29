﻿using PlotDrift;
using ScottPlot;

string rootFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
const string fontName = "Times New Roman";

// Define the culture for parsing dates in the folder name
var sampleRate = 65536.0;
var fftBinSize = 256;

List<double> crossCorrelationValues;
double[] refernceSamples, syncSamples;

// Process each CSV file and calculate delays
var csvFilePath = Path.Combine(rootFolder, "raw_data GPS.csv");
if (File.Exists(csvFilePath))
{
    (_, refernceSamples, syncSamples) = Csv.Parse(csvFilePath);
    crossCorrelationValues = CalculateDelay.UsingCrossCorrelation(refernceSamples, syncSamples, fftBinSize);
}
else
{
    throw new FileNotFoundException($"No file named raw_data.csv found in folder {rootFolder}");
}

// Output the summary information
Console.WriteLine($"Delay calculated: {CalculateDelay.TimeValue(crossCorrelationValues, fftBinSize, sampleRate) * 1000000} us");

// Plot the delay
var timeValuePlot = PlotTimeValues(refernceSamples, syncSamples, sampleRate);
SavePlot(timeValuePlot, "TimePlot");

var correlationValuePlot = PlotCorrelationValues(crossCorrelationValues);
SavePlot(correlationValuePlot, "CorrelationPlot");
Console.ReadKey();

static Plot PlotTimeValues(double[] referenceSamples, double[] syncSamples, double sampleRate)
{
    var xValues = new double[referenceSamples.Length];
    for (var index = 0; index < xValues.Length; index++)
    {
        xValues[index] = index / (sampleRate / 1000);
    }

    // Create a new ScottPlot plot
    var plot = new Plot();
    var signalProperties = plot.Add.Scatter(xValues, referenceSamples);
    signalProperties.Label = "System 1 Channel";
    signalProperties = plot.Add.Scatter(xValues, syncSamples);
    signalProperties.Label = "System 2 Channel";

    // Customize the plot style
    plot.Title("Time delay measured between two DAQ systems", size: 36);
    plot.Axes.Title.Label.FontName = fontName;

    plot.XLabel("Time in milliseconds (ms)", size: 32);
    plot.Axes.Bottom.Label.FontName = fontName;
    plot.Axes.Bottom.TickLabelStyle.FontSize = 20;
    plot.Axes.Bottom.TickLabelStyle.FontName = fontName;

    plot.YLabel("Voltage (V)", size: 32);
    plot.Axes.Left.Label.FontName = fontName;
    plot.Axes.Left.TickLabelStyle.FontSize = 20;
    plot.Axes.Left.TickLabelStyle.FontName = fontName;

    plot.Legend.IsVisible = true;
    plot.Legend.Location = Alignment.LowerRight;
    plot.Legend.Font.Size = 28;
    plot.Legend.Font.Name = fontName;
    return plot;
}

static Plot PlotCorrelationValues(List<double> samples)
{
    // Create a new ScottPlot plot
    var plot = new Plot();
    var signalProperties = plot.Add.Signal(samples, 1);
    signalProperties.Label = "DFT Cross-correlation";

    // Customize the plot style
    plot.Title("Time delay visualised using cross-correlation", size: 36);
    plot.Axes.Title.Label.FontName = fontName;

    plot.XLabel("Delay bin", size: 32);
    plot.Axes.Bottom.Label.FontName = fontName;
    plot.Axes.Bottom.TickLabelStyle.FontSize = 20;
    plot.Axes.Bottom.TickLabelStyle.FontName = fontName;

    plot.YLabel("Correlation Coeficient", size: 32);
    plot.Axes.Left.Label.FontName = fontName;
    plot.Axes.Left.TickLabelStyle.FontSize = 20;
    plot.Axes.Left.TickLabelStyle.FontName = fontName;
    return plot;
}

static void SavePlot(Plot plot, string fileName)
{
    // Save the plot as an image file within the specified folder
    var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Result");
    string fileFullPath = Path.Combine(outputDirectory, fileName);
    if (Directory.Exists(outputDirectory) == false)
    {
        Directory.CreateDirectory(outputDirectory);
    }

    plot.SaveSvg(fileFullPath + ".svg", 1400, 700);
    plot.SavePng(fileFullPath + ".png", 1400, 700);

    // Notify the user where the plot has been saved
    Console.WriteLine($"Plot saved to: {fileFullPath}");
}
