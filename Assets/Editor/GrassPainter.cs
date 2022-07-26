using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class GrassPainter : EditorWindow
{
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneView;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneView;

    }
    private enum PaintMode {
        COLOR,
        DENSITY,
        HEIGHT,
        FREEROAM
    }
    static GameObject target;
    static GrassPointScatter targetScatter;
    static float brushSize = 4.0f;
    static float brushSoftness = 0.5f;
    static float brushStrength = 0.5f;
    static Texture targetColorInfo;
    static Texture targetHeightInfo;
    static PaintMode currentPaintMode = PaintMode.FREEROAM;
    static Color brushColor;

    static Shader paintBrushShader;
    static RenderTexture blitBuffer;
    static Material paintBrushMaterial;

    static int isCapturingControls = 1;
    [MenuItem("ˢ��/ˢ��")]
    public static void OpenEditorWindow()
    {
        EditorWindow.GetWindow<GrassPainter>(false, "ˢ�ݣ�");
        paintBrushShader = Shader.Find("Hidden/GrassPaint/PaintBrushShader");
   
        paintBrushMaterial = new Material(paintBrushShader);
    }


    private void OnGUI()
    {
        GUIStyle header = new GUIStyle();
        header.normal.textColor = Color.white;
        header.fontSize = 18; // whatever you set

        GUIStyle subheader = new GUIStyle();
        subheader.normal.textColor = Color.white;
        subheader.fontSize = 12; // whatever you set

        GUILayout.Label("  I. ѡ�����ˢ�ݵ���", header);
        if (!target)
        {
            
            GUILayout.Label("    ѡ����Ҫˢ�ݵ����壬Ȼ������\"ѡ��Ŀ����ˢ��\"");
        }
        if (GUILayout.Button("��ѡ��Ŀ����ˢ��") && Selection.activeGameObject != null)
        {
            target = Selection.activeGameObject;
            bool hasCreatedScatter = false;
            foreach (GrassPointScatter gps in GameObject.FindObjectsOfType<GrassPointScatter>())
            {
                //check if the active is the one, if none exists, create new:
                Debug.Log(gps.GetMatchedMesh());
                if (gps.GetMatchedMesh() == target)
                {
                    targetScatter = gps;
                    hasCreatedScatter = true;
                    break;
                }
            }
            if (!hasCreatedScatter)
            {
                //create scatter;
                GameObject empty = new GameObject();
                targetScatter = empty.AddComponent<GrassPointScatter>();
            }
            //add collider if none exists:
            if(target.GetComponent<Collider>() == null)
            {
                target.AddComponent<MeshCollider>();
            }
            targetColorInfo = targetScatter.GetColorInfoTexture();
            targetHeightInfo = targetScatter.GetHeightInfoTexture();
        }

        if (target)
        {
            GUILayout.Label("  II. ˢ����Ϣ����", header);
            GUILayout.Label("    ���֮ǰ�б������Ϣ������ѡ����Ϣͼ��ֱ�����롣");
            EditorGUILayout.ObjectField("�ݵ���ɫ��Ϣͼ", targetColorInfo, typeof(Texture2D), false); //rgb - color
            EditorGUILayout.ObjectField("�ݵظ߰�&������Ϣͼ", targetHeightInfo, typeof(Texture2D), false); //r - heightmap, g - amount, b - patch height
            GUILayout.Button("�����Զ���ݵ���Ϣ");
            GUILayout.Button("�������ɵĲݵ���Ϣ");

            GUILayout.Label("  III. ����ˢ����", header);
            GUILayout.Label("    ������ˢ��Ȼ��ʼˢ�ݰɣ�");
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Toggle(currentPaintMode == PaintMode.DENSITY, "ģʽ���ݵ�����", "Button"))
                currentPaintMode = PaintMode.DENSITY;
            if (GUILayout.Toggle(currentPaintMode == PaintMode.COLOR, "ģʽ���ݵ�Ⱦɫ", "Button"))
                currentPaintMode = PaintMode.COLOR;
            if (GUILayout.Toggle(currentPaintMode == PaintMode.HEIGHT, "ģʽ���ݵظ߰�", "Button"))
                currentPaintMode = PaintMode.HEIGHT;
            if (GUILayout.Toggle(currentPaintMode == PaintMode.FREEROAM, "ģʽ���ر�", "Button"))
                currentPaintMode = PaintMode.FREEROAM;
            EditorGUILayout.EndHorizontal();
            brushSize = EditorGUILayout.Slider("���ʴ�С([]������)", brushSize, 0, 100);
            brushSoftness = EditorGUILayout.Slider("����Ӳ��(Shift+[]������)", brushSoftness, 0, 1);
            brushStrength = EditorGUILayout.Slider("����ǿ��(+-������)", brushStrength, 0, 1);
            if(currentPaintMode != PaintMode.FREEROAM)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                GUIUtility.hotControl = controlId;
            }
            if (currentPaintMode == PaintMode.COLOR)
            {
                //color slider:
                brushColor = EditorGUILayout.ColorField("������ɫ", brushColor);
            }
           

        }

    }

    public void OnSceneView(SceneView sceneview)
    {
        /*


        //for (int i = 0; i < ((Path)target).nodes.Count; i++)
        //    ((Path)target).nodes[i] = Handles.PositionHandle(((Path)target).nodes[i], Quaternion.identity);

        //Handles.DrawPolyLine(((Path)target).nodes.ToArray());

        */
        Event e = Event.current;
        HandleBrushTweak(e);
        if (!target) return;



        Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hitInfo;

        //Debug.DrawRay(worldRay.origin, worldRay.direction * 50);
        
        if (Physics.Raycast(worldRay, out hitInfo, float.MaxValue))
        {
            //Debug.Log("hit");
            
            if(hitInfo.collider.gameObject == target)
            {
                Handles.DrawWireDisc(hitInfo.point, hitInfo.normal, brushSize);
                Handles.DrawWireDisc(hitInfo.point, hitInfo.normal, Mathf.Clamp(brushSoftness, 0.01f, 0.99f) * brushSize);
                Handles.color = Color.white;
                if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
                {
                    if (e.button == 0)
                    {
                        int controlId = GUIUtility.GetControlID(FocusType.Passive);
                        GUIUtility.hotControl = controlId;
                        e.Use();
                        //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        HandleDrawing(hitInfo.point, e.shift?-1.0f:1.0f);
                        
                    }
                }
            }
        }

        SceneView.RepaintAll();
        Repaint();
        //if (GUI.changed)
            //EditorUtility.SetDirty(target);

    }



    void HandleDrawing(Vector3 worldPos, float reverseInfo)
    {

        //Compute UV Coordinates:
        if (!paintBrushMaterial)
        {
            paintBrushShader = Shader.Find("Hidden/GrassPaint/PaintBrushShader");
            paintBrushMaterial = new Material(paintBrushShader);
        }
        Vector2 uvCoords = targetScatter.ConvertToUVSpace(worldPos);
        float minX, maxX, minZ, maxZ;
        targetScatter.GetGrassBounds(out minX, out maxX, out minZ, out maxZ);
        //compute aspect ratio:
        float widthHeightRatio = (maxX - minX) / (maxZ - minZ); //Crucial key to correct aspect
        Vector4 mouseInfoBundle = new Vector4(uvCoords.x, uvCoords.y, widthHeightRatio, 0.0f);
        float scaledBrushSize = brushSize / (maxZ - minZ);
        Vector4 brushSettingsBundle = new Vector4(scaledBrushSize, brushSoftness, brushStrength, reverseInfo);

        paintBrushMaterial.SetVector("_MouseInfo", mouseInfoBundle);
        paintBrushMaterial.SetVector("_BrushSettings", brushSettingsBundle);

        //argetHeightInfo.
        blitBuffer = RenderTexture.GetTemporary(1024, 1024, 0, RenderTextureFormat.ARGB32);//new Texture2D(1024, 1024, TextureFormat.ARGB32, false, true);
        Graphics.Blit(targetHeightInfo, blitBuffer, paintBrushMaterial);
        Graphics.CopyTexture(blitBuffer, targetHeightInfo);
        //blitBuffer.Release();
    }

    void HandleBrushTweak(Event e)
    {
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.LeftBracket:
                    if (e.shift)
                    {
                        //softness
                        brushSoftness -= 0.05f;
                        brushSoftness = Mathf.Max(0.0f, brushSoftness);
                    }
                    else
                    {
                        brushSize -= 1.0f;
                        brushSize = Mathf.Max(1.0f, brushSize);
                    }
                    break;
                case KeyCode.RightBracket:
                    if (e.shift)
                    {
                        brushSoftness += 0.05f;
                        brushSoftness = Mathf.Min(1.0f, brushSoftness);
                    }
                    else
                    {
                        brushSize += 1.0f;
                        brushSize = Mathf.Min(100.0f, brushSize);
                    }

                    break;
                case KeyCode.Equals:
                    //fall thru
                    goto case KeyCode.KeypadPlus;
                case KeyCode.KeypadPlus:
                    brushStrength += 0.05f;
                    brushStrength = Mathf.Min(brushStrength, 1.0f);
                    break;
                case KeyCode.Minus:
                    //fall thru
                    goto case KeyCode.KeypadMinus;
                case KeyCode.KeypadMinus:
                    brushStrength -= 0.05f;
                    brushStrength = Mathf.Max(brushStrength, 0.0f);
                    break;
                case KeyCode.Escape:
                    HandleUtility.Repaint();
                    currentPaintMode = PaintMode.FREEROAM;
                    break;
                default:
                    break;
            }
        }
    }

}
