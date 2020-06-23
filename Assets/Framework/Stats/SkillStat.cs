using UnityEngine;
using System.Collections;

namespace PixelComrades {
    [System.Serializable]
    public class SkillStat : BaseStat {
        private const float TrainAmountPerUseBase = 1f;
        private const float TrainLevelUp = 100;
        private const float TrainIncreaseStat = 1;
        private const float BuyTrainAmount = 5;

        private float _trainAmt = 0;
        private float _trainMulti = 1;
        private float _increaseRank = 0;
        private float _maxTrain = 0;
        private int _maxRank = 0;
        private int _currentRank = 0;
        private DataEntry _data;

        public SkillStat(int owner, DataEntry data, string label, string id, float baseValue) : base(owner, label, id, baseValue) {
            _data = data;
            SkillDescription = data.GetValue<string>(DatabaseFields.Description);
            var list = data.Get<DataList>("Ranks");
            if (list == null) {
                RankDescription = "Error no ranks list";
                return;
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < list.Count; i++) {
                var line = list[i];
                sb.AppendNewLine("<b>");
                sb.Append(line.TryGetValue("Rank", "None"));
                sb.AppendNewLine("</b> ");
                sb.AppendNewLine(line.GetValue<string>(DatabaseFields.Description));
            }
            RankDescription = sb.ToString();
        }

        public virtual int Category { get { return 0; } }
        public int MaxRank { get { return _maxRank; } }
        public int CurrentRank {  get { return _currentRank; } }
        public bool CanIncreaseRank { get { return (int) _currentRank < (int) _maxRank && BaseValue >= _increaseRank - 1; } }
        public bool CanBuyTraining { get { return BaseValue < _maxTrain; } }
        public string SkillDescription { get; private set; }
        public string RankDescription { get; private set; }
        


        public void SetMaxRank(int rank) {
            _maxRank = rank;
            _trainMulti = RpgSettings.GetTrainingScaling(rank);
            _maxTrain = RpgSettings.GetMaxStat(_maxRank);
        }

        public virtual void SetCurrentRank(int rank) {
            _currentRank = rank;
            _increaseRank = RpgSettings.GetMaxStat(_currentRank);
            StatChanged();
        }

        //public bool TryIncreaseRank() {
        //    if (!CanIncreaseRank) {
        //        return false;
        //    }
        //    SetCurrentRank((SkillRank) (((int) _currentRank)+1));
        //    StatChanged();
        //    if (_baseValue <= 0) {
        //        ChangeBase(1);
        //    }
        //    return true;
        //}

        public bool TryBuyTrainStat() {
            if (!CanBuyTraining) {
                return false;
            }
            BaseValue = Mathf.Clamp(BaseValue + BuyTrainAmount, 0, _increaseRank);
            _trainAmt = 0;
            if (CanIncreaseRank) {
                SetCurrentRank(_currentRank + 1);
            }
            StatChanged();
            return true;
        }

        public void TrainStatOnUse() {
            _trainAmt += TrainAmountPerUseBase * _trainMulti;
            if (_trainAmt >= TrainLevelUp && BaseValue < _increaseRank) {
                AddToBase(TrainIncreaseStat);
                _trainAmt = 0;
            }
            if (CanIncreaseRank) {
                SetCurrentRank(_currentRank + 1);
                StatChanged();
            }
        }

        protected string UpdateDerivedMod(string mod, float value, BaseStat stat) {
            if (string.IsNullOrEmpty(mod)) {
                mod = AddDerivedStat(value, stat);
            }
            else {
                UpdateDerivedStat(mod, value);
            }
            return mod;
        }
    }
}