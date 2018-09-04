using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/*
Extended Component Reodering by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
public class ExtendedComponentReodering  {
    [MenuItem("CONTEXT/Component/Move To Top")]
    private static void MoveToTop(MenuCommand menuCommand) {
        Component c = (Component)menuCommand.context;
        Component[] allComponents = c.GetComponents<Component>();
        int iOffset = 0;
        for(int i=0; i < allComponents.Length; i++)
        {
            if(allComponents[i] == c)
            {
                iOffset = i;
                break;
            }
        }
        for(int i =0; i < iOffset -1; i++)
        {
            UnityEditorInternal.ComponentUtility.MoveComponentUp(c);
        }
        EditorSceneManager.MarkAllScenesDirty();
    }

    [MenuItem("CONTEXT/Component/Move To Bottom")]
    private static void MoveToBottom(MenuCommand menuCommand) {
        Component c = (Component)menuCommand.context;
        Component[] allComponents = c.GetComponents<Component>();
        int iOffset = 0;
        for (int i = 0; i < allComponents.Length; i++)
        {
            if (allComponents[i] == c)
            {
                iOffset = i;
                break;
            }
        }
        for (; iOffset < allComponents.Length; iOffset++)
        {
            UnityEditorInternal.ComponentUtility.MoveComponentDown(c);
        }
        EditorSceneManager.MarkAllScenesDirty();
    }
}