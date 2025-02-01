using System;
using System.Collections.Generic;

namespace AnomalyDetectionSample
{

    public class CSVToHTMInputConverter
    {

        public Dictionary<string, List<double>> ConvertToHTMInput(List<List<double>> inputSequences)
        {
            Dictionary<string, List<double>> sequenceDictionary = new Dictionary<string, List<double>>();

            for (int index = 0; index < inputSequences.Count; index++)
            {
  
                sequenceDictionary.Add(sequenceKey, sequenceData);
            }

            return sequenceDictionary;
        }
    }
}
