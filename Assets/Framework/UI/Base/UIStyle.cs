using UnityEngine;
using System.Collections;
using TMPro;

namespace PixelComrades {
    public class UIStyle : MonoSingleton<UIStyle> {

        public enum ColorConfig {
            Damage,
            StandardLog,
            InterfaceDark,
            FloatStandard,
            InterfaceNormal,
            InterfaceNormalOffColor,
            None = 99,
        }

        public enum TextConfig {
            Standard,
            Header,
            None = 99,
        }

        [SerializeField] private Color32 _damageColor = Color.red;
        [SerializeField] private Color32 _standardLogColor = Color.green;
        [SerializeField] private Color32 _floatStandard = Color.green;
        [SerializeField] private Color32 _interfaceStandard = Color.green;
        [SerializeField] private Color32 _interfaceStandardOffColor = Color.green;
        [SerializeField] private Color32 _interfaceDark = Color.green;
        [SerializeField] private TMP_FontAsset _fontStandard = null;
        [SerializeField] private TMP_FontAsset _fontHeader = null;

        public static Color32 Damage { get { return main._damageColor; } }
        public static Color32 LogStandard { get { return main._standardLogColor; } }
        public static Color32 InterfaceDark { get { return main._interfaceDark; } }
        public static Color32 FloatStandard { get { return main._floatStandard; } }
        public static Color32 InterfaceNormal { get { return main._interfaceStandard; } }
        public static Color32 InterfaceNormalOffColor { get { return main._interfaceStandardOffColor; } }
        public static TMP_FontAsset FontStandard { get { return main._fontStandard; } }
        public static TMP_FontAsset FontHeader { get { return main._fontHeader; } }

        public static Color32 Get(ColorConfig colorConfig) {
            switch (colorConfig) {
                default:
                case ColorConfig.InterfaceDark:
                    return InterfaceDark;
                case ColorConfig.Damage:
                    return Damage;
                case ColorConfig.StandardLog:
                    return LogStandard;
                case ColorConfig.FloatStandard:
                    return FloatStandard;
                case ColorConfig.InterfaceNormal:
                    return InterfaceNormal;
                case ColorConfig.InterfaceNormalOffColor:
                    return InterfaceNormalOffColor;
            }
        }

        public static TMP_FontAsset Get(TextConfig colorConfig) {
            switch (colorConfig) {
                default:
                case TextConfig.Standard:
                    return FontStandard;
                case TextConfig.Header:
                    return FontHeader;
            }
        }
    }
}   