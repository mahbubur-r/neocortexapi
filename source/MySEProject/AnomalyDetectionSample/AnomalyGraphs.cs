using XPlot.Plotly;

namespace AnomalyDetectionSample
{
    public class AnomalyGraphs
    {
        public static void CompareGraphForSequences(List<double[]> allLearnedData, List<double[]> allTestingData)
        {
            List<Scatter> allGraphs = new List<Scatter>();

            for (int i = 0; i < allTestingData.Count; i++)
            {
                double[] data = allTestingData[i];
                double[] learnedData = allLearnedData[i];

                // Plot actual data sequence
                var actualGraph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(),
                    y = data,
                    mode = "lines",
                    name = $"Training Sequence {i + 1}",
                    line = new Line { color = "blue" } // Solid blue line for actual data
                };

                // Plot predicted (learned) sequence
                var learnedGraph = new Scatter
                {
                    x = Enumerable.Range(0, learnedData.Length).ToArray(),
                    y = learnedData,
                    mode = "lines",
                    name = $"Predicting Sequence {i + 1}",
                    line = new Line { color = "green", dash = "dashdot" } // Green dashed line for predicted data
                };

                allGraphs.Add(actualGraph);
                allGraphs.Add(learnedGraph);
            }

            // Create and configure the plot
            var chart = Chart.Plot(allGraphs);
            chart.WithTitle("Comparison of Actual and Testing Sequences");
            chart.WithXTitle("X-axis (Index in Sequence)");
            chart.WithYTitle("Y-axis (Value of Sequence)");

            // Define output directory and file path
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string outputDirectory = Path.Combine(projectRoot!, "result", "plots");
            Directory.CreateDirectory(outputDirectory); // Ensure directory exists

            string filePath = Path.Combine(outputDirectory, $"Actual_Testing_Sequence_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            // Save the chart as an HTML file
            File.WriteAllText(filePath, chart.GetHtml());

            // Display the chart in a browser
            chart.Show();
        }

        public static void CompareBothSequenceWithAnomalies(List<double[]> allLearnedData, List<double[]> allTestingData, List<List<int>> allAnomalyIndices)
        {
            List<Scatter> allGraphs = new List<Scatter>();
            List<Scatter> allAnomalies = new List<Scatter>();

            for (int i = 0; i < allTestingData.Count; i++)
            {
                double[] data = allTestingData[i];
                double[] learnedData = allLearnedData[i];
                List<int> anomalyIndices = allAnomalyIndices[i];

                // Plot actual data sequence
                var actualGraph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(),
                    y = data,
                    mode = "lines",
                    name = $"Testing Sequence {i + 1}",
                    line = new Line { color = "green" }
                };

                // Plot predicted (learned) sequence
                var learnedGraph = new Scatter
                {
                    x = Enumerable.Range(0, learnedData.Length).ToArray(),
                    y = learnedData,
                    mode = "lines",
                    name = $"Learned Sequence {i + 1}",
                    line = new Line { color = "orange", dash = "dashdot" } // Dashed line for learned data
                };

                allGraphs.Add(actualGraph);
                allGraphs.Add(learnedGraph);

                // Plot anomalies as red markers
                var anomalyGraph = new Scatter
                {
                    x = anomalyIndices.Select(idx => (double)idx).ToArray(),
                    y = anomalyIndices.Select(idx => data[idx]).ToArray(),
                    mode = "markers",
                    name = $"Anomalies in Sequence {i + 1}",
                    marker = new Marker { color = "crimson", size = 8 }
                };

                allAnomalies.Add(anomalyGraph);
            }

            // Create and configure the plot
            var chart = Chart.Plot(allGraphs.Concat(allAnomalies));
            chart.WithTitle("Plot for Learned Sequences and Testing Sequences with Anomalies");
            chart.WithXTitle("X-axis (Index in Sequence)");
            chart.WithYTitle("Y-axis (Value of Sequence)");

            // Define output directory and file path
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string outputDirectory = Path.Combine(projectRoot!, "result", "plots");
            Directory.CreateDirectory(outputDirectory); // Ensure directory exists

            string filePath = Path.Combine(outputDirectory, $"Actual_Testing_Sequence_anomaly_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            // Save the chart as an HTML file
            File.WriteAllText(filePath, chart.GetHtml());

            // Display the chart in a browser
            chart.Show();
        }

    }
}
