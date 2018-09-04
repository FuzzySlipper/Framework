using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PixelComrades {
    public static class FileUtility {

        public static string ReadFile(string path) {
            if (!File.Exists(path)) {
                Debug.LogError("File path does not exist!\n" + path);
                return "";
            }
            var contents = File.ReadAllText(path);
            return contents;
        }

        public static bool SaveFile(string path, string contents) {
            try {
                File.WriteAllText(path, contents);
            }
            catch (Exception e) {
                Debug.LogError("Failed writing to path: " + path + "\n" + e);
                return false;
            }

            return true;
        }

        public static string GetUniqueName(string desiredName, string ext, string[] existingNames) {
            if (existingNames == null || existingNames.Length == 0) {
                return desiredName;
            }

            for (var i = 0; i < existingNames.Length; ++i) {
                existingNames[i] = existingNames[i].ToLower();
            }

            var existingNamesHS = new HashSet<string>(existingNames);
            if (string.IsNullOrEmpty(ext)) {
                if (!existingNamesHS.Contains(desiredName.ToLower())) {
                    return desiredName;
                }
            }
            else {
                if (!existingNamesHS.Contains(string.Format("{0}.{1}", desiredName.ToLower(), ext))) {
                    return desiredName;
                }
            }

            var parts = desiredName.Split(' ');
            var lastPart = parts[parts.Length - 1];
            int number;
            if (!int.TryParse(lastPart, out number)) {
                number = 1;
            }
            else {
                desiredName = desiredName.Substring(0, desiredName.Length - lastPart.Length).TrimEnd(' ');
            }

            const int maxAttempts = 10000;
            for (var i = 0; i < maxAttempts; ++i) {
                string uniqueName;
                if (string.IsNullOrEmpty(ext)) {
                    uniqueName = string.Format("{0} {1}", desiredName, number);
                }
                else {
                    uniqueName = string.Format("{0} {1}.{2}", desiredName, number, ext);
                }

                if (!existingNamesHS.Contains(uniqueName.ToLower())) {
                    return uniqueName;
                }

                number++;
            }

            if (string.IsNullOrEmpty(ext)) {
                return string.Format("{0} {1}", desiredName, Guid.NewGuid().ToString("N"));
            }

            return string.Format("{0} {1}.{2}", desiredName, Guid.NewGuid().ToString("N"), ext);
        }
    }
}
