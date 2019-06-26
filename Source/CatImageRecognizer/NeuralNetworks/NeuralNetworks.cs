using AForge.Neuro;
using AForge.Neuro.Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatImageRecognizer.NeuralNetworks
{
    public interface INeuralNetwork
    {
        void BatchTrain(double[][] batchInputs, double[][] batchOutputs, int iterations, Action<double, int, string> progressCallback);
        double[] GenerateOutput(double[] inputs);
        int GetInputLength();
        void SaveNetwork(string filePath);
        void LoadNetworkFromFile(string filePath);
        int GetBatchSubSize();
        string GetNetworkName();
        void StopBatchTraining();

        int GetMaxIterationsOnEachPage();
        double GetStopPageTrainingErrorRate();
        double GetStopIterationTrainingErrorRate();

        string GetModelExtension();
    }
}
