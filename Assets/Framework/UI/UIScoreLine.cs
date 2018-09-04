using UnityEngine;
using System.Collections;
using TMPro;
namespace PixelComrades {
    public class UIScoreLine : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI[] _data = new TextMeshProUGUI[4];

        public void SetScore(ScoreEntry score) {
            _data[0].text = score.Date;
            _data[1].text = score.Class;
            _data[2].text = score.KilledBy;
            _data[3].text = score.LevelReached.ToString();
            _data[4].text = score.Score.ToString();
        }
    }
}