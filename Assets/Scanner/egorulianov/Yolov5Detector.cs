using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Barracuda;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Assets.Scripts
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Detection
    {
        public readonly float x, y, w, h;
        public readonly uint classIndex;
        public readonly float score;

        // sizeof(Detection)
        public static int Size = 6 * sizeof(int);

        // String formatting
        public override string ToString()
          => $"({x},{y})-({w}x{h}):{classIndex}({score})";
    };

    public class Yolov5Detector : MonoBehaviour, Detector
    {
        public string INPUT_NAME;

        public int IMAGE_SIZE = 416;
        public int CLASS_COUNT = 3;
        public int OUTPUT_ROWS = 10647;
        public float MINIMUM_CONFIDENCE = 0.25f;
        public int OBJECTS_LIMIT = 20;
        public int LAYERS_PER_FRAME = 1000;

        public NNModel modelFile;
        public TextAsset labelsFile;
        public ComputeShader postprocess;

        private string[] labels;
        private IWorker worker;

        private const int IMAGE_MEAN = 0;
        private const float IMAGE_STD = 255.0F;

        private ComputeBuffer computeBuffer;
        private Detection[] detections;
        //DetectionCache outputCache;

        public void Start()
        {
            this.labels = Regex.Split(this.labelsFile.text, "\n|\r|\r\n")
                .Where(s => !String.IsNullOrEmpty(s)).ToArray();
            var model = ModelLoader.Load(this.modelFile);
            this.worker = GraphicsWorker.GetWorker(model);
            CodeTimer.SetSamples(1);
            computeBuffer = new ComputeBuffer(CLASS_COUNT, Detection.Size);
            detections = new Detection[CLASS_COUNT];

            //transpose layer?
            var builder = new ModelBuilder(model);
            builder.Transpose("transpose", "output", new[] { 0, 3, 2, 1 });
            builder.Output("transpose");
            //outputCache = new DetectionCache(computeBuffer, CLASS_COUNT);
            Debug.Log("async supported: " + (SystemInfo.supportsAsyncGPUReadback && worker.Summary().Contains("Unity.Barracuda.ComputeVarsWithSharedModel")).ToString());
        }

        public void DetectNow(RenderTexture picture, int width, System.Action<IList<BoundingBox>> callback)
        {
            using (var tensor = new Tensor(picture, 3, "images")) //TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width))
            {
                worker.Execute(tensor);
                var output = worker.PeekOutput("output");
                var results = ParseYoloV5Output(output, MINIMUM_CONFIDENCE);

                var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                callback(boxes);
            }
        }

        //starts 570 before it should
        //what if row len is 25010 ??
        int curFeature = 0;
        int offset = 0; // (25200 * 4) - 600;
        int inc = 0; //5;
        RenderTexture texture;

        private TextureFormat textureFormat = TextureFormat.RFloat;
        private RenderTextureFormat renderTextureFormat = RenderTextureFormat.RFloat;

        public IEnumerator DetectV2(RenderTexture picture, int width, System.Action<IList<BoundingBox>> callback)
        {
            //hack
            //MINIMUM_CONFIDENCE = 0.51f;
            using (var tensor = new Tensor(picture, 3, "images")) //TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width))
            {
                int stepsPerFrame = LAYERS_PER_FRAME;
                var automaticSteps = stepsPerFrame <= 0;
                var enumerator = worker.StartManualSchedule(tensor);
                int step = 0;
                DateTime lastYield = DateTime.Now;
                while (enumerator.MoveNext())
                {
                    step++;
                    if (!automaticSteps && step % stepsPerFrame == 0) yield return null;
                    if (automaticSteps && step % 10 == 0)
                    {
                        if ((DateTime.Now - lastYield).TotalSeconds > 1f / 240f)
                        {
                            lastYield = DateTime.Now;
                            yield return null;
                        }
                    }
                }
                //orig shape = (1, 1, 24, 25200)

                var output = worker.PeekOutput("transpose");
                var reshaped = output.Reshape(new TensorShape(1, 350 * 2, 432 * 2, 1));
                if (texture == null)
                {
                    texture = new RenderTexture(reshaped.width, reshaped.height, 0, renderTextureFormat);
                    outputTextureCPU = new Texture2D(reshaped.width, reshaped.height, textureFormat, false);
                }
                reshaped.ToRenderTexture(texture);

                //Debug.Log("orig shape = " + output.shape.ToString());
                /*
                //current findings - seems to reshape into 575 / 576 pos blocks, possibly just not working with ARGBFloat. Maybe try RFloat?
                var reshaped = output.Reshape(new TensorShape(1, 350, 432, 4));
                if(texture == null)
                    texture = new RenderTexture(reshaped.width, reshaped.height, 0, RenderTextureFormat.ARGBFloat);
                reshaped.ToRenderTexture(texture);
                detections = new Detection[CLASS_COUNT];
                for (int i = 0; i < detections.Length; i++)
                    detections[i] = new Detection();
                RunComputeShader(texture);
                //GetComponent<RawImage>().texture = GetComponent<RawImage>().material.mainTexture = texture;
                List<BoundingBox> results = new List<BoundingBox>();
                foreach(var d in detections)
                {
                    //Debug.Log(d.ToString());
                    if(d.score >= MINIMUM_CONFIDENCE)
                    {
                        Debug.Log(d.ToString());
                        results.Add(new BoundingBox
                        {
                            Dimensions = new BoundingBoxDimensions
                            {
                                X = d.x,
                                Y = d.y,
                                Width = d.w,
                                Height = d.h,
                            },
                            ClassIndex = (int)d.classIndex,
                            Confidence = d.score,
                            Label = "test" //labels[d.classIndex]
                        });
                    }
                }
                //Debug.Log("num results " + results.Count);
                //tex2d test
                /*
                RenderTexture.active = texture;
                var tex2D = new Texture2D(output.width, output.height);
                tex2D.ReadPixels(new Rect(0, 0, output.width, output.height), 0, 0);
                for (var i = 0; i < 10; i++)
                {
                    GetFeature(tex2D, curFeature);
                    curFeature++;
                }
                Debug.Log("texture info:" + texture.width + "x" + texture.height + " " + texture.format);
                
                */

                //Debug.Log("async start");
                var req = AsyncGPUReadback.Request(texture, 0, textureFormat, OnCompleteReadback);
                while (!req.done)
                    yield return null;
                
                //Debug.Log("done");
                //GetComponent<RawImage>().texture = GetComponent<RawImage>().material.mainTexture = outputTextureCPU;
                if (outputArray.Length != 25200 * 24)
                {
                    Debug.Log("output length not correct: " + outputArray.Length);
                    callback(new List<BoundingBox>());
                }
                else
                {
                    /*
                    for (var i = 0; i < 10; i++)
                    {
                        GetFeature(curFeature);
                        curFeature++;
                    }
                    */
                    var results = ParseYoloV5OutputV2(MINIMUM_CONFIDENCE);
                    var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                    callback(boxes);

                }
                
                //var results = ParseYoloV5OutputV2(MINIMUM_CONFIDENCE);
                //var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                //Destroy(texture);
                //callback(results);
                //Destroy(tex2D);
            }

        }

        public IEnumerator DetectGPU(RenderTexture picture, int width, System.Action<IList<BoundingBox>> callback)
        {
            //hack
            //MINIMUM_CONFIDENCE = 0.51f;
            using (var tensor = new Tensor(picture, 3, "images")) //TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width))
            {
                var enumerator = worker.StartManualSchedule(tensor);
                int step = 0;
                int stepsPerFrame = LAYERS_PER_FRAME;
                while (enumerator.MoveNext())
                {
                    step++;
                    if (step % stepsPerFrame == 0) yield return null;
                }
                var output = worker.PeekOutput("transpose");
                var reshaped = output.Reshape(new TensorShape(1, 350 * 2, 432 * 2, 1));
                if (texture == null)
                {
                    texture = new RenderTexture(reshaped.width, reshaped.height, 0, renderTextureFormat);
                }
                reshaped.ToRenderTexture(texture);
                detections = new Detection[CLASS_COUNT];
                RunComputeShader(texture);
                List<BoundingBox> results = new List<BoundingBox>();
                foreach (var d in detections)
                {
                    //Debug.Log(d.ToString());
                    if (d.score >= MINIMUM_CONFIDENCE)
                    {
                        //Debug.Log(d.ToString());
                        results.Add(new BoundingBox
                        {
                            Dimensions = new BoundingBoxDimensions
                            {
                                X = d.x,
                                Y = d.y,
                                Width = d.w,
                                Height = d.h,
                            },
                            ClassIndex = (int)d.classIndex,
                            Confidence = d.score,
                            Label = labels[d.classIndex]
                        });
                    }
                }
                callback(results);
            }

        }

        private Texture2D outputTextureCPU;
        float[] outputArray = new float[1];
        Action<IList<BoundingBox>> cback;

        private void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.Log("GPU readback error detected.");
                return;
            }

            if (outputTextureCPU != null)
            {
                try
                {
                    // Load readback data into the output texture and apply changes
                    outputTextureCPU.LoadRawTextureData(request.GetData<uint>());
                    outputTextureCPU.Apply();
                    outputArray = outputTextureCPU.GetPixels().Select(color => color.r).ToArray(); //Reverse()?

                }
                catch (UnityException ex)
                {
                    if (ex.Message.Contains("LoadRawTextureData: not enough data provided (will result in overread)."))
                    {
                        Debug.Log("Updating input data size to match the texture size.");
                    }
                    else
                    {
                        Debug.LogError($"Unexpected UnityException: {ex.Message}");
                    }
                }
            }
        }
        void GetFeature(int featureID)
        {
            //25200 / 4 = 6300
            float[] fullFeature = new float[24];
            var featureString = "feature " + featureID + ":\n";
            for(int i = 0; i < 24; i++)
            {
                fullFeature[i] = GetFloat((featureID * 24) + i);
                featureString += fullFeature[i] + "\n";
                if (i == 4) featureString += "---------\n";
            }
            Debug.Log(featureString);
           
        }

        float GetFeature(int element, int feature)
        {
            return GetFloat((element * 24) + feature);
        }
        float GetFloat(int rawPosition)
        {
            //432 x 350
            // rawPosition = 604800 - 1 - rawPosition;
            return outputArray[rawPosition];
        }
        public IEnumerator DetectV3(RenderTexture picture, int width, System.Action<IList<BoundingBox>> callback)
        {
            using (var tensor = new Tensor(picture, 3, "images")) //TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width))
            {

                var output = worker.Execute(tensor).PeekOutput("output");
                //this is bad, waits for cpu synchronization, very slow
                yield return new WaitForCompletion(output);
                
                var results = ParseYoloV5Output(output, MINIMUM_CONFIDENCE);

                var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                
                callback(boxes);
            }
        }

        public IEnumerator Detect(Color32[] picture, int width, System.Action<IList<BoundingBox>> callback)
        {
            using (var tensor = TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width))
            {
                var inputs = new Dictionary<string, Tensor>();
                inputs.Add(INPUT_NAME, tensor);
                yield return StartCoroutine(worker.StartManualSchedule(inputs));

                var output = worker.PeekOutput("output");
                var results = ParseYoloV5Output(output, MINIMUM_CONFIDENCE);
                
                var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                callback(boxes);
            }
        }

        public IEnumerator Detect(Texture picture, int width, System.Action<IList<BoundingBox>> callback)
        {
            using (var tensor = new Tensor(picture, 3, "images"))
            {
                var itr = worker.StartManualSchedule(tensor);
                Debug.Log(worker.scheduleProgress);
                while (worker.scheduleProgress < 1f)
                {
                    Debug.Log(worker.scheduleProgress);
                    for (int i = LAYERS_PER_FRAME; i > 0 && itr.MoveNext(); i--) ;
                    worker.FlushSchedule();
                    yield return itr;
                }
                var output = worker.PeekOutput("output");
                var results = ParseYoloV5Output(output, MINIMUM_CONFIDENCE);

                var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                callback(boxes);
            }
        }

        public void DetectNow(Color32[] picture, int width, System.Action<IList<BoundingBox>> callback)
        {
            using (var tensor = TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width))
            {
                worker.Execute(tensor);

                var output = worker.PeekOutput("output");
                var results = ParseYoloV5Output(output, MINIMUM_CONFIDENCE);

                var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                callback(boxes);
            }
        }

        void RunComputeShader(RenderTexture output)
        {

            computeBuffer.SetData(detections);
            postprocess.SetInt("InputWidth", output.width);
            postprocess.SetFloat("Threshold", MINIMUM_CONFIDENCE);
            postprocess.SetTexture(0, "Input", output);
            postprocess.SetInt("wtfOffset", offset);
            postprocess.SetBuffer(0, "Output", computeBuffer);
            postprocess.Dispatch(0, 150, 1, 1);
            //_cached = new Detection[count];
            computeBuffer.GetData(detections);//, 0, 0, CLASS_COUNT);
        }
        private static Tensor TransformInput(Color32[] pic, int width, int height, int requestedWidth)
        {
            float[] floatValues = new float[width * height * 3];

            int beginning = (((pic.Length / requestedWidth) - height) * requestedWidth) / 2;
            int leftOffset = (requestedWidth - width) / 2; 
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var color = pic[beginning + leftOffset + j];

                    floatValues[(i * width + j) * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
                    floatValues[(i * width + j) * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
                    floatValues[(i * width + j) * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
                }
                beginning += requestedWidth;
            }

            return new Tensor(1, height, width, 3, floatValues);
        }

        //todo - shader this
        private IList<BoundingBox> ParseYoloV5Output(Tensor tensor, float thresholdMax)
        {
            var boxes = new List<BoundingBox>();

            for (int i = 0; i < OUTPUT_ROWS; i++)
            {
                float confidence = GetConfidence(tensor, i);
                if (confidence < thresholdMax)
                    continue;

                BoundingBoxDimensions dimensions = ExtractBoundingBoxDimensionsYolov5(tensor, i);
                (int classIdx, float maxClass) = GetClassIdx(tensor, i);

                float maxScore = confidence * maxClass;

                if (maxScore < thresholdMax)
                    continue;

                boxes.Add(new BoundingBox
                {
                    Dimensions = MapBoundingBoxToCell(dimensions),
                    Confidence = confidence,
                    Label = labels[classIdx],
                    ClassIndex = classIdx
                });
            }

            return boxes;
        }

        private IList<BoundingBox> ParseYoloV5OutputV2(float thresholdMax)
        {
            var boxes = new List<BoundingBox>();

            for (int i = 0; i < OUTPUT_ROWS; i++)
            {
                float confidence = Sigmoid(GetFeature(i, 4));
                if (confidence < thresholdMax)
                    continue;

                BoundingBoxDimensions dimensions = ExtractBoundingBoxDimensionsYolov5(i);
                (int classIdx, float maxClass) = GetClassIdx(i);

                float maxScore = confidence * maxClass;

                if (maxScore < thresholdMax)
                    continue;

                boxes.Add(new BoundingBox
                {
                    Dimensions = MapBoundingBoxToCell(dimensions),
                    Confidence = confidence,
                    Label = labels[classIdx],
                    ClassIndex = classIdx
                });
            }

            return boxes;
        }

        private BoundingBoxDimensions ExtractBoundingBoxDimensionsYolov5(Tensor tensor, int row)
        {
            return new BoundingBoxDimensions
            {
                X = tensor[0, 0, 0, row],
                Y = tensor[0, 0, 1, row],
                Width = tensor[0, 0, 2, row],
                Height = tensor[0, 0, 3, row]
            };
        }

        private BoundingBoxDimensions ExtractBoundingBoxDimensionsYolov5(int row)
        {
            return new BoundingBoxDimensions
            {
                X = GetFeature(row, 0),
                Y = GetFeature(row, 1),
                Width = GetFeature(row, 2),
                Height = GetFeature(row, 3)
            };
        }

        private float GetConfidence(Tensor tensor, int row)
        {
            float tConf = tensor[0, 0, 4, row];
            return Sigmoid(tConf);
        }

        private ValueTuple<int, float> GetClassIdx(Tensor tensor, int row)
        {
            int classIdx = 0;

            float maxConf = tensor[0, 0, 5, row];

            for (int i = 0; i < CLASS_COUNT; i++)
            {
                if (tensor[0, 0, 5 + i, row] > maxConf)
                {
                    maxConf = tensor[0, 0, 5 + i, row];
                    classIdx = i;
                }
            }
            return (classIdx, maxConf);
        }

        private ValueTuple<int, float> GetClassIdx(int row)
        {
            int classIdx = 0;

            float maxConf = GetFeature(row, 5);

            for (int i = 0; i < CLASS_COUNT; i++)
            {
                float thisConf = GetFeature(row, i + 5);
                if (thisConf > maxConf)
                {
                    maxConf = thisConf;
                    classIdx = i;
                }
            }
            return (classIdx, maxConf);
        }

        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);

            return k / (1.0f + k);
        }

        private BoundingBoxDimensions MapBoundingBoxToCell(BoundingBoxDimensions boxDimensions)
        {
            return new BoundingBoxDimensions
            {
                X = (boxDimensions.X) * (IMAGE_SIZE / IMAGE_SIZE),
                Y = (boxDimensions.Y) * (IMAGE_SIZE / IMAGE_SIZE),
                Width = boxDimensions.Width * (IMAGE_SIZE / IMAGE_SIZE),
                Height = boxDimensions.Height * (IMAGE_SIZE / IMAGE_SIZE),
            };
        }

        private IList<BoundingBox> FilterBoundingBoxes(IList<BoundingBox> boxes, int limit, float threshold)
        {
            var activeCount = boxes.Count;
            var isActiveBoxes = new bool[boxes.Count];
            var classesFound = new bool[CLASS_COUNT];

            for (int i = 0; i < isActiveBoxes.Length; i++)
            {
                isActiveBoxes[i] = true;
            }
            for (int i = 0; i < classesFound.Length; i++)
                classesFound[i] = false;

            var sortedBoxes = boxes.Select((b, i) => new { Box = b, Index = i })
                    .OrderByDescending(b => b.Box.Confidence)
                    .ToList();

            var results = new List<BoundingBox>();

            for (int i = 0; i < boxes.Count; i++)
            {
                if (isActiveBoxes[i] && !classesFound[sortedBoxes[i].Box.ClassIndex])
                {
                    var boxA = sortedBoxes[i].Box;
                    //we know there's only one of each class possible in this case, so ignore the lower confidence results
                    classesFound[sortedBoxes[i].Box.ClassIndex] = true;
                    results.Add(boxA);

                    if (results.Count >= limit)
                        break;

                    for (var j = i + 1; j < boxes.Count; j++)
                    {
                        if (isActiveBoxes[j])
                        {
                            var boxB = sortedBoxes[j].Box;

                            if (IntersectionOverUnion(boxA.Rect, boxB.Rect) > threshold)
                            {
                                isActiveBoxes[j] = false;
                                activeCount--;

                                if (activeCount <= 0)
                                    break;
                            }
                        }
                    }

                    if (activeCount <= 0)
                        break;
                }
            }

            return results;
        }

        private float IntersectionOverUnion(Rect boundingBoxA, Rect boundingBoxB)
        {
            var areaA = boundingBoxA.width * boundingBoxA.height;

            if (areaA <= 0)
                return 0;

            var areaB = boundingBoxB.width * boundingBoxB.height;

            if (areaB <= 0)
                return 0;

            var minX = Math.Max(boundingBoxA.xMin, boundingBoxB.xMin);
            var minY = Math.Max(boundingBoxA.yMin, boundingBoxB.yMin);
            var maxX = Math.Min(boundingBoxA.xMax, boundingBoxB.xMax);
            var maxY = Math.Min(boundingBoxA.yMax, boundingBoxB.yMax);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }
    }
}
