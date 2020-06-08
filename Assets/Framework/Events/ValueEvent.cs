using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {
    public enum ValueEventType {
        None = 0,
        CheckValue = 1,
        AddValue = 2,
        ReduceValue = 3,
        SetValue = 4,
        BroadcastValue = 5,
    }

    public enum OpenMenuType {
        Merchant,
        Party,
        Class,
    }
    
    [System.Serializable]
    public class ValueEvent {
        public ValueEventType Type = ValueEventType.None;
        public int ValueType;
        public StringVariable StringValue;
        public int EnumIndex;
        public int IntValue;
        public string AdditionalString;
        public Object ObjectValue;

        public void Copy(ValueEvent source) {
            Type = source.Type;
            //ValueType = source.ValueType;
            StringValue = source.StringValue;
            EnumIndex = source.EnumIndex;
            IntValue = source.IntValue;
            AdditionalString = source.AdditionalString;
        }

        public bool PassCheck() {
            //if (ValueType.IsStat()) {
            //    var stat = GetStat();
            //    if (stat != null) {
            //        return stat.Value >= IntValue;
            //    }
            //}
            //switch (ValueType) {
            //    case GenericValue.Experience:
            //        return Player.SelectedActor.ClassController.Current.XpStat.TotalXp.Value >= IntValue;
            //    case GenericValue.Money:
            //        return Player.Currency.Value >= IntValue;
            //    case GenericValue.Spells:
            //        return Player.SelectedActor.SpellInventory.HasItem(ObjectValue as AbilityTemplate);
            //    case GenericValue.Items:
            //        return Player.MainInventory.HasItem(ItemDatabase.GetItem(StringValue));
            //    case GenericValue.Quest:
            //        var quest = QuestController.GetEntry(ObjectValue as QuestTemplate);
            //        return quest != null && quest.Status == (QuestStatus) IntValue;
            //    case GenericValue.String:
            //        var stringData = Game.GetDataString(StringValue);
            //        return stringData != null && stringData == AdditionalString;
            //    case GenericValue.Int:
            //        return Game.GetDataInt(StringValue) == IntValue;
            //}
            return true;
        }

        public void ModifyValue() {
            //if (ValueType.IsStat()) {
            //    var stat = GetStat();
            //    if (stat != null) {
            //        switch (Type) {
            //            case ValueEventType.AddValue:
            //                stat.AddToBase(IntValue);
            //                Player.SelectedActor.ShowFloatingText(string.Format("+{0} {1}", IntValue, stat.Label), Color.green);
            //                break;
            //            case ValueEventType.ReduceValue:
            //                Player.SelectedActor.ShowFloatingText(string.Format("-{0} {1}", IntValue, stat.Label), Color.red);
            //                stat.AddToBase(-IntValue);
            //                break;
            //            case ValueEventType.SetValue:
            //                Player.SelectedActor.ShowFloatingText(string.Format("{1} = {0}", IntValue, stat.Label), Color.yellow);
            //                stat.ChangeBase(IntValue);
            //                break;
            //        }
            //    }
            //    return;
            //}
            //switch (ValueType) {
            //    case GenericValue.Experience:
            //        switch (Type) {
            //            case ValueEventType.AddValue:
            //                PlayerControllerSystem.Current.AddExperience(IntValue);
            //                UIFloatingText.SpawnCentered(string.Format("+{0} XP", IntValue), Color.green);
            //                break;
            //            case ValueEventType.ReduceValue:
            //                PlayerControllerSystem.Current.AddExperience(-IntValue);
            //                UIFloatingText.SpawnCentered(string.Format("-{0} XP", IntValue), Color.red);
            //                break;
            //            case ValueEventType.SetValue:
            //                PlayerControllerSystem.Current.SetExperience(IntValue);
            //                UIFloatingText.SpawnCentered(string.Format("XP = {0}", IntValue), Color.yellow);
            //                break;
            //        }
            //        break;
            //    case GenericValue.Money:
            //        switch (Type) {
            //            case ValueEventType.AddValue:
            //                Player.Currency.AddToValue(IntValue);
            //                UIFloatingText.SpawnCentered(string.Format("+{0} {1}", IntValue, GameLabels.Currency), Color.green);
            //                break;
            //            case ValueEventType.ReduceValue:
            //                UIFloatingText.SpawnCentered(string.Format("-{0} {1]", IntValue, GameLabels.Currency), Color.red);
            //                Player.Currency.AddToValue(-IntValue);
            //                break;
            //            case ValueEventType.SetValue:
            //                UIFloatingText.SpawnCentered(string.Format("{1} = {0}", IntValue, GameLabels.Currency), Color.yellow);
            //                Player.Currency.ChangeValue(IntValue);
            //                break;
            //        }
            //        break;
            //    case GenericValue.Spells:
            //        var spell = SpellDatabase.GetSpell(AdditionalString);
            //        if (spell == null) {
            //            return;
            //        }
            //        switch (Type) {
            //            case ValueEventType.AddValue:
            //                Player.SelectedActor.ShowFloatingText(string.Format("Learned {0}", spell.Name), Color.green);
            //                Player.SelectedActor.SpellInventory.AddItem(spell);
            //                break;
            //            case ValueEventType.ReduceValue:
            //                Player.SelectedActor.ShowFloatingText(string.Format("Lost {0}", spell.Name), Color.red);
            //                Player.SelectedActor.SpellInventory.RemoveItem(spell);
            //                break;
            //            case ValueEventType.SetValue:
            //                Player.SelectedActor.ShowFloatingText(string.Format("Learned {0}", spell.Name), Color.green);
            //                Player.SelectedActor.SpellInventory.AddItem(spell);
            //                break;
            //        }
            //        break;
            //    case GenericValue.Items:
            //        var item = ItemDatabase.Instance.GetData(AdditionalString);
            //        if (item == null) {
            //            return;
            //        }
            //        int level = 1;
            //        int.TryParse(AdditionalString, out level);
            //        switch (Type) {
            //            case ValueEventType.AddValue:
            //                UIFloatingText.SpawnCentered(string.Format("Gained {0}", item.Name), Color.green);
            //                Player.MainInventory.AddItem(item.New(level, null, null));
            //                break;
            //            case ValueEventType.ReduceValue:
            //                UIFloatingText.SpawnCentered(string.Format("Lost {0}", item.Name), Color.red);
            //                Player.MainInventory.RemoveItem(item.Id);
            //                break;
            //            case ValueEventType.SetValue:
            //                UIFloatingText.SpawnCentered(string.Format("Gained {0}", item.Name), Color.green);
            //                Player.MainInventory.AddItem(item.New(level, null, null));
            //                break;
            //        }
            //        break;
            //    case GenericValue.String:
            //        switch (Type) {
            //            case ValueEventType.AddValue:
            //            case ValueEventType.SetValue:
            //                Game.SetDataString(StringValue, AdditionalString);
            //                break;
            //            case ValueEventType.ReduceValue:
            //                Game.SetDataString(StringValue, "");
            //                break;
            //        }
            //        break;
            //    case GenericValue.Int:
            //        switch (Type) {
            //            case ValueEventType.AddValue:
            //                Game.AddToData(StringValue, IntValue);
            //                break;
            //            case ValueEventType.SetValue:
            //                Game.SetDataInt(StringValue, IntValue);
            //                break;
            //            case ValueEventType.ReduceValue:
            //                Game.AddToData(StringValue, -IntValue);
            //                break;
            //        }
            //        break;
            //}
        }


#if UNITY_EDITOR
        public void DisplayConfigInspector(Rect rect, GUIStyle style) {
            var typeRect = new Rect(rect.x, rect.y, rect.width / 2, rect.height);
            var compareRect = new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);
            Type = (ValueEventType)EditorGUI.EnumPopup(typeRect, Type, style);
            //ValueType = (GenericValue)EditorGUI.EnumPopup(compareRect, ValueType, style);
        }

        //private const float MidWidth = 22;

        public void DisplayValueInspector(Rect rect, GUIStyle style, GUIStyle midStyle) {
            //if (Type == ValueEventType.None) {
            //    return;
            //}
            //var width = (rect.width / 2) - MidWidth;
            //var typeRect = new Rect(rect.x, rect.y, width, rect.height);
            //var compareRect = new Rect(rect.x + typeRect.width + MidWidth, rect.y, width, rect.height);
            //var midRect = new Rect(typeRect.x + typeRect.width+2, rect.y, MidWidth, rect.height);
            //string valueLabel = "";
            //switch (Type) {
            //    case ValueEventType.CheckValue:
            //        valueLabel = "==";
            //        break;
            //    case ValueEventType.SetValue:
            //        valueLabel = "=";
            //        break;
            //    case ValueEventType.AddValue:
            //        valueLabel = "+";
            //        break;
            //    case ValueEventType.ReduceValue:
            //        valueLabel = "-";
            //        break;
            //}
            //if (ValueType == GenericValue.Int) {
            //    StringValue = (StringVariable) EditorGUI.ObjectField(typeRect, StringValue, typeof(StringVariable), false);
            //    EditorGUI.LabelField(midRect, valueLabel, midStyle);
            //    IntValue = EditorGUI.IntField(compareRect, IntValue, style);
            //}
            //else if (ValueType.IsString()) {
            //    StringValue = (StringVariable) EditorGUI.ObjectField(typeRect, StringValue, typeof(StringVariable), false);
            //    EditorGUI.LabelField(midRect, valueLabel, midStyle);
            //    AdditionalString = EditorGUI.TextField(compareRect, AdditionalString, style);
            //}
            //else if (ValueType.IsObject()) {
            //    System.Type type = typeof(UnityEngine.Object);
            //    switch (ValueType) {
            //        case GenericValue.Quest:
            //            type = typeof(QuestTemplate);
            //            break;
            //        case GenericValue.Items:
            //            type = typeof(Entity);
            //            break;
            //        case GenericValue.Spells:
            //            type = typeof(Spell);
            //            break;
            //    }
            //    ObjectValue = EditorGUI.ObjectField(typeRect, ObjectValue, type, false);
            //    EditorGUI.LabelField(midRect, valueLabel, midStyle);
            //    switch (ValueType) {
            //        case GenericValue.Quest:
            //            IntValue = (int) (QuestStatus) EditorGUI.EnumPopup(compareRect, (QuestStatus) IntValue, style);
            //            break;
            //        case GenericValue.Items:
            //        case GenericValue.Spells:
            //            IntValue = EditorGUI.IntField(compareRect, IntValue, style);
            //            break;
            //    }
            //}
            //else {
            //    if (ValueType.NeedLabel()) {
            //        EnumIndex = IntDisplay(ValueType, "", EnumIndex, typeRect, style);
            //    }
            //    EditorGUI.LabelField(midRect, valueLabel, midStyle);
            //    IntValue = EditorGUI.IntField(compareRect,  IntValue, style);
            //}
        }

        public void DisplayChoiceInspector(Rect rect, GUIStyle style, GUIStyle midStyle) {
            var typeRect = new Rect(rect.x, rect.y, rect.width/3, rect.height);
            //ValueType = (GenericValue)EditorGUI.EnumPopup(typeRect, ValueType, style);
            var valueRect = new Rect(rect.x + typeRect.width, rect.y, rect.width-typeRect.width, rect.height);
            DisplayValueInspector(valueRect, style, midStyle);
        }

        //public static int IntDisplay(GenericValue valueType, string valueLabel, int statIndex, Rect rect, GUIStyle style) {
        //    switch (valueType) {
        //        case GenericValue.Attributes:
        //            return (int) (Attributes) EditorGUI.EnumPopup(rect, valueLabel, (Attributes) statIndex, style);
        //        case GenericValue.DamageSource:
        //            return (int) (ActionSource) EditorGUI.EnumPopup(rect, valueLabel, (ActionSource) statIndex);
        //        case GenericValue.DamageTypes:
        //            return (int) (DamageTypes) EditorGUI.EnumPopup(rect, valueLabel, (DamageTypes) statIndex);
        //        case GenericValue.DefenseStats:
        //            return (int) (DefensiveStats.List) EditorGUI.EnumPopup(rect, valueLabel, (DefensiveStats.List) statIndex);
        //        case GenericValue.ModifyValue:
        //            return (int) (ModifyValue) EditorGUI.EnumPopup(rect, valueLabel, (ModifyValue) statIndex);
        //        case GenericValue.OffenseStats:
        //            return (int) (OffensiveStats.List) EditorGUI.EnumPopup(rect, valueLabel, (OffensiveStats.List) statIndex);
        //        case GenericValue.Skills:
        //            return (int) (Skills) EditorGUI.EnumPopup(rect, valueLabel, (Skills) statIndex);
        //        case GenericValue.Vitals:
        //        case GenericValue.VitalStats:
        //            return (int) (Vitals) EditorGUI.EnumPopup(rect, valueLabel, (Vitals) statIndex);
        //        case GenericValue.Menu:
        //            return (int)(OpenMenuType)EditorGUI.EnumPopup(rect, valueLabel, (OpenMenuType)statIndex);

        //    }
        //    return EditorGUI.IntField(rect,valueLabel, statIndex);
        //}
#endif
        }
}
