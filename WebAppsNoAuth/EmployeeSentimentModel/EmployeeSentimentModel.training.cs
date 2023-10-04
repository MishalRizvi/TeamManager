// This file was auto-generated by ML.NET Model Builder.
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Transforms;

namespace EmployeeSentimentModel.ConsoleApp
{
    public partial class EmployeeSentimentModel
    {
        public const string RetrainFilePath =  @"/Users/mishalrizvi/Projects/TeamManager/WebAppsNoAuth/WebAppsNoAuth/wwwroot/train_set.csv";
        public const char RetrainSeparatorChar = ',';
        public const bool RetrainHasHeader =  true;

         /// <summary>
        /// Train a new model with the provided dataset.
        /// </summary>
        /// <param name="outputModelPath">File path for saving the model. Should be similar to "C:\YourPath\ModelName.mlnet"</param>
        /// <param name="inputDataFilePath">Path to the data file for training.</param>
        /// <param name="separatorChar">Separator character for delimited training file.</param>
        /// <param name="hasHeader">Boolean if training file has a header.</param>
        public static void Train(string outputModelPath, string inputDataFilePath = RetrainFilePath, char separatorChar = RetrainSeparatorChar, bool hasHeader = RetrainHasHeader)
        {
            var mlContext = new MLContext();

            var data = LoadIDataViewFromFile(mlContext, inputDataFilePath, separatorChar, hasHeader);
            var model = RetrainModel(mlContext, data);
            SaveModel(mlContext, model, data, outputModelPath);
        }

        /// <summary>
        /// Load an IDataView from a file path.
        /// </summary>
        /// <param name="mlContext">The common context for all ML.NET operations.</param>
        /// <param name="inputDataFilePath">Path to the data file for training.</param>
        /// <param name="separatorChar">Separator character for delimited training file.</param>
        /// <param name="hasHeader">Boolean if training file has a header.</param>
        /// <returns>IDataView with loaded training data.</returns>
        public static IDataView LoadIDataViewFromFile(MLContext mlContext, string inputDataFilePath, char separatorChar, bool hasHeader)
        {
            return mlContext.Data.LoadFromTextFile<ModelInput>(inputDataFilePath, separatorChar, hasHeader);
        }



        /// <summary>
        /// Save a model at the specified path.
        /// </summary>
        /// <param name="mlContext">The common context for all ML.NET operations.</param>
        /// <param name="model">Model to save.</param>
        /// <param name="data">IDataView used to train the model.</param>
        /// <param name="modelSavePath">File path for saving the model. Should be similar to "C:\YourPath\ModelName.mlnet.</param>
        public static void SaveModel(MLContext mlContext, ITransformer model, IDataView data, string modelSavePath)
        {
            // Pull the data schema from the IDataView used for training the model
            DataViewSchema dataViewSchema = data.Schema;

            using (var fs = File.Create(modelSavePath))
            {
                mlContext.Model.Save(model, dataViewSchema, fs);
            }
        }


        /// <summary>
        /// Retrains model using the pipeline generated as part of the training process.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="trainData"></param>
        /// <returns></returns>
        public static ITransformer RetrainModel(MLContext mlContext, IDataView trainData)
        {
            var pipeline = BuildPipeline(mlContext);
            var model = pipeline.Fit(trainData);

            return model;
        }


        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(@"feedback", @"feedback", outputKind: OneHotEncodingEstimator.OutputKind.Indicator)      
                                    .Append(mlContext.Transforms.ReplaceMissingValues(new []{new InputOutputColumnPair(@"id", @"id"),new InputOutputColumnPair(@"potential_class", @"potential_class"),new InputOutputColumnPair(@"data_type", @"data_type")}))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"person_name",outputColumnName:@"person_name"))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"adjusted",outputColumnName:@"adjusted"))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"reviewed",outputColumnName:@"reviewed"))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"label",outputColumnName:@"label"))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"feedback_len",outputColumnName:@"feedback_len"))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"num_of_sent",outputColumnName:@"num_of_sent"))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"performance_class",outputColumnName:@"performance_class"))      
                                    .Append(mlContext.Transforms.Concatenate(@"Features", new []{@"feedback",@"id",@"potential_class",@"data_type",@"person_name",@"adjusted",@"reviewed",@"label",@"feedback_len",@"num_of_sent",@"performance_class"}))      
                                    .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName:@"nine_box_category",inputColumnName:@"nine_box_category",addKeyValueAnnotationsAsText:false))      
                                    .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(binaryEstimator:mlContext.BinaryClassification.Trainers.FastTree(new FastTreeBinaryTrainer.Options(){NumberOfLeaves=4,MinimumExampleCountPerLeaf=20,NumberOfTrees=4,MaximumBinCountPerFeature=254,FeatureFraction=1,LearningRate=0.09999999999999998,LabelColumnName=@"nine_box_category",FeatureColumnName=@"Features"}),labelColumnName: @"nine_box_category"))      
                                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName:@"PredictedLabel",inputColumnName:@"PredictedLabel"));

            return pipeline;
        }
    }
 }
