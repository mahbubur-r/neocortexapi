# ML23/24-08: Implement Anomaly Detection Sample


# Introduction:

HTM (Hierarchical Temporal Memory) is a machine learning algorithm that processes time-series data in a distributed manner using a hierarchical network of nodes. Each nodes, or columns, can be trained to learn, and recognize patterns in input data. This can be used in identifying anomalies/deviations from normal patterns. It is a promising method for predicting and detecting anomalies in a range of applications. In this project, we will train our HTM Engine using the multisequencelearning class in the NeoCortex API, and then use the trained engine to learn patterns and identify anomalies. Specifically, numerical sequences will be read from various CSV files inside a folder in order to create an anomaly detection system.  

# Requirements

To run this project, we need.
* [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* Nuget package: [NeoCortexApi Version= 1.1.4](https://www.nuget.org/packages/NeoCortexApi/)

For code debugging, we recommend using visual studio IDE/visual studio code. This project can be run on [github codespaces](https://github.com/features/codespaces) as well.

# Usage

To run this project, 

* Install .NET SDK. Then using code editor/IDE of your choice, create a new console project and place all the C# codes inside your project folder. 
* Add/reference nuget package NeoCortexApi v1.1.4 to this project.
* Place numerical sequence CSV Files (datasets) under relevant folders respectively. All the folders should be inside the project folder. More details given below.

Our project is based on NeoCortex API. More details [here](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/gettingStarted.md).

# Details

To train our HTM Engine, we used the [MultiSequenceLearning](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) class in the NeoCortex API. Firstly, we will read and train the HTM Engine using the data from both our training (learning) and predicting (predictive) folders, which are present as numerical sequences in CSV files in the 'training' and 'predicting' folders inside the project directory. We will read numerical sequence data from the prediction folder for testing purposes, remove the first few elements (thus effectively turning the data into a subsequence of the original sequence; we have already inserted anomalies at random indexes into this data), and then use it to detect anomalies.

Please take note that all files inside the folders are read with the.csv extension, and exception handlers are set up in case the file format is incorrect.

We are employing artificial integer sequence data of network load for this project, which is saved inside of CSV files and is rounded off to the nearest integer, in percentage. Example of a csv file within training folder.

```
69,72,75,68,72,67,66,70,72,67
69,72,75,68,72,67,66,70,69,67
.............................
.............................
68,74,75,68,72,67,66,70,69,65
71,74,75,68,72,67,66,70,69,65
```
Normally, the values stay within the range of 65 to 75. All values outside of this range are considered anomalies for testing purposes. However, in order to identify anomalies, we have a csv file in the predicting folder. Typically, some of the data in this file does not fall within 65 and 75. 

```
71,74,98,68,92,65,66,70,69,65
71,74,75,68,72,65,66,30,69,35
.............................
.............................
71,74,75,71,72,65,36,70,69,65
71,75,75,71,72,65,66,70,98,95
```
We have uploaded the anomaly results of our data in this repository for reference.

1. output result of combined numerical sequence data from training folder (without anomalies) and predicting folder (with anomalies) can be found [here](https://github.com/mahbubur-r/neocortexapi/tree/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/output).

### Encoding:

Encoding of our input data is very important, such that it can be processed by our HTM Engine. More on [this](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/Encoders.md). 

As we are going to train and test data between the range of integer values between 0-100 with no periodicity, we are using the following settings. Minimum and maximum values are set to 0 and 100 respectively, as we are expecting all the values to be in this range only. In other used cases, these values need to be changed.

```csharp

int inputBits = 121;
int numColumns = 1210;
.......................
.......................
double max = 100;

Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 21},
                ...........
                { "MinVal", 0.0},
                ...........
                { "MaxVal", max}
            };
 ```
 
 Complete settings:
 
 ```csharp

Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 21},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "integer"},
                { "ClipInput", false},
                { "MaxVal", max}
            };
```

### HTM Configuration:

We have used the following configuration. More on [this](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/SpatialPooler.md#parameter-desription)

```csharp
{
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1
};
```

### Multisequence learning

The [RunExperiment](https://github.com/SouravPaulSumit/Team_anomaly/blob/be27813af65f611df7cbd33009d72a3ee72e3756/mySEProject/AnomalyDetectionSample/multisequencelearning.cs#L75) method inside the [multisequencelearning](https://github.com/SouravPaulSumit/Team_anomaly/blob/master/mySEProject/AnomalyDetectionSample/multisequencelearning.cs) class file demonstrates how multisequence learning works. To summarize, 

* HTM Configuration is taken and memory of connections are initialized. After that, HTM Classifier, Cortex layer and HomeostaticPlasticityController are initialized.
```csharp
.......
var mem = new Connections(cfg);
.......
HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, numUniqueInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) => ..
.......
.......
```

* After that, Spatial Pooler and Temporal Memory is initialized.
```csharp
.....
TemporalMemory tm = new TemporalMemory();
SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
.....
```
* After that, spatial pooler memory is added to cortex layer and trained for maximum number of cycles.
```csharp
.....
layer1.HtmModules.Add("sp", sp);
int maxCycles = 3500;
for (int i = 0; i < maxCycles && isInStableState == false; i++)
.....
`````
* After that, temporal memory is added to cortex layer to learn all the input sequences.
```csharp
.....
layer1.HtmModules.Add("tm", tm);
foreach (var sequenceKeyPair in sequences){
.....
}
.....
```
* Finally, the trained cortex layer and HTM classifier is returned.
```csharp
.....
return new Predictor(layer1, mem, cls)
.....
`````
We will use this for prediction in later parts of our project.

## Execution of the project

Our project is executed in the following way. 

* In the beginning, we have ReadFolder method of [CSVFolderReader](https://github.com/SouravPaulSumit/Team_anomaly/blob/master/mySEProject/AnomalyDetectionSample/CSVFolderReader.cs) class to read all the files placed inside a folder. Alternatively, we can use ReadFile method of [CSVFileReader](https://github.com/SouravPaulSumit/Team_anomaly/blob/master/mySEProject/AnomalyDetectionSample/CSVFileReader.cs) to read a single file; it works in a similar way, except that it reads a single file. These classes store the read sequences to a list of numeric sequences, which will be used in a number of occasions later. These classes have exception handling implemented inside for handling non-numeric data. Data can be trimmed using Trimsequences method. It trims one to four elements(Number 1 to 4 is decided randomly) from the beginning of a numeric sequence and returns it.

```csharp
 public List<List<double>> ReadFolder()
        {
         ....  
          return folderSequences;
        }

public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
        ....
          return trimmedSequences;
        }
```

* After that, the method BuildHTMInput of [CSVToHTMInput](https://github.com/SouravPaulSumit/Team_anomaly/blob/master/mySEProject/AnomalyDetectionSample/CSVToHTMInput.cs) class is there which converts all the read sequences to a format suitable for HTM training.
```csharp
Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
for (int i = 0; i < sequences.Count; i++)
    {
     // Unique key created and added to dictionary for HTM Input                
     string key = "S" + (i + 1);
     List<double> value = sequences[i];
     dictionary.Add(key, value);
    }
     return dictionary;
```
* After that, we have RunHTMModelLearning method of [HTMModeltraining](https://github.com/SouravPaulSumit/Team_anomaly/blob/master/mySEProject/AnomalyDetectionSample/HTMModeltraining.cs) class to train our model using the converted sequences. The numerical data sequences from training (for learning) and predicting folders are combined before training the HTM engine. This class returns our trained model object predictor.
```csharp
.....
MultiSequenceLearning learning = new MultiSequenceLearning();
predictor = learning.Run(htmInput);
.....
.....
List<List<double>> combinedSequences = new List<List<double>>(sequences1);
combinedSequences.AddRange(sequences2);
.....
```
* In the end, we use [HTMAnomalyTesting]to detected anomalies in sequences read from files inside predicting folder. All the classes explained earlier- CSV files reading (CSVFileReader), combining and converting them for HTM training (CSVToHTMInput) and training the HTM engine (using HTMModelTraining) will be used here. We use the same class (CSVFolderReader) to read files for our predicting sequences. TrimSequences method is then used to trim sequences for anomaly testing. Method for trimming is already explained earlier.
```csharp
.....
CSVFolderReader testseq = new CSVFolderReader(_predictingFolderPath);
var inputtestseq = testseq.ReadFolder();
var triminputtestseq = CSVFolderReader.TrimSequences(inputtestseq);
.....
```
Path to training and predicting folder is set as default and passed on the constructor, or can be set inside the class manually.

```csharp
.....
 _trainingFolderPath = Path.Combine(projectbaseDirectory, trainingFolderPath);
_predictingFolderPath = Path.Combine(projectbaseDirectory, predictingFolderPath);
.....
```
In the end, DetectAnomaly method is used to detect anomalies in our trimmed sequences one by one, using our trained HTM Model predictor. 
```csharp
foreach (List<double> list in triminputtestseq)
       {
         .....
         double[] lst = list.ToArray();
         DetectAnomaly(myPredictor, lst);
       }
```
Exception handling is present, such that errors thrown from DetectAnomaly method can be handled (like passing of non-numeric values, or number of elements in list less than two).

DetectAnomaly is the main method which detects anomalies in our data. It traverses each value of a list one by one in a sliding window manner, and uses trained model predictor to predict the next element for comparison. We use an anomalyscore to quantify the comparison and detect anomalies; if the prediction crosses a certain tolerance level, it is declared as an anomaly.

In our sliding window approach, naturally the first element is skipped, so we ensure that the first element is checked for anomaly in the beginning.

We can get our prediction in a list of results in format of "NeoCortexApi.Classifiers.ClassifierResult`1[System.String]" from our trained model Predictor using the following:

```csharp
var res = predictor.Predict(item);
```
Here, assume that item passed to the model is of int type with value 8. We can use this to analyze how prediction works. When this is executed,
```csharp
foreach (var pred in res)
 {
   Console.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
    }
```
We get the following output.
```
S2_2-9-10-7-11-8-1 - 100
S1_1-2-3-4-2-5-0 - 5
S1_-1.0-0-1-2-3-4 - 0
S1_-1.0-0-1-2-3-4-2 - 0
```
We know that the item we passed here is 8. The first line gives us the best prediction with similarity accuracy. We can easily get the predicted value which will come after 8 (here, it is 1), and previous value (11, in this case). We use basic string operations to get our required values.

We will then use this to detect anomalies.

* When we iteratively pass values to DetectAnomaly method using our sliding window approach, we will not be able to detect anomaly in the first element. So, in the beginning, we use the second element of the list to predict and compare the previous element (which is the first element). A flag is set to control the command execution; if the first element has anomaly, then we will not use it to detect our second element. We will directly start from second element. Otherwise, we will start from first element as usual.

* Now, when we traverse the list one by one to the right, we pass the value to the predictor to get the next value and compare the prediction with the actual value. If there's anomaly, then it is outputted to the user, and the anomalous element is skipped. Upon reaching to the last element, we can end our traversal and move on to next list.

We use anomalyscore (difference ratio) for comparison with our already preset threshold. When it exceeds, probable anomalies are found.

To run this project, use the following class/methods given in [Program.cs].

```csharp
 HTMAnomalyTesting tester = new HTMAnomalyTesting();
 tester.Run();
```
 
# Results

After running this project, we got the following [output]

We can observe that the false negative rate is high in our output (0.65). It is desired that false negative rate should be as lower as possible in an anomaly detection program. Lower false positive rate is also desirable, but not absolutely essential.

Although, it depends on a number of factors, like quantity (the more, the better) and quality of data, and hyperparameters used to tune and train model; more data should be used for training, and hyperparameters should be further tuned to find the most optimal setting for training to get the best results. We were using less amount of numerical sequences as data to demonstrate our sample project due to time and computational constraints, but that can be improved if we use better resources, like cloud.
