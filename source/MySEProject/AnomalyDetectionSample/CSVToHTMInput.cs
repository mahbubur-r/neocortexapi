using System;

namespace AnomalyDetectionSample
{
    public class CSVToHTMInput
    {
        public Dictionary<string, List<double>> BuildHTMInput(List<List<double>> sequences)
        {
            Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
            for (int i = 0; i < sequences.Count; i++)
            {         
                string key = "S" + (i + 1);
                List<double> value = sequences[i];
                dictionary.Add(key, value);
            }
            return dictionary;
        }
    }
}
