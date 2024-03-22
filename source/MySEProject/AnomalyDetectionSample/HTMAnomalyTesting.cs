using NeoCortexApi;
using System;
using System.Text;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// This class is responsible for testing an HTM model.
    /// CSV files from both training(learning) and predicting folders will be used for training our HTM Model.
    /// Default training(learning) and predicting folder paths are passed on to the constructor.
    /// Testing is carried out by trained model created using HTMModeltraining class,
    /// then CSVFolderReader is used to read all the sequences from all the CSV files inside predicting folder and trimmed,
    /// after that, the trimmed subsequences are tested sequence by sequence. 
    /// In the end, DetectAnomaly method will be used for
    /// testing all the elements in a sequence as sliding window, one by one.
    /// </summary>
    public class HTMAnomalyTesting
    {
        private readonly string _trainingFolderPath;
        private readonly string _predictingFolderPath;
        private static double totalAccuracy = 0.0;
        private static int iterationCount = 0;
        readonly double tolerance = 0.1;

        /// <summary>
        /// Default training(learning)/ predicting folder paths are passed on to the constructor.
        /// </summary>
        /// <param name="trainingFolderPath">The path to the folder containing the CSV files for training(learning).</param>
        ///<param name="predictingFolderPath">The path to the folder containing the CSV files for predicting.</param>
        public HTMAnomalyTesting(string trainingFolderPath = "training", string predictingFolderPath = "predicting")
        {
            // Folder directory set to location of C# files. This is the relative path.
            string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            _trainingFolderPath = Path.Combine(projectbaseDirectory, trainingFolderPath);
            _predictingFolderPath = Path.Combine(projectbaseDirectory, predictingFolderPath);
        }

        /// <summary>
        /// Runs the anomaly detection experiment with default tolerance level set to 0.1.
        /// The input parameters can be overridden.
        /// <param name="tolerance">The tolerance level for anomaly detection.</param>
        /// <param name="trainingFolderPath">The path to the folder containing the CSV files for training (learning).</param>
        /// <param name="predictingFolderPath">The path to the folder containing the CSV files for predicting.</param>
        public void Run()
        {
            // HTM model training initiated
            HTMTrainingManager myModel = new HTMTrainingManager();
            Predictor myPredictor;

            myModel.ExecuteHTMModelTraining(_trainingFolderPath, _predictingFolderPath, out myPredictor);

            Console.WriteLine();
            Console.WriteLine("Started testing our trained HTM Engine...................");
            Console.WriteLine();

            // Starting to test our trained HTM model
            // CSVFileReader can also be used in place of CSVFolderReader to read a single file
            // We will take sequences from predicting folder
            // After that, we will then trim those sequences: sequences where first few elements are removed, for anomaly detection
            CSVFolderReader testseq = new CSVFolderReader(_predictingFolderPath);
            var inputtestseq = testseq.ReadFolder();
            var triminputtestseq = CSVFolderReader.TrimSequences(inputtestseq);
            myPredictor.Reset();

            // Writing output of our experiment in csv file
            // It can later be used for visualization
            // Bottom few lines are used for setting the output path and output file name
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputFile = $"anomaly_output_{timestamp}.txt";

            // For Windows
            string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            string outputFolderPath = Path.Combine(projectbaseDirectory, "output");
            string outputFilePath = Path.Combine(outputFolderPath, outputFile);

            // For Linux and Github codespaces, add the path explicitly
            // string outputFolderPath = "";
            // string outputFilePath = Path.Combine(outputFolderPath, outputFile);


            // Output path set for cloud experiment
            //string outputFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            //string outputFilePath = Path.Combine(outputFolderPath, outputFile);
            //StoredOutputValues.OutputPath = outputFilePath;

            // Testing the sequences one by one
            // Our anomaly detection experiment is complete after all the lists are traversed iteratively.
            // If the list contains less than two values, or contain non-negative values, exception is thrown from DetectAnomaly method.
            // Errors are handled using exception handling without disrupting our program flow.
            // Used for writing our output to text file.
            // We will store the returned output of DetectAnomaly method to list of list of strings
            // Then we will use that for writing our experiment output to txt file

            List<List<string>> totalOutputStringList = new List<List<string>>();

            foreach (List<double> list in triminputtestseq)
            {
                double[] lst = list.ToArray();
                List<string> outputReturnedLines = new List<string>();

                try
                {
                    outputReturnedLines = DetectAnomaly(myPredictor, lst, tolerance);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Exception caught: {ex.Message}");
                }
                totalOutputStringList.Add(outputReturnedLines);
            }

            // Initialize a StringBuilder to efficiently build a multi-line string
            StringBuilder myStringBuilder = new StringBuilder();

            // Iterate over each inner list of strings
            foreach (List<string> innerLine in totalOutputStringList)
            {
                foreach (string line in innerLine)
                {
                    // Append the current line to the StringBuilder
                    myStringBuilder.AppendLine(line);
                }
            }

            // Write the content of the StringBuilder to output text file
            File.WriteAllText(outputFilePath, myStringBuilder.ToString());

            // Storing average accuracy of our HTM engine from cloud experiment.
            StoredOutputValues.totalAvgAccuracy = totalAccuracy / iterationCount;

            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Writing experiment results to text file successful!");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Anomaly detection experiment complete!!.");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
        }

        /// <summary>
        /// Detects anomalies in the input list using the HTM trained model.
        /// The anomaly score is calculated using a sliding window approach.
        /// The difference between the predicted value and the actual value is used to calculate the anomaly score.
        /// If the difference exceeds a certain tolerance set earlier, anomaly is detected.
        /// Returns the result in a list of strings
        /// </summary>
        /// <param name="predictor">Trained HTM model, used for prediction.</param>
        /// <param name="list">Input list which will be used to detect anomalies.</param>
        /// <param name="tolerance">Tolerance value ratio can be overloaded from outside. Default is 0.1</param>
        private static List<string> DetectAnomaly(Predictor predictor, double[] list, double tolerance = 0.1)
        {
            // Checking if the list contains at least two values
            if (list.Length < 2)
            {
                throw new ArgumentException($"List must contain at least two values. Actual count: {list.Length}. List: [{string.Join(",", list)}]");

            }
            // Checking if the list contains any non-numeric values
            foreach (double value in list)
            {
                if (double.IsNaN(value))
                {
                    throw new ArgumentException($"List contains non-numeric values. List: [{string.Join(",", list)}]");
                }
            }

            //Storing our output to list of strings
            List<string> resultOutputStringList = new List<string>
            {
                "------------------------------",
                "",
                "Testing the sequence for anomaly detection: " + string.Join(", ", list) + ".",
                ""
            };

            // Initializing variable for calculating accuracy of each tested sequence
            double currentAccuracy = 0.0;

            // Input list will be traversed one by one, like a sliding window
            for (int i = 0; i < list.Length; i++)
            {

                //Values for the rest of the list will be iteratively referred to, in the following variable.
                var item = list[i];

                // Using our trained HTM model predictor to predict next item.
                var res = predictor.Predict(item);

                resultOutputStringList.Add("Current element in the testing sequence from input list: " + item);

                if (res.Count > 0)
                {
                    // Extracting predicted item and accuracy from predictor output
                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    var tokens3 = res.First().Similarity;

                    // We exclude the last element of the list
                    // Because there is no element after that to detect anomaly in.
                    if (i < list.Length - 1)
                    {
                        int nextIndex = i + 1;
                        double nextItem = list[nextIndex];
                        double predictedNextItem = double.Parse(tokens2.Last());

                        // Anomalyscore variable will be used to check the deviation from predicted item
                        var AnomalyScore = Math.Abs(predictedNextItem - nextItem);
                        var deviation = AnomalyScore / nextItem;

                        if (deviation <= tolerance)
                        {

                            resultOutputStringList.Add("Anomaly not detected in the next element!! HTM Engine found similarity to be: " + tokens3 + "%.");
                            currentAccuracy += tokens3;

                        }
                        else
                        {
                            resultOutputStringList.Add($"****Anomaly detected**** in the next element. HTM Engine predicted it to be {predictedNextItem} with similarity: {tokens3}%, but the actual value is {nextItem}.");
                            i++; // skip to the next element for checking, as we cannot use anomalous element for prediction
                            resultOutputStringList.Add("As anomaly was detected, so we are skipping to the next element in our testing sequence.");
                            currentAccuracy += tokens3;
                        }

                    }

                    else
                    {
                        resultOutputStringList.Add("End of input list. Further anomaly testing cannot be continued.");
                    }
                }
                else
                {
                    resultOutputStringList.Add("Nothing predicted from HTM Engine. Anomaly cannot be detected.");
                }

            }

            //Calculating HTM engine's average accuracy per sequence
            var avgSeqAccuracy = currentAccuracy / list.Length;

            resultOutputStringList.Add("");
            resultOutputStringList.Add("Average accuracy for this sequence: " + avgSeqAccuracy + "%.");
            resultOutputStringList.Add("");
            resultOutputStringList.Add("------------------------------");

            // Incrementing our HTM engine's total average accuracy outside of this method after each loop
            totalAccuracy += avgSeqAccuracy;

            //Count the number of iterations
            iterationCount++;

            ShowOutputOnConsole(predictor, list, tolerance);

            return resultOutputStringList;

        }

        private static void ShowOutputOnConsole(Predictor predictor, double[] list, double tolerance)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Testing the sequence for anomaly detection: " + string.Join(", ", list) + ".");

            // In the beginning, we are going to check whether's anomaly in the first element of the list.
            // For that, we are going to input second item from the list and predict previous item using our trained HTM model.
            // After that, we will compare the first item of the list with the predicted previous item of the second item, i.e predicted first item. 


            // Boolean flag is used to check whether we can start checking from first element or not.
            // We will not start checking from first element if there is anomaly in the first element.
            bool startFromFirst = true;

            // These are the first and second items from the input list.
            // These values are neccesary to detect anomaly in the first element of the list.
            double firstItem = list[0];
            double secondItem = list[1];

            // Checking the first element of the list for anomaly
            // Using our trained HTM model predictor to predict the first item.
            var secondItemRes = predictor.Predict(secondItem);

            Console.WriteLine("First element in the testing sequence from input list: " + firstItem);

            if (secondItemRes.Count > 0)
            {
                // Extracting predicted item and accuracy from predictor output
                // Refer to documentation to know about how this works.
                var stokens = secondItemRes.First().PredictedInput.Split('_');
                var stokens2 = secondItemRes.First().PredictedInput.Split('-');
                var stokens3 = secondItemRes.First().Similarity;
                var stokens4 = stokens2.Reverse().ElementAt(2);
                double predictedFirstItem = double.Parse(stokens4);
                // firstanomalyScore variable will be used to check the deviation for first element in the list only
                var firstanomalyScore = Math.Abs(predictedFirstItem - firstItem);
                var fdeviation = firstanomalyScore / firstItem;

                if (fdeviation <= tolerance)
                {

                    Console.WriteLine($"No anomaly detected in the first element. HTM Engine found similarity to be:{stokens3}%. Starting check from beginning of the list.");
                    startFromFirst = true;

                }
                else
                {

                    Console.WriteLine($"****Anomaly detected**** in the first element. HTM Engine predicted it to be {predictedFirstItem} with similarity: {stokens3}%, but the actual value is {firstItem}. Moving to the next element.");
                    startFromFirst = false;

                }
            }
            else
            {

                Console.WriteLine("Anomaly detection cannot be performed for the first element. Starting check from beginning of the list.");
                startFromFirst = true;

            }

            // Ending check for anomaly in the first element
            // The above few lines of code is only used to detect whether anomaly is present in the first element only
            // which is missed when we use sliding window approach.
            // We use a checking condition to set the starting point depending on
            // whether there's anomaly in first element or not.
            // Ternary operator checks the flag and sets the starting point accordingly
            // If "startFromFirst" flag is true, checkCondition is set to 0, otherwise it is set to 1. 

            int checkCondition = startFromFirst ? 0 : 1;

            // Starting element depends on whether there is anomaly in first element or not
            // Input list will be traversed one by one, like a sliding window
            for (int i = checkCondition; i < list.Length; i++)
            {
                //Values for the rest of the list will be iteratively referred to, in the following variable.
                var item = list[i];

                // Using our trained HTM model predictor to predict next item.
                var res = predictor.Predict(item);
                Console.WriteLine("Current element in the testing sequence from input list: " + item);

                if (res.Count > 0)
                {
                    // Extracting predicted item and accuracy from predictor output
                    // Refer to documentation to know about how this works.
                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    var tokens3 = res.First().Similarity;

                    // We exclude the last element of the list
                    // Because there is no element after that to detect anomaly in.
                    if (i < list.Length - 1)
                    {
                        int nextIndex = i + 1;
                        double nextItem = list[nextIndex];
                        double predictedNextItem = double.Parse(tokens2.Last());
                        // Anomalyscore variable will be used to check the deviation from predicted item
                        var AnomalyScore = Math.Abs(predictedNextItem - nextItem);
                        var deviation = AnomalyScore / nextItem;

                        if (deviation <= tolerance)
                        {
                            Console.WriteLine("Anomaly not detected in the next element!! HTM Engine found similarity to be: " + tokens3 + "%.");
                        }
                        else
                        {
                            Console.WriteLine($"****Anomaly detected**** in the next element. HTM Engine predicted it to be {predictedNextItem} with similarity: {tokens3}%, but the actual value is {nextItem}.");
                            i++; // skip to the next element for checking, as we cannot use anomalous element for prediction
                            Console.WriteLine("As anomaly was detected, so we are skipping to the next element in our testing sequence.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("End of input list. Further anomaly testing cannot be continued.");
                        Console.WriteLine();
                        Console.WriteLine("------------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("Nothing predicted from HTM Engine. Anomaly cannot be detected.");
                }
            }
        }
    }

}
