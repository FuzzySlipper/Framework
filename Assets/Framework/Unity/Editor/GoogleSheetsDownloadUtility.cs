using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class GoogleSheetsDownloadUtility {

        public static void Download(System.Action<string, char> del, string docId, string sheetId, bool saveAsset = false, string assetName = null) {
            TimeManager.StartUnscaled(DownloadCsv(del, docId, sheetId, saveAsset, assetName));
        }

        private static IEnumerator DownloadCsv(System.Action<string, char> del, string docId, string sheetId, bool saveAsset = false, string assetName = null) {
            string url = "https://docs.google.com/spreadsheets/d/" + docId + "/export?format=csv";

            if (!string.IsNullOrEmpty(sheetId)) {
                url += "&gid=" + sheetId;
            }

            WWWForm form = new WWWForm();
            WWW download = new WWW(url, form);

            yield return download;

            if (!string.IsNullOrEmpty(download.error)) {
                Debug.Log("Error downloading: " + download.error);
            }
            else {
                del(download.text, ',');
                if (saveAsset) {
                    if (!string.IsNullOrEmpty(assetName))
                        System.IO.File.WriteAllText("Assets/Resources/" + assetName + ".csv", download.text);
                    else {
                        throw new System.Exception("assetName is null");
                    }
                }
            }
        }

    }
}
