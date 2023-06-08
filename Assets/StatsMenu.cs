using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DO
{
    public class StatsMenu : MonoBehaviour
    {
        public float playTime = 0f;
        public TextMeshProUGUI playTimeText;
        public bool isTimerRunning = true;

        public int timesEatenCounter = 0;
        public TextMeshProUGUI timesEatenText;

        public int timesCaughtCounter = 0;
        public TextMeshProUGUI timesCaughtText;

        public int timesLayedEggsCounter = 0;
        public TextMeshProUGUI timesEggsLayedText;

        public int timesJumpedCounter = 0;
        public TextMeshProUGUI timesJumpedText;

        public void Update()
        {
            timesEatenText.text = "Food Items Eaten: " + timesEatenCounter;
            timesCaughtText.text = "Times Caught: " + timesCaughtCounter;
            timesEggsLayedText.text = "Eggs layed: " + timesLayedEggsCounter;
            timesJumpedText.text = "Times Jumped: " + timesJumpedCounter;
        }

        public void Start()
        {
            StartCoroutine(UpdatePlayTime());
        }

        public IEnumerator UpdatePlayTime()
        {
            while (isTimerRunning)
            {
                playTime += Time.deltaTime;
                UpdatePlayTimeText();
                yield return null;
            }
        }

        public void UpdatePlayTimeText()
        {
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            playTimeText.text = "Play Time: " + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }

        public void StopTimer()
        {
            isTimerRunning = false;
        }
    }
}
