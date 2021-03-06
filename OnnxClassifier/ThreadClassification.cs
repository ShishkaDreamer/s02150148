﻿using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;


namespace OnnxClassifier
{
    public class ResultClassification
    {
        private string PathImage;
        private string ClassImage;
        public float Probability;

        public ResultClassification(string path, string ci, float prob)
        {
            PathImage = path;
            ClassImage = ci;
            Probability = prob;
        }

        public string _ClassImage {
            get { return ClassImage; }

        }

        public string _PathImage
        {
            get { return PathImage; }

        }

       

        public override string ToString()
        {
            return "  " + PathImage.Remove(0 , 15) + " is a " + ClassImage + " chance = " + Probability.ToString();
        }
    }



    public class ThreadClassification
    {

        Action<ResultClassification> ImageRecognitionCompleted;


        private ConcurrentQueue<string> PathImages = new ConcurrentQueue<string>();
        public ConcurrentQueue<ResultClassification> Result = new ConcurrentQueue<ResultClassification>();

        private CancellationTokenSource CancelThreads = new CancellationTokenSource(); //для пула потоков

        private OnnxClassifier Model;

        public ThreadClassification(string currentDirectory, OnnxClassifier onnxModel, Action<ResultClassification> handler)
        {
            Model = onnxModel;
            ImageRecognitionCompleted = handler;


            PathImages = new ConcurrentQueue<string>(Directory.GetFiles(currentDirectory, "*.jpg"));
        }

        public ThreadClassification(ConcurrentQueue<string> pathImages, OnnxClassifier onnxModel, Action<ResultClassification> handler)
        {
            Model = onnxModel;
            ImageRecognitionCompleted = handler;


            PathImages = pathImages;
        }

        public void Run()
        {
            

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                var th = new Thread(Worker);
                th.Name = $"Thread {i}";
                th.Start();
            }

            

        }

        public void Stopper()
        {
            CancelThreads.Cancel();

        }



        private void Worker()
        {

            while (!CancelThreads.Token.IsCancellationRequested && PathImages.TryDequeue(out string image))
            {
                ResultClassification result = Model.PredictModel(image);
                Result.Enqueue(result);
                ImageRecognitionCompleted(result);

            }

        }
    
    
    }
}
