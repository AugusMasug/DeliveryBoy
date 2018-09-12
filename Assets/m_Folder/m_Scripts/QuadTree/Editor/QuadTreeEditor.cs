using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(CreateQuadTreeWithBounds))]
public class QuadTreeEditor : Editor {

    void OnSceneGUI()
    {

        if (EditorApplication.isPlaying)
        {
            //得到test脚本的对象
            CreateQuadTreeWithBounds test = (CreateQuadTreeWithBounds)target; 
            for (int i = 0; i < test.objects.Length; i++)
            {
                Handles.Label(test.objects[i].center + new Vector3(0.35f, 0, 0) - Vector3.right * 0.5f, (i + 1).ToString());
            }
        }

        

    }
}