using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AccuracyCounter
{
    [HarmonyPatch]
    [BepInPlugin("com.steven.trombone.accuracycounter", "Accuracy Counter", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            var harmony = new Harmony("com.steven.trombone.accuracycounter");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(GameController), "Start")]
        static void Postfix(GameController __instance, List<float[]> ___leveldata)
        {
            if (__instance.freeplay) return;

            var score = GameObject.Find("ScoreShadow");
            var counter = Instantiate(score, score.transform.position, score.transform.rotation).AddComponent<Counter>();
            counter.gameObject.name = "Accuracy Counter";
            counter.transform.parent = score.transform.parent;
            counter.Init(___leveldata);
        }

        [HarmonyPatch(typeof(GameController), "getScoreAverage")]
        static void Postfix(int ___totalscore, int ___currentnoteindex)
        {
            Counter.onScoreChanged.Invoke(___totalscore, ___currentnoteindex);
        }

        class Counter : MonoBehaviour
        {
            private Text _foregroundText;
            private Text _shadowText;

            private List<float[]> _levelData;

            private int _maxScoreSoFar = 0;

            public static Action<int, int> onScoreChanged;

            public void Init(List<float[]> leveldata)
            {
                _levelData = leveldata;

                transform.localScale = Vector3.one;
            }

            void Start()
            {
                _foregroundText = transform.Find("Score").GetComponent<Text>();
                _shadowText = GetComponent<Text>();

                SetText("S");

                onScoreChanged += OnScoreChanged;

                var rect = GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 25);
            }

            void OnDestroy() => onScoreChanged -= OnScoreChanged;

            void OnScoreChanged(int totalScore, int noteIndex)
            {
                var scoreForNoteIndex = Mathf.Floor(Mathf.Floor(_levelData[noteIndex][1] * 10f) * 100f * 1.3f) * 10f;
                _maxScoreSoFar += Mathf.FloorToInt(scoreForNoteIndex);

                var scorePercentage = (float)totalScore / _maxScoreSoFar;
                string letterScore = CalculateLetterScore(scorePercentage);

                SetText(letterScore);
            }

            string CalculateLetterScore(float scorePercentage)
            {
                if (scorePercentage > 1f)
                    return "S";
                else if (scorePercentage > 0.8f)
                    return "A";
                else if (scorePercentage > 0.6f)
                    return "B";
                else if (scorePercentage > 0.4f)
                    return "C";
                else if (scorePercentage > 0.2f)
                    return "D";
                else
                    return "F";
            }

            void SetText(string text)
            {
                _foregroundText.text = text;
                _shadowText.text = text;
            }
        }
    }
}
