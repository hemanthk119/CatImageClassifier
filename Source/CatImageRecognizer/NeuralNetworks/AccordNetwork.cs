using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatImageRecognizer.NeuralNetworks
{
    public static class Randomizer
    {
        private static Random rng = new Random();
        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle(double[][] values1, double[][] values2)
        {
            int n = values1.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                double[] value1 = values1[k];
                values1[k] = values1[n];
                values1[n] = value1;
                double[] value2 = values2[k];
                values2[k] = values2[n];
                values2[n] = value2;
            }
        }
    }

    public class AccordNetwork : INeuralNetwork
    {
        private int inputLength = 2600;
        DeepBeliefNetwork network;
        DeepBeliefNetworkLearning unsuperVisedTeacher;
        ResilientBackpropagationLearning supervisedTeacher;

        private int batchSubSize = 500;
        private int maxIterationsOnEachPage = 1000;
        private int maxUnSupervisedIterationsOnEachPage = 100;
        private double stopPageTrainingErrorRate = 0.0001;
        private double stopIterationTrainingErrorRate = 0.001;

        public AccordNetwork()
        {
            network = new DeepBeliefNetwork(new BernoulliFunction(), inputLength, 1200, 600, 2);
            new NguyenWidrow(network).Randomize();
            network.UpdateVisibleWeights();
            unsuperVisedTeacher = GetUnsupervisedTeacherForNetwork(network);
            supervisedTeacher = GetSupervisedTeacherForNetwork(network);
        }

        public void BatchTrain(double[][] batchInputs, double[][] batchOutputs, int iterations, Action<double, int, string> progressCallback)
        {
            Randomizer.Shuffle(batchInputs, batchOutputs);
            TrainUnsupervised(batchInputs, maxUnSupervisedIterationsOnEachPage, progressCallback);
            TrainSupervised(batchInputs, batchOutputs, iterations, progressCallback);
        }

        private void TrainUnsupervised(double[][] batchInputs, int iterations, Action<double, int, string> progressCallback)
        {
            for (int layerIndex = 0; layerIndex < network.Machines.Count - 1; layerIndex++)
            {
                unsuperVisedTeacher.LayerIndex = layerIndex;
                var layerData = unsuperVisedTeacher.GetLayerInput(batchInputs);
                foreach (int i in Enumerable.Range(1, iterations))
                {
                    var error = unsuperVisedTeacher.RunEpoch(layerData) / batchInputs.Length;
                    if (progressCallback != null)
                    {
                        progressCallback(error, i, $"Unsupervised Layer {layerIndex}");
                    }
                    if (this.ShouldStopTraning)
                    {
                        this.ShouldStopTraning = false;
                        break;
                    }
                }
            }
        }

        private void TrainSupervised(double[][] batchInputs, double[][] batchOutputs, int iterations, Action<double, int, string> progressCallback)
        {
            foreach (int i in Enumerable.Range(1, iterations))
            {
                var error = supervisedTeacher.RunEpoch(batchInputs, batchOutputs) / batchInputs.Length;
                if (progressCallback != null)
                {
                    progressCallback(error, i, "Supervised");
                }
                if (this.ShouldStopTraning)
                {
                    this.ShouldStopTraning = false;
                    break;
                }
            }
        }

        private DeepBeliefNetworkLearning GetUnsupervisedTeacherForNetwork(DeepBeliefNetwork deepNetwork)
        {
            var teacher = new DeepBeliefNetworkLearning(deepNetwork)
            {
                Algorithm = (hiddenLayer, visibleLayer, i) => new ContrastiveDivergenceLearning(hiddenLayer, visibleLayer)
                {
                    LearningRate = 0.1,
                    Momentum = 0.5
                }
            };
            return teacher;
        }

        private ResilientBackpropagationLearning GetSupervisedTeacherForNetwork(DeepBeliefNetwork deepNetwork)
        {
            var teacher = new ResilientBackpropagationLearning(deepNetwork)
            {
                LearningRate = 0.1
                //Momentum = 0.5
            };
            return teacher;
        }


        public double[] GenerateOutput(double[] inputs)
        {
            return network.Compute(inputs);
        }

        public int GetBatchSubSize()
        {
            return batchSubSize;
        }

        public int GetInputLength()
        {
            return inputLength;
        }

        public string GetNetworkName()
        {
            return "Accord.NET";
        }

        public void LoadNetworkFromFile(string filePath)
        {
            network = DeepBeliefNetwork.Load(filePath);
            supervisedTeacher = GetSupervisedTeacherForNetwork(network);
            unsuperVisedTeacher = GetUnsupervisedTeacherForNetwork(network);
        }

        public void SaveNetwork(string filePath)
        {
            network.Save(filePath);
        }

        public bool ShouldStopTraning { get; set; } = false;

        public void StopBatchTraining()
        {
            this.ShouldStopTraning = true;
        }

        public int GetMaxIterationsOnEachPage()
        {
            return maxIterationsOnEachPage;
        }

        public double GetStopPageTrainingErrorRate()
        {
            return stopPageTrainingErrorRate;
        }

        public double GetStopIterationTrainingErrorRate()
        {
            return stopIterationTrainingErrorRate;
        }

        public string GetModelExtension()
        {
            return "accmodel";
        }
    }
}
