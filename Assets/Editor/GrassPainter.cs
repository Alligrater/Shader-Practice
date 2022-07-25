using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class GrassPainter : EditorWindow
{
    static GameObject target;
    static GrassPointScatter targetScatter;
    [MenuItem("ˢ��/ˢ��")]
    public static void OpenEditorWindow()
    {
        EditorWindow.GetWindow<GrassPainter>(false, "ˢ�ݣ�");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("��ѡ��Ŀ����ˢ��") && Selection.activeGameObject != null)
        {
            target = Selection.activeGameObject;
            bool hasCreatedScatter = false;
            foreach(GrassPointScatter gps in GameObject.FindObjectsOfType<GrassPointScatter>())
            {
                //check if the active is the one, if none exists, create new:
                if(gps.GetMatchedMesh() == target)
                {
                    targetScatter = gps;
                    hasCreatedScatter = true;
                    break;
                }
            }
            if (!hasCreatedScatter)
            {
                //create scatter;
            }
        }
        
    }
}
