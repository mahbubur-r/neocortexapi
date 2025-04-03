using XPlot.Plotly;

namespace AnomalyDetectionSample
{
    public class AnomalyGraphs
    {
        /// <summary>
        /// Compares and visualizes actual (testing) and learned (predicted) sequences using line plots.
        /// The method generates an interactive HTML file and also displays the plot in a browser.
        /// </summary>
        /// <param name="allLearnedData">List of predicted sequences.</param>
        /// <param name="allTestingData">List of actual testing sequences.</param>
        public static void CompareGraphForSequences(List<double[]> allLearnedData, List<double[]> allTestingData)
        {
            // List to hold all the graph data (both actual and learned sequences)
            List<Scatter> allGraphs = new List<Scatter>();

            // Loop through each testing data sequence
            for (int i = 0; i < allTestingData.Count; i++)
            {
                // Get the current testing data sequence
                double[] data = allTestingData[i];
                // Get the corresponding learned (predicted) data sequence
                double[] learnedData = allLearnedData[i];

                // Plot actual data sequence (testing data)
                var actualGraph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(),  // x-axis values (index of data)
                    y = data,  // y-axis values (actual data)
                    mode = "lines",  // Line plot mode
                    name = $"Training Sequence {i + 1}",  // Legend name
                    line = new Line { color = "blue" }  // Blue color for actual data
                };

                // Plot predicted (learned) sequence
                var learnedGraph = new Scatter
                {
                    x = Enumerable.Range(0, learnedData.Length).ToArray(),  // x-axis values (index of learned data)
                    y = learnedData,  // y-axis values (learned data)
                    mode = "lines",  // Line plot mode
                    name = $"Predicting Sequence {i + 1}",  // Legend name
                    line = new Line { color = "green", dash = "dashdot" }  // Green dashed line for predicted data
                };

                // Add both graphs (actual and learned) to the list
                allGraphs.Add(actualGraph);
                allGraphs.Add(learnedGraph);
            }

            // Create and configure the plot with the gathered data
            var chart = Chart.Plot(allGraphs);
            chart.WithTitle("Comparison of Actual and Testing Sequences");  // Set plot title
            chart.WithXTitle("X-axis (Index in Sequence)");  // Set X-axis label
            chart.WithYTitle("Y-axis (Value of Sequence)");  // Set Y-axis label

            // Define output directory and file path for saving the plot
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string outputDirectory = Path.Combine(projectRoot!, "result", "plots");
            Directory.CreateDirectory(outputDirectory);  // Ensure output directory exists

            // Generate file path for saving the chart as HTML
            string filePath = Path.Combine(outputDirectory, $"Actual_Testing_Sequence_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            // Save the chart as an HTML file
            File.WriteAllText(filePath, chart.GetHtml());

            // Display the chart in a browser
            chart.Show();
        }

        /// <summary>
        /// Compares actual (testing) sequences, learned (predicted) sequences, and detected anomalies.
        /// The anomalies are marked separately in red, making it easier to visualize abnormal points.
        /// The method generates an interactive HTML file and also displays the plot in a browser.
        /// </summary>
        /// <param name="allLearnedData">List of predicted sequences.</param>
        /// <param name="allTestingData">List of actual testing sequences.</param>
        /// <param name="allAnomalyIndices">List of indices marking detected anomalies for each sequence.</param>
        public static void CompareBothSequenceWithAnomalies(List<double[]> allLearnedData, List<double[]> allTestingData, List<List<int>> allAnomalyIndices)
        {
            // List to hold graphs for actual and learned sequences
            List<Scatter> allGraphs = new List<Scatter>();
            // List to hold graphs for anomalies
            List<Scatter> allAnomalies = new List<Scatter>();

            // Loop through each testing data sequence
            for (int i = 0; i < allTestingData.Count; i++)
            {
                // Get the current testing data sequence
                double[] data = allTestingData[i];
                // Get the corresponding learned (predicted) data sequence
                double[] learnedData = allLearnedData[i];
                // Get the list of anomaly indices for the current sequence
                List<int> anomalyIndices = allAnomalyIndices[i];

                // Plot actual data sequence (testing data)
                var actualGraph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(),  // x-axis values (index of data)
                    y = data,  // y-axis values (actual data)
                    mode = "lines",  // Line plot mode
                    name = $"Testing Sequence {i + 1}",  // Legend name
                    line = new Line { color = "green" }  // Green color for actual data
                };

                // Plot predicted (learned) sequence
                var learnedGraph = new Scatter
                {
                    x = Enumerable.Range(0, learnedData.Length).ToArray(),  // x-axis values (index of learned data)
                    y = learnedData,  // y-axis values (learned data)
                    mode = "lines",  // Line plot mode
                    name = $"Learned Sequence {i + 1}",  // Legend name
                    line = new Line { color = "orange", dash = "dashdot" }  // Orange dashed line for predicted data
                };

                // Add both graphs (actual and learned) to the list
                allGraphs.Add(actualGraph);
                allGraphs.Add(learnedGraph);

                // Plot anomalies as red markers
                var anomalyGraph = new Scatter
                {
                    x = anomalyIndices.Select(idx => (double)idx).ToArray(),  // x-axis values (index of anomalies)
                    y = anomalyIndices.Select(idx => data[idx]).ToArray(),  // y-axis values (anomalous data points)
                    mode = "markers",  // Markers plot mode
                    name = $"Anomalies in Sequence {i + 1}",  // Legend name
                    marker = new Marker { color = "crimson", size = 8 }  // Red color and size for anomaly markers
                };

                // Add the anomaly graph to the list
                allAnomalies.Add(anomalyGraph);
            }

            // Create and configure the plot with the gathered data
            var chart = Chart.Plot(allGraphs.Concat(allAnomalies));  // Combine the graphs for actual, learned data, and anomalies
            chart.WithTitle("Plot for Learned Sequences and Testing Sequences with Anomalies");  // Set plot title
            chart.WithXTitle("X-axis (Index in Sequence)");  // Set X-axis label
            chart.WithYTitle("Y-axis (Value of Sequence)");  // Set Y-axis label

            // Define output directory and file path for saving the plot
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string outputDirectory = Path.Combine(projectRoot!, "result", "plots");
            Directory.CreateDirectory(outputDirectory);  // Ensure output directory exists

            // Generate file path for saving the chart as HTML
            string filePath = Path.Combine(outputDirectory, $"Actual_Testing_Sequence_anomaly_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            // Save the chart as an HTML file
            File.WriteAllText(filePath, chart.GetHtml());

            // Display the chart in a browser
            chart.Show();
        }
    }
}
