using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Video;
using System.IO;
using UnityEngine.UI;
using Unity.Barracuda;
using UnityEngine.Profiling;
using Assets.Scripts;
using TMPro;

public class Scanner_Ego : MonoBehaviour
{
    public struct PositionDataPoint
    {
        public Vector2 coordinates;
        public float confidence;
        public DateTime time;
    }
    public class PositionHistory
    {
        public List<PositionDataPoint> timeline = new List<PositionDataPoint>();
        public void AddPosition(Vector2 position, float confidence, DateTime time)
        {
            var newPos = new PositionDataPoint{ confidence = confidence, coordinates = position, time = time };
            timeline.Add(newPos);
            if (timeline.Count >= 5)
                timeline.RemoveAt(0);
        }
        public PositionDataPoint lastKnownPosition { get { return timeline.Count > 0 ? timeline[timeline.Count - 1] : default(PositionDataPoint); } }
        public Vector2 projectedPosition { get
            {
                if ((DateTime.Now - lastKnownPosition.time).TotalSeconds < 0.05f || CVManager.useFrameBuffer)
                    return lastKnownPosition.coordinates;

                int numValidPositions = 0;
                List<PositionDataPoint> points = new List<PositionDataPoint>();
                for(int i = 0; i < timeline.Count; i++)
                {
                    if ((DateTime.Now - timeline[i].time).TotalMilliseconds < 500f)
                    {
                        points.Add(timeline[i]);
                    }
                }
                if (points.Count < 2 || (DateTime.Now - lastKnownPosition.time).TotalSeconds > .25f) // not enough to do anything with, or last inference was too long ago
                {
                    return new Vector2(-99f, -99f);
                } else if(points.Count < 3) //not enough to extrapolate, use last known position
                {
                    return lastKnownPosition.coordinates;
                }
                //take average of guesses from all points
                List<Vector2> allPositionGuesses = new List<Vector2>();
                for(int i = 0; i < points.Count; i++)
                {
                    var pointA = points[i];
                    for(int j = i + 1; j < points.Count; j++){
                        var pointB = points[j];
                        if(j == i + 1 && Vector2.Distance(pointA.coordinates, pointB.coordinates) > 100f)
                        {
                            //two consecutive estimates are too far apart, don't trust extrapolation
                            return lastKnownPosition.coordinates;
                        }
                        var vec = (pointB.coordinates - pointA.coordinates) / (float)(pointB.time - pointA.time).TotalSeconds;
                        allPositionGuesses.Add(vec);
                    }
                }
                Vector2 averageAverage = Vector2.zero;
                var last = points[points.Count - 1];
                foreach (var guess in allPositionGuesses)
                    averageAverage += guess;
                averageAverage /= (float)allPositionGuesses.Count;
                Vector2 projection = last.coordinates + (averageAverage * (float)(DateTime.Now - last.time).TotalSeconds);
                return projection;
            } }
    }
    [System.Serializable]
    public class PlayerPosition
    {
        public int teamID;
        public int positionID;
        public string names;
        public float curConfidence;
        public Vector2 curPosition;
        public DateTime lastPositionTime;
        public PlayerCVTracker label;
        public SpriteRenderer hat;
        public PositionHistory positionTimeline;
        public bool isWarrior;
        public bool isQueen;
        public bool isSnail;
        public bool isVisible;
    }
    public List<PlayerPosition> playerPositions = new List<PlayerPosition>();
    public Yolov5Detector detector;
    public bool inferenceMode = false;
    public WebcamController webcam;

    public IWorker worker;
    public float detectionConfidence = .8f;
    public float inferenceConfidence = .25f;
    public int lpf = 1000;
    public static float positionHistoryLength = 1f;
    public bool logResults = true;
    public bool alwaysShow = false;
    public bool showNames = true;
    public bool showHats = true;
    public bool showFire = true;
    public Sprite defaultHat;
    public Vector2 labelOrigin;
    public Vector2 labelBoundaries;
    public PlayerCVTracker playerLabelTemplate;

    TextureScaler textureScaler;
    bool GCInProgress = false;
    int nextScanFrame = 10;
    int ssNumber = 1;
    bool currentlyRecording = false;
    bool currentlyPlaying = false;
    Texture2D[] recordingTrack;
    RenderTexture blitTex;
    int recordingLocation = 0;
    bool detectionInProgress = false;
    bool inferenceDidFinish = false;
    RenderTexture lastTex;

    Color emptyColor = new Color(0f, 0f, 0f, 0f);
    DateTime inferenceStartTime;
    DateTime lastInferenceStartTime;

    IEnumerator CVCoroutine;

    public void Init(WebcamController wc, bool startEnabled)
    {
        webcam = wc;
        inferenceMode = startEnabled;
    }

    // Start is called before the first frame update
    void Start()
    {
        //worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
        textureScaler = new TextureScaler(640, 640);


        recordingTrack = new Texture2D[300];
        int recordWidth = 1280 / 2;
        int recordHeight = 720 / 2;

        LSConsole.AddCommandHook("cvDetectionConf", "set the minimum [confidence] for detecting a player sprite", DetectionConfidenceCommand);
        LSConsole.AddCommandHook("cvInferenceConf", "set the minimum [confidence] for filtering detection results for sprites", InferenceConfidenceCommand);
        LSConsole.AddCommandHook("cvLayers", "set the number of [layers] processed per frame in the CV module. 0 will attempt automatic layers (to maintain framerate) and -1 will process as fast as possible", CVProcessSpeedCommand);
        LSConsole.AddCommandHook("cvLog", "[start] or [stop] getting logs of CV inference results", CVLogCommand);
        LSConsole.AddCommandHook("cvDebug", "[enable] or [disable] CV debug mode, which always shows hats and position names", CVDebugCommand);
        LSConsole.AddCommandHook("cvOptions", "list things to show when detecting a character: [hats], [name], and/or [fire]", CVOptionsCommand);

        lastTex = new RenderTexture(640, 640, 24);

        foreach (var p in playerPositions)
        {
            var label = Instantiate(playerLabelTemplate);
            p.label = label;
            p.hat = label.GetComponentInChildren<SpriteRenderer>();
            p.hat.enabled = false;
            p.positionTimeline = new PositionHistory();
        }

    }

    string DetectionConfidenceCommand(string [] options)
    {
        if (options.Length == 0) return "Current detection confidence: " + detectionConfidence;
        detectionConfidence = float.Parse(options[0]);
        return "";
    }

    string InferenceConfidenceCommand(string[] options)
    {
        if (options.Length == 0) return "Current inference confidence: " + inferenceConfidence;
        inferenceConfidence = float.Parse(options[0]);
        return "";
    }

    string CVProcessSpeedCommand(string[] options)
    {
        if (options.Length == 0) return "Current layers per frame: " + lpf;
        lpf = int.Parse(options[0]);
        return "";
    }

    string CVDebugCommand(string[] options)
    {
        if (options.Length == 0) alwaysShow = !alwaysShow;
        else alwaysShow = options[0].Contains("enable") ? true : false;
        return "";
    }

    string CVLogCommand(string[] options)
    {
        if (options.Length == 0) logResults = !logResults;
        logResults = options[0] == "start" ? true : false;
        return "";
    }

    string CVOptionsCommand(string[] options)
    {
        showNames = showHats = showFire = false;
        foreach(var s in options)
        {
            switch(s)
            {
                case "hats": showHats = true; break;
                case "names": showNames = true; break;
                case "fire": showFire = true; break;
            }
        }
        return "";
    }

    private void Update()
    {
        int numDetections = 0;
        if (!detectionInProgress && inferenceMode && (GameModel.instance.gameIsRunning.property || alwaysShow))
        {
            StartInference();
        }

        foreach (var player in playerPositions)
        {
            var pos = player.positionTimeline.projectedPosition;
            var droneHeightOffset = player.isQueen ? .00f : player.isWarrior ? 0f : -.15f;

            bool onFire = false;

            if(player.isSnail && GameModel.instance.gameIsRunning.property)
            {
                player.label.nameText.text = "";
                player.hat.enabled = false;
                CVManager.snailObservedPosition = player.curPosition.x * 3f; //640 x 3 = 1920 game resolution
            }
            else if (pos.x > -90 && (GameModel.instance.gameIsRunning.property || alwaysShow)) //-99 is code for not found
            {
                player.isVisible = true;
                numDetections++;
                player.label.transform.position = new Vector3(
                    labelOrigin.x + ((pos.x / 640f) * labelBoundaries.x),
                    labelOrigin.y - ((pos.y / 360f) * labelBoundaries.y) + droneHeightOffset, 0f); //max is 640 * (9/16) = 360
                var name = "";
                player.hat.sprite = null;
                if(player.teamID <= 1)
                {
                    var model = GameModel.GetPlayer(player.teamID, player.positionID);
                    name = showNames ? model.playerName.property : "";
                    player.hat.sprite = showHats ? model.hat : null;
                    onFire = showFire ? model.isOnFire.property : false;
                    if(alwaysShow)
                    {
                        name = model.positionName;
                        player.hat.sprite = defaultHat;
                        onFire = true;
                    }
                }
                player.label.nameText.text = name;
                player.hat.enabled = true;
                player.label.SetFire(onFire ? player.teamID : -1);
            }
            else if (player.isVisible)
            {
                player.label.nameText.text = "";
                player.hat.enabled = false;
                player.isVisible = false;
                player.label.SetFire(-1);
            }
        }

        if(inferenceDidFinish)
        {
            var finalTime = (DateTime.Now - lastInferenceStartTime).TotalSeconds;
            CVManager.OnInferenceComplete((float)finalTime);
            if (logResults)
                Debug.Log("inference time: " + finalTime + ", frameBuffer " + VideoClipper.frameBuffer + " numDetections " + numDetections);
            inferenceDidFinish = false;
        }

        /*else if(CVCoroutine != null)
        {
            for (int i = lpf; i > 0 && CVCoroutine.MoveNext(); i--) ;
        }*/
    }

    void StartInference()
    {
        detectionInProgress = true;
        inferenceStartTime = DateTime.Now;
        TextureCropTools.SquareAndScaleToRenderTexture(webcam.GetWebcamTexture, lastTex);
        if (lastTex == null)
        {
            Debug.Log("lastTex is null");
            return;
        }

        if(logResults)
            Debug.Log("inference start");
        
        detector.MINIMUM_CONFIDENCE = detectionConfidence;
        detector.LAYERS_PER_FRAME = lpf;

        StartCoroutine(detector.DetectGPU(lastTex, 640, OnInferenceComplete));
        /*
        if (lpf < 0) //detect as quickly as possible
            detector.DetectNow(lastTex, 640, OnInferenceComplete);
        //else if(lpf == 0) //detect with coroutine (should hopefully maintain framerate)
        //    StartCoroutine(detector.DetectV3(finalTex, 640, OnInferenceComplete));
        else //detect with custom # of layers per frame to control framerate better
            StartCoroutine(detector.DetectV2(lastTex, 640, OnInferenceComplete));
        */
        //Destroy(rt);
        //rt = null;
    }

    void OnInferenceComplete(IList<BoundingBox> boxes)
    {
        int numConfidentBoxes = 0;
        foreach (var p in playerPositions) p.curConfidence = 0f;
        foreach(var b in boxes)
        {
            
            if (b.Confidence >= inferenceConfidence)
            {
                numConfidentBoxes++;
                var pos = playerPositions.Find(x => x.names.IndexOf(b.Label) > -1);
                if (pos == null || pos.curConfidence > b.Confidence) continue;
                pos.curConfidence = b.Confidence;
                pos.curPosition = new Vector2(b.Dimensions.X, b.Dimensions.Y - 140);
                pos.lastPositionTime = inferenceStartTime;
                pos.isWarrior = b.Label.Contains("warrior");
                pos.isQueen = b.Label.Contains("queen");
                pos.isSnail = b.Label.Contains("snail");
                //Debug.Log(b.ToString());
            }
        }
        foreach(var player in playerPositions)
        {
            if(player.curConfidence > 0f)
            {
                player.positionTimeline.AddPosition(player.curPosition, player.curConfidence, inferenceStartTime);
            }
        }
        lastInferenceStartTime = inferenceStartTime;
        detectionInProgress = false;
        inferenceDidFinish = true;
        //start next inference right away (unless we demand immediate results)
        if (lpf >= 0 && inferenceMode && (GameModel.instance.gameIsRunning.property || alwaysShow))
        {
            StartInference();
        }
    }


    private void OnDestroy()
    {
        //detector.Dispose();
    }
}
