using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelComrades {
    public static class StringUtilities {
        public static List<string>[] SplitStringWithLines(string totalString, char splitCharacter) {
            string[] lines = totalString.Split('\n');
            List<string>[] list = new List<string>[lines.Length];
            for (int i = 0; i < lines.Length; i++) {
                list[i] = SplitString(lines[i], splitCharacter);
            }
            return list;
        }

        public static List<string> SplitChildMultiEntry(string targetLine) {
            return SplitString(targetLine, StringConst.ChildMultiEntryBreak);
        }

        public static string EmptySplitString(int count) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++) {
                sb.AppendEntryBreak();
            }
            return sb.ToString();
        }

        public static List<string> SplitString(string targetLine, char splitChar) {
            if (string.IsNullOrEmpty(targetLine)) {
                return null;
            }
            List<string> output = new List<string>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < targetLine.Length; i++) {
                if (targetLine[i] == splitChar) {
                    output.Add(sb.ToString());
                    //sb.Length = 0;
                    sb = new StringBuilder();
                }
                else {
                    sb.Append(targetLine[i]);
                }
            }
            var finalString = sb.ToString();
            if (!string.IsNullOrEmpty(finalString)) {
                output.Add(finalString);
            }
            return output;
        }

        public static int ProcessStringLists(string text, char splitChar, int linesToSkip, System.Action<List<string>> del) {
            var lines = StringUtilities.SplitStringWithLines(text, splitChar);
            for (int i = linesToSkip; i < lines.Length; i++) {
                var entries = lines[i];
                del(entries);
            }
            return lines.Length - linesToSkip;
        }

        public static string EncodeData(this string strData, int dataCnt, int index, string value) {
            return EncodeNewData(strData, dataCnt, index, value);
        }

        public static string EncodeNewData(string strData, int dataCnt, int index, string value) {
            var data = string.IsNullOrEmpty(strData) ? new List<string>() : strData.SplitMultiEntry();
            if (data.Count != dataCnt || !data.HasIndex(index)) {
                var array = data.ToArray();
                System.Array.Resize(ref array, MathEx.Max(index + 1, dataCnt));
                array[index] = value;
                return array.EncodeWithEntryBreak();
            }
            data[index] = value;
            return data.EncodeWithEntryBreak();
        }
    }
}
