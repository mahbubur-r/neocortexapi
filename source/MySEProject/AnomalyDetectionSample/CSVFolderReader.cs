using System;

namespace AnomalyDetectionSample
{
    public class CSVFolderReader
    {
        private string _folderPathToCSV;
        public CSVFolderReader(string folderPathToCSV)
        {
            _folderPathToCSV = folderPathToCSV;
        }
        public List<List<double>> ReadFolder()
        {
            List<List<double>> folderSequences = new List<List<double>>();
            string[] fileEntries = Directory.GetFiles(_folderPathToCSV, "*.csv");

            foreach (string fileName in fileEntries)
            {
                string[] csvLines = File.ReadAllLines(fileName);
                List<List<double>> sequencesInFile = new List<List<double>>();

                for (int i = 0; i < csvLines.Length; i++)
                {
                    string[] columns = csvLines[i].Split(new char[] { ',' });
                    List<double> sequence = new List<double>();

                    for (int j = 0; j < columns.Length; j++)
                    {
                        if (double.TryParse(columns[j], out double value))
                        {
                            sequence.Add(value);
                        }
                        else
                        {
                            throw new ArgumentException($"Non-numeric value found! Please check file: {fileName}.");
                        }
                    }
                    sequencesInFile.Add(sequence);
                }
                folderSequences.AddRange(sequencesInFile);
            }
            return folderSequences;
        }
        public void CSVSequencesConsoleOutput()
        {
            List<List<double>> sequences = ReadFolder();
            for (int i = 0; i < sequences.Count; i++)
            {
                Console.Write("Sequence " + (i + 1) + ": ");
                foreach (double number in sequences[i])
                {
                    Console.Write(number + " ");
                }
                Console.WriteLine("");
            }
        }

        public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
            Random rnd = new Random();
            List<List<double>> trimmedSequences = new List<List<double>>();

            foreach (List<double> sequence in sequences)
            {
                int numElementsToRemove = rnd.Next(1, 5);
                List<double> trimmedSequence = sequence.Skip(numElementsToRemove).ToList();
                trimmedSequences.Add(trimmedSequence);
            }

            return trimmedSequences;
        }

    }
}
