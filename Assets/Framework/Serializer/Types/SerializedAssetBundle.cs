using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class SerializedAssetBundle : ISerializable {

        /// The name of an asset bundle (as set in the Unity Editor).
        public string AssetBundleName;
        /// The path to the asset within the AssetBundle.  Usually this will be set by the CreateAssetBundles 
        /// editor tool.
        public string FilePath;

        /**
         * Constructor
         * @param InAssetBundleName Name of the asset bundle (should be name only - use with pb_Config AssetBundle_SearchDirectories).
         * @param InFilePath The path to the asset within the bundle.
         */
        public SerializedAssetBundle(string inAssetBundleName, string inFilePath) {
            AssetBundleName = inAssetBundleName;
            FilePath = inFilePath;
        }

        /**
         * Serialization constructor.
         */
        public SerializedAssetBundle(SerializationInfo info, StreamingContext context) {
            AssetBundleName = (string)info.GetValue("AssetBundleName", typeof(string));
            FilePath = (string)info.GetValue("FilePath", typeof(string));
        }

        /**
         * Serialization override.
         */
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("AssetBundleName", AssetBundleName, typeof(string));
            info.AddValue("FilePath", FilePath, typeof(string));
        }

        /**
         * Returns a nicely formatted summary of this bundle information.
         */
        public override string ToString() {
            return "Bundle: " + AssetBundleName + "\nPath: " + FilePath;
        }
    }
}
