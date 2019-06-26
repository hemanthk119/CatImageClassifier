using ConvNetSharp.Core;
using ConvNetSharp.Core.Layers;
using ConvNetSharp.Core.Layers.Double;
using ConvNetSharp.Core.Training;
using ConvNetSharp.Volume;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvNetSharp.Core.Serialization;
using CatImageRecognizer.Models;
using System.IO;

namespace CatImageRecognizer.NeuralNetworks
{
    public class ConvNetSharpNetwork : INeuralNetwork
    {
        private Net<double> network;
        TrainerBase<double> trainer;

        private int batchSubSize = 50;
        private int maxIterationsOnEachPage = 500;
        private double stopPageTrainingErrorRate = 0.02;
        private double stopIterationTrainingErrorRate = 1;
        public ConvNetSharpNetwork()
        {
            network = new Net<double>();

            network.AddLayer(new InputLayer(50, 52, 1));
            network.AddLayer(new ConvLayer(3, 3, 8) { Stride = 1, Pad = 2 });
            network.AddLayer(new ReluLayer());
            network.AddLayer(new PoolLayer(3, 3) { Stride = 2 });
            network.AddLayer(new ConvLayer(3, 3, 16) { Stride = 1, Pad = 2 });
            network.AddLayer(new ReluLayer());
            network.AddLayer(new PoolLayer(3, 3) { Stride = 2 });
            network.AddLayer(new ConvLayer(3, 3, 32) { Stride = 1, Pad = 2 });
            network.AddLayer(new FullyConnLayer(20));
            network.AddLayer(new FullyConnLayer(50));
            network.AddLayer(new FullyConnLayer(2));
            network.AddLayer(new SoftmaxLayer(2));

            trainer = GetTrainerForNetwork(network);
        }

        private TrainerBase<double> GetTrainerForNetwork(Net<double> net)
        {
            return new SgdTrainer<double>(net)
            {
                LearningRate = 0.003
            };
        }

        private double[] Layout2DArray(double[][] inputs, int totalLength)
        {
            var dataLayoutOutInputs = new double[totalLength];
            foreach (int d in Enumerable.Range(0, inputs.Length))
            {
                foreach (int i in Enumerable.Range(0, inputs[d].Length))
                {
                    dataLayoutOutInputs[(inputs[d].Length * d) + i] = inputs[d][i];
                }
            }
            return dataLayoutOutInputs;
        }

        public bool ShouldStopTraning { get; set; } = false;

        public void StopBatchTraining()
        {
            this.ShouldStopTraning = true;
        }

        private (Volume<double>, Volume<double>) GetVolumeDataSetsFromArrays(double[][] batchInputs, double[][] batchOutputs)
        {
            var dataLayedoutInputs = Layout2DArray(batchInputs, batchInputs.Length * 50 * 52);
            var dataLayedoutOutputs = Layout2DArray(batchOutputs, batchOutputs.Length * 2);

            trainer.BatchSize = batchInputs.Length;
            Volume<double> inputs = BuilderInstance<double>.Volume.From(dataLayedoutInputs, new Shape(50, 52, 1, batchInputs.Length));
            Volume<double> outputs = BuilderInstance<double>.Volume.From(dataLayedoutOutputs, new Shape(1, 1, 2, batchOutputs.Length));

            return (inputs, outputs);
        }

        public void BatchTrain(double[][] batchInputs, double[][] batchOutputs, int iterations, Action<double, int, string> progressCallback)
        {
            trainer.BatchSize = batchInputs.Length;
            foreach (int currentIteration in Enumerable.Range(1, iterations))
            {
                Randomizer.Shuffle(batchInputs, batchOutputs);
                (Volume<double> inputs, Volume<double> outputs) = GetVolumeDataSetsFromArrays(batchInputs, batchOutputs);
                trainer.Train(inputs, outputs);
                var error = network.GetCostLoss(inputs, outputs);
                if (progressCallback != null)
                {
                    progressCallback(error, currentIteration, "Supervised");
                }
                inputs.Dispose();
                outputs.Dispose();
                if(this.ShouldStopTraning)
                {
                    this.ShouldStopTraning = false;
                    break;
                }
            }
        }

        public double[] GenerateOutput(double[] inputs)
        {
            Volume<double> inputImageData = BuilderInstance<double>.Volume.From(inputs, new Shape(50, 52, 1));            
            Volume<double> calculatedPrediction = network.Forward(inputImageData);
            inputImageData.Dispose();
            return calculatedPrediction.ToArray();
        }

        public int GetInputLength()
        {
            return 2600;
        }

        public void LoadNetworkFromFile(string filePath)
        {
            var networkJSON = File.ReadAllText(filePath);
            network = SerializationExtensions.FromJson<double>(networkJSON);
            trainer = GetTrainerForNetwork(network);
        }

        public void SaveNetwork(string filePath)
        {
            var networkJSON = network.ToJson();
            FileHelper.WriteTextAsync(filePath, Encoding.ASCII.GetBytes(networkJSON)).RunSynchronously();
        }

        public string GetNetworkName()
        {
            return "ConvNetSharp";
        }
        public int GetBatchSubSize()
        {
            return batchSubSize;
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
            return "convmodel";
        }
    }
}
