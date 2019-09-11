using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

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

            //WWWForm form = new WWWForm();
            var download = new UnityWebRequest(url);
            //WWW download = new WWW(url, form);
            while (!download.isDone) {
                //if (!string.IsNullOrEmpty(download.error)) {
                //    break;
                //}
                yield return null;
            }
            //yield return download;
            if (!string.IsNullOrEmpty(download.error)) {
                Debug.Log("Error downloading: " + download.error);
            }
            else {
                del(download.downloadHandler.text, ',');
                if (saveAsset) {
                    if (!string.IsNullOrEmpty(assetName))
                        System.IO.File.WriteAllText("Assets/GameData/" + assetName + ".csv", download.downloadHandler.text);
                    else {
                        throw new System.Exception("assetName is null");
                    }
                }
            }
            download.Dispose();
        }

    }
}
