using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public interface IGridNode {
        Rect EditorWindowRect { get; set; }
        string DisplayName { get; set; }
        int ChildNodeCount { get; }
        IGridNode GetChildNode(int index);
    }

}
