using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public static class StringBuilderExtension {
        public static void TabAppend(this StringBuilder sb, string text) {
            sb.Append(text);
            sb.Append("\t");
        }

        public static void NewLineAppend(this StringBuilder sb, string text) {
            sb.Append(text);
            sb.Append(System.Environment.NewLine);
        }

        public static void NewLine(this StringBuilder sb) {
            sb.Append(System.Environment.NewLine);
        }

        public static void AppendNewLine(this StringBuilder sb, string text) {
            sb.Append(text);
            sb.Append(System.Environment.NewLine);
        }

        public static void AppendEntryBreak(this StringBuilder sb) {
            sb.Append(StringConst.MultiEntryBreak);
        }

        public static void AppendEntryBreak(this StringBuilder sb, string text) {
            sb.Append(text);
            sb.Append(StringConst.MultiEntryBreak);
        }

        public static void AppendChildEntryBreak(this StringBuilder sb) {
            sb.Append(StringConst.ChildMultiEntryBreak);
        }

        public static void AppendChildEntryBreak(this StringBuilder sb, string text) {
            sb.Append(text);
            sb.Append(StringConst.ChildMultiEntryBreak);
        }

        public static void AppendSeparator(this StringBuilder sb, string text, char separator) {
            sb.Append(text);
            sb.Append(separator);
        }

        public static void Headline(this StringBuilder sb, string text) {
            sb.Append("<size=175%><u><b>");
            sb.Append(text);
            sb.Append("</b></u><size=100%>");
            sb.Append(System.Environment.NewLine);
        }

        public static void BoldLabel(this StringBuilder sb, string text) {
            sb.Append("<size=125%><b>");
            sb.Append(text);
            sb.Append(":</b><size=100%> ");
        }

        public static string ToBoldOversized(this string text) {
            return string.Format("<size=125%><b>{0}</b><size=100%>", text);
        }

        public static string ToBoldOversizedLabel(this string text) {
            return string.Format("<size=125%><b>{0}:</b><size=100%> ", text);
        }

        public static string ToBoldLabel(this string text, string value) {
            return string.Format("<size=125%><b>{0}:</b><size=100%> {1}", text,value);
        }
    }
}
