using DG.Tweening;
using UnityEngine;

    public class SQEggObserver : KQObserver
    {
        public SpriteRenderer[] eggs;
        //blue day, gold day, blue night...
        float[] xPositions = new float[]{ -7.39f, 7.3f, -42.3f, 42.14f, -12.52f, 12.3f, -36.7f, 36.6f };
        float[] yPositions = new float[]{ 51.3f, 51.3f, 8.11f, 8.11f, 32.7f, 32.7f, 9.7f, 9.7f};

        float[] secondEggDelta = new float[] {3.7f, 3.7f, -3.7f, -3.7f, 3.7f, 3.7f, -3.7f, -3.7f};
        int[] queenLives = new int[] { 2, 2 };

        // Use this for initialization
        void Start()
        {
            for (var i = 0; i < 2; i++)
            {
                foreach (var p in GameModel.instance.teams[i].players)
                {
                    var copiedIndex = i; //this is needed to capture i and pass into funcs as value and not reference
                    p.curGameStats.queenKills.onChange.AddListener((b, a) => { if(a > b) OnQueenKill(copiedIndex, a - b); });
                }
            }
            MapDB.currentMap.onChange.AddListener(OnMapChange);
            GameModel.onGameModelComplete.AddListener((_, __) => OnGameEnd());
        }

        void OnMapChange(MapData before, MapData after)
        {
            int mapIndex = 0;
            var map = after.display_name;
            switch(map)
            {
                case "Day": mapIndex = 0; break;
                case "Night": mapIndex = 2; break;
                case "Dusk": mapIndex = 4; break;
                case "Twilight": mapIndex = 6; break;
                default: mapIndex = -1; break;
            }
            SetAllEggs(mapIndex != -1);
            queenLives[0] = queenLives[1] = 2;
            if(mapIndex == -1)
            {
                return;
            }

            eggs[0].transform.localPosition = new Vector3(xPositions[mapIndex], yPositions[mapIndex], 0f);
            eggs[1].transform.localPosition = new Vector3(xPositions[mapIndex]+secondEggDelta[mapIndex], yPositions[mapIndex], 0f);
            eggs[2].transform.localPosition = new Vector3(xPositions[mapIndex+1], yPositions[mapIndex], 0f);
            eggs[3].transform.localPosition = new Vector3(xPositions[mapIndex+1]-secondEggDelta[mapIndex], yPositions[mapIndex], 0f);

        }

        void OnGameEnd()
        {
            SetAllEggs(false);
        }
        void SetAllEggs(bool show)
        {
            foreach(var egg in eggs)
                egg.color = new Color(1f,1f,1f,show ? 1f : 0f);
        }
        void OnQueenKill(int teamID, int score)
        {
            int damagedQueen = 1 - teamID;
            queenLives[damagedQueen] -= 1;
            int eggToAnimate = 1 - queenLives[damagedQueen];

            if(eggToAnimate < 0 || eggToAnimate > 1) return; //invalid egg
            if(damagedQueen == 1) eggToAnimate += 2;

            eggs[eggToAnimate].DOColor(new Color(1f,1f,1f,0f), .5f);
        }

    }