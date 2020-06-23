using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IGenericImporter {
        void SetDatabase(GenericDatabase db);
        void ProcessImport(List<string> entry);
    }
}
