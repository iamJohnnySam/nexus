using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using ScottPlot.Plottables;

namespace SimulatorSequenceConsole
{
    public class ThroughputPlotter(string layoutName, Dictionary<int, (float, float, float, float)> data)
    {
        private readonly Dictionary<int, (float, float, float, float)> _data = data;
        private readonly int _width = 1181;  // Approx 30cm at 100 DPI
        private readonly int _height = 591;  // Approx 15cm at 100 DPI
        private readonly string LayoutName = layoutName;

        public void PlotGraph(string savePath)
        {
            var plt = new Plot();
            //plt.SetSize(_width, _height);

            List<int> xValues = new(_data.Keys);
            List<float>[] yValues = new List<float>[4];

            for (int i = 0; i < 4; i++)
                yValues[i] = [];

            foreach (var entry in _data)
            {
                var (v1, v2, v3, v4) = entry.Value;
                yValues[0].Add(v1);
                yValues[1].Add(v2);
                yValues[2].Add(v3);
                yValues[3].Add(v4);
            }

            ScottPlot.Color[] colors = [new(255, 0, 0), new(0, 0, 255), new(0, 128, 0), new(255, 165, 0)];
            string[] labels = ["Throughput @Load", "Throughput Running Average", "Steady-state Throughput @Load", "Steady-state Throughput Running Average"];

            for (int i = 0; i < 4; i++)
            {
                var scatter = plt.Add.Scatter(xValues.ConvertAll(x => (double)x).ToArray(),
                               [.. yValues[i].ConvertAll(y => (double)y)],
                               color: colors[i]);
                scatter.LineWidth = 2; // Set line width separately
                scatter.LegendText = labels[i];
            }

            plt.ShowLegend();
            plt.Title($"Throughput Over Time (Layout: {LayoutName})");
            plt.XLabel("Time");
            plt.YLabel("Throughput");

            plt.SavePng(savePath, _width, _height);
        }
    }
}
