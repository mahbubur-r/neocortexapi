using NeoCortexApi;
using System;

namespace AnomalyDetectionSample
{
    public class HTMAnomalyTesting
    {
        private readonly string _trainingFolderPath;
        private readonly string _predictingFolderPath;
        /// <summary>
        /// Runs the anomaly detection experiment.
        /// </summary>
        public HTMAnomalyTesting(string trainingFolderPath = "training", string predictingFolderPath = "predicting")
        {
            // Folder directory set to location of C# files. This is the relative path.
            string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            _trainingFolderPath = Path.Combine(projectbaseDirectory, trainingFolderPath);
            _predictingFolderPath = Path.Combine(projectbaseDirectory, predictingFolderPath);

        }
        public void Run()
        {
            // HTM model training initiated
            HTMModeltraining myModel = new HTMModeltraining();
            Predictor myPredictor;


            myModel.RunHTMModelLearning(_trainingFolderPath, _predictingFolderPath, out myPredictor);

            Console.WriteLine("------------------------------");
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


            foreach (List<double> list in triminputtestseq)
            {
                double[] lst = list.ToArray();
                try
                {
                    DetectAnomaly(myPredictor, lst);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Exception caught: {ex.Message}");
                }
            }
        }

        Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Anomaly detection experiment complete!!.");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
}
    private static void DetectAnomaly(Predictor predictor, double[] list)
    {
        if (list.Length < 2)
        {
            throw new ArgumentException($"List must contain at least two values. Actual count: {list.Length}. List: [{string.Join(",", list)}]");

        }
        foreach (double value in list)
        {
            if (double.IsNaN(value))
            {
                throw new ArgumentException($"List contains non-numeric values. List: [{string.Join(",", list)}]");
            }
        }
        Console.WriteLine("------------------------------");
        Console.WriteLine();
        Console.WriteLine("Testing the sequence for anomaly detection: " + string.Join(", ", list) + ".");


        double tolerance = 0.1;

        bool startFromFirst = true;

        double firstItem = list[0];
        double secondItem = list[1];


        var secondItemRes = predictor.Predict(secondItem);

        Console.WriteLine("First element in the testing sequence from input list: " + firstItem);
        if (secondItemRes.Count > 0)
        {
            var stokens = secondItemRes.First().PredictedInput.Split('_');
            var stokens2 = secondItemRes.First().PredictedInput.Split('-');
            var stokens3 = secondItemRes.First().Similarity;
            var stokens4 = stokens2.Reverse().ElementAt(2);
            double predictedFirstItem = double.Parse(stokens4);
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
        int checkCondition = startFromFirst ? 0 : 1;
        for (int i = checkCondition; i < list.Length; i++)
        {
            var item = list[i];
            var res = predictor.Predict(item);
            Console.WriteLine("Current element in the testing sequence from input list: " + item);
            if (res.Count > 0)
            {
                var tokens = res.First().PredictedInput.Split('_');
                var tokens2 = res.First().PredictedInput.Split('-');
                var tokens3 = res.First().Similarity;
                if (i < list.Length - 1)
                {
                    int nextIndex = i + 1;
                    double nextItem = list[nextIndex];
                    double predictedNextItem = double.Parse(tokens2.Last());
                    var AnomalyScore = Math.Abs(predictedNextItem - nextItem);
                    var deviation = AnomalyScore / nextItem;

                    if (deviation <= tolerance)
                    {
                        Console.WriteLine("Anomaly not detected in the next element!! HTM Engine found similarity to be: " + tokens3 + "%.");
                    }
                    else
                    {
                        Console.WriteLine($"****Anomaly detected**** in the next element. HTM Engine predicted it to be {predictedNextItem} with similarity: {tokens3}%, but the actual value is {nextItem}.");
                        i++;
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
