using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace PixelComrades {
    public class UIScoreList : MonoSingleton<UIScoreList> {
        [SerializeField] private UIScoreLine _scorePrefab = null;
        [SerializeField] private Transform _listTr = null;

        private List<UIScoreLine> _currentLines = new List<UIScoreLine>(10);

        public void ListScores() {
            for (int i = 0; i < _currentLines.Count; i++) {
                ItemPool.Despawn(_currentLines[i].gameObject);
            }
            _currentLines.Clear();

            var scores = ScoreKeeper.RetrieveScores();
            if (scores == null || scores.Length == 0) {
                return;
            }
            for (int i = 0; i < scores.Length; i++) {
                var newLine = ItemPool.SpawnUIPrefab<UIScoreLine>(_scorePrefab.gameObject, _listTr);
                newLine.SetScore(scores[i]);
                _currentLines.Add(newLine);
            }
        }

        public void CloseList() {
            //UIMainMenu.main.CloseScores();
        }
    }

    public static class ScoreKeeper {

        private static string _scoreKey = "PlayerScore";
        private static int _limit = 10;
        public static void AddCurrentScore(int score, string killedBy, int level) {
            var newScore = new ScoreEntry();
            newScore.Date = DateTime.Today.ToShortDateString();
            newScore.Score = score;
            newScore.LevelReached = level;
            newScore.KilledBy = killedBy;
            //newScore.Class = Player.Actor.Class.Current.Template.Name;
            var scores = RetrieveScores();
            if (scores == null || scores.Length == 0) {
                scores = new ScoreEntry[1];
                scores[0] = newScore;
                SaveData<ScoreEntry>(scores, _scoreKey);
                return;
            }
            var listScores = new List<ScoreEntry>();
            listScores.AddRange(scores);
            if (listScores.Count < _limit - 1) {
                listScores.Add(newScore);
                listScores.Sort();
                SaveData<ScoreEntry>(listScores.ToArray(), _scoreKey);
                return;
            }
            listScores.Sort();
            if (listScores.LastElement().Score <= newScore.Score) {
                listScores.RemoveLast();
                listScores.Add(newScore);
                listScores.Sort();
                SaveData<ScoreEntry>(listScores.ToArray(), _scoreKey);
            }
        }

        public static ScoreEntry[] RetrieveScores() {
            return GetDataArray<ScoreEntry>(_scoreKey);
        }

        public static void SaveData<T>(object[] items, string key) where T : class {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, items as T[]);
            PlayerPrefs.SetString(key, Convert.ToBase64String(ms.GetBuffer()));
        }

        public static T[] GetDataArray<T>(string key) where T : class {
            if (PlayerPrefs.HasKey(key) == false) {
                return null;
            }
            string str = PlayerPrefs.GetString(key);
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(str));
            return bf.Deserialize(ms) as T[];
        }
    }

    [Serializable]
    public class ScoreEntry : IComparable<ScoreEntry> {
        public string Date;
        public string KilledBy;
        public string Class;
        public int LevelReached;
        public int Score;

        public int CompareTo(ScoreEntry otherScore) {
            if (otherScore== null) {
                return 1;
            }
            if (Score > otherScore.Score) {
                return -1;
            }
            if (Score == otherScore.Score) {
                return 0;
            }
            return 1;
        }
    }
}