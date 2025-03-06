using System;
using System.Collections.Generic;
using UnityEngine;
using Games.Util;

namespace Games.Manager
{
    [ExecuteInEditMode]
    public class DynamicLightManager : MonoBehaviour
    {
        private List<DynamicLight> dplList = new List<DynamicLight>(32);
        private Dictionary<DynamicLight, MeshRenderer> dplRendererDic = new Dictionary<DynamicLight, MeshRenderer>(32);

        [Header("裁剪距离")]
        public float cullDistance = 30;
        private float cullDistance2 = 900;
        private const int MAXLIGHT_PERRENDER = 10;
        private const int MAXRENDERER_PERLIGHT = 5;
        private static int DynamicPointLightNum = Shader.PropertyToID("DynamicPointLightNum");
        private static int DynamicPointPos = Shader.PropertyToID("DynamicPointPos");
        private static int DynamicPointColor = Shader.PropertyToID("DynamicPointColor");
        private static int DynamicPointDir = Shader.PropertyToID("DynamicPointDir");
        private static int DynamicPointRight = Shader.PropertyToID("DynamicPointRight");
        private static int DynamicCharaterUV = Shader.PropertyToID("DynamicCharaterUV");
        private static int DynamicFontTexture = Shader.PropertyToID("DynamicFontTexture");
        private static int DynamicLightTexture_1 = Shader.PropertyToID("DynamicLightTexture_1");

        private Vector4 tempVector4 = Vector4.zero;
        private List<Vector4> worldPosList = new List<Vector4>(100);
        private List<Vector4> colorList = new List<Vector4>(100);
        private List<Vector4> spotLightDir = new List<Vector4>(100);
        private List<Vector4> spotLightRight = new List<Vector4>(100);
        public List<Texture2D> textureList = new List<Texture2D>(4);
        [SerializeField]
        private Font _font;
        private List<Vector4> charaterUV = new List<Vector4>(100);
        private Texture fontTexture = null;

        private const int MAXDPL = 100;
        private const int MAXTEXTURE = 3;


        private float lightDistance = 5f;
        private float glowDistance = 5f;
        private float spotAngle = 30f;

        private bool lightShow = true;
        private static bool showDebug = false;
        public static void ShowDebug(bool show)
        {
            showDebug = show;
        }
        private bool globalMode = false;
        [Header("场景的尺寸大小")]
        public int sceneSize = 256;
        public List<RendererList> sceneGridData = new List<RendererList>();
        private Transform mainCameraTr = null;
#if GAMEDEBUG
        private List<GameObject> dplObjs = new List<GameObject>(8);
        #region ====test====
        private GameObject pointLightGO;
        private GameObject spotLightGO;
        private List<DynamicLight> pointLightList = new List<DynamicLight>();
        private List<DynamicLight> spotLightList = new List<DynamicLight>();
        private int pointLightNum = 0;
        private int spotLightNum = 0;
        private bool dynaimcLightOpen = true;
        private bool dragEnable = true;
        private bool showGlow = true;
        private bool forEnable = true;
        private float line0y = 100f;
        private float line1y = 150f;
        private float line2y = 200f;
        private void AddPointLight()
        {
            if(pointLightNum >= MAXDPL)
            {
                return;
            }
            pointLightNum += 1;

            for(int i = 0; i < pointLightList.Count; i++)
            {
                var dpl = pointLightList[i];
                if(!dpl.gameObject.activeInHierarchy)
                {
                    dpl.transform.parent.gameObject.SetActive(true);
                    return;
                }
            }
            
            if (null != pointLightGO)
            {
                var go = GameObject.Instantiate(pointLightGO);
                go.SetActive(true);
                var dpl = go.GetComponentInChildren<DynamicLight>();
                if (null != dpl)
                {
                    pointLightList.Add(dpl);
                }
                if(null != Obj_MainPlayer.Instance)
                {
                    //设置位置
                    Vector3 pos = new Vector3(Obj_MainPlayer.Instance.Position.x - 5 + pointLightList.Count % 10 * 1.5f, 
                                              Obj_MainPlayer.Instance.Position.y - 1f, 
                                              Obj_MainPlayer.Instance.Position.z - 5 + pointLightList.Count / 10 * 1.5f);
                    go.transform.parent = this.transform;
                    go.transform.position = pos;
                }
            }
        }
        private void MinusPointLight()
        {
            pointLightNum -= 1;
            pointLightNum = pointLightNum <= 0 ? 0 : pointLightNum;

            for (int i = 0; i < pointLightList.Count; i++)
            {
                var dpl = pointLightList[i];
                if (dpl.gameObject.activeInHierarchy)
                {
                    dpl.transform.parent.gameObject.SetActive(false);
                    return;
                }
            }
        }

        private void AddSpotLight()
        {
            if (spotLightNum >= MAXDPL)
            {
                return;
            }
            spotLightNum += 1;

            for (int i = 0; i < spotLightList.Count; i++)
            {
                var dpl = spotLightList[i];
                if (!dpl.gameObject.activeInHierarchy)
                {
                    dpl.transform.parent.gameObject.SetActive(true);
                    return;
                }
            }

            if (null != spotLightGO)
            {
                var go = GameObject.Instantiate(spotLightGO);
                go.SetActive(true);
                var dpl = go.GetComponentInChildren<DynamicLight>();
                if (null != dpl)
                {
                    spotLightList.Add(dpl);
                }

                if (null != Obj_MainPlayer.Instance)
                {
                    //设置位置
                    Vector3 pos = new Vector3(Obj_MainPlayer.Instance.Position.x - 5 + spotLightList.Count % 10 * 1.5f,
                                              Obj_MainPlayer.Instance.Position.y + 4f,
                                              Obj_MainPlayer.Instance.Position.z - 5 + spotLightList.Count / 10 * 1.5f);
                    go.transform.parent = this.transform;
                    go.transform.position = pos;
                }
            }

        }
        private void MinusSpotLight()
        {
            spotLightNum -= 1;
            spotLightNum = spotLightNum <= 0 ? 0 : spotLightNum;

            for (int i = 0; i < spotLightList.Count; i++)
            {
                var dpl = spotLightList[i];
                if (dpl.gameObject.activeInHierarchy)
                {
                    dpl.transform.parent.gameObject.SetActive(false);
                    return;
                }
            }
        }
        #endregion
#endif
        private static DynamicLightManager _instance = null;
        public static DynamicLightManager Instance
        {
            get { return _instance; }
        }
#if GAMEDEBUG
        private void OnGUI()
        {
            if(!showDebug)
            {
                return;
            }
            int renderLevel = LuaSettings.RenderLevel();
            if (GUI.Button(new Rect(Screen.width - 80, Screen.height / 2f - line1y + 50, 100, 50), "显隐:" + renderLevel))
            {
                lightShow = !lightShow;
                for (int i = 0; i < dplObjs.Count; i++)
                {
                    var dpl = dplObjs[i];
                    if(null != dpl)
                    {
                        dpl.transform.gameObject.SetActive(lightShow);
                    }
                }
            }
            if (!lightShow)
            {
                return;
            }
            GameObject camObj = GameObject.Find("Camera2");
            if (camObj != null)
            {
                UICamera uiCamera = camObj.GetComponent<UICamera>();
                if (uiCamera != null)
                {
                    uiCamera.eventReceiverMask = lightShow ? 0 : -1;
                }
            }

            if (GUI.Button(new Rect(Screen.width - 80, Screen.height / 2f - line1y, 100, 50), "动态:"))
            {
                var go = GameObject.Instantiate(spotLightGO);
                go.SetActive(true);
                if (null != Obj_MainPlayer.Instance)
                {
                    go.transform.parent = Obj_MainPlayer.Instance.GetPlayerRender().gameObject.transform;
                    go.transform.localPosition = new Vector3(0f, 4f, 0f);
                }
                var dpl = go.GetComponentInChildren<DynamicLight>();
                if (null != dpl)
                {
                    pointLightList.Add(dpl);
                    dpl.showType = DynamicLight.ShowType.FORCESHOW;
                    dpl.moveType = DynamicLight.MoveType.DYNAMIC;
                    dpl.UpdateData();
                }
            }

            //if (GUI.Button(new Rect(Screen.width - 80, Screen.height / 2f - line1y, 100, 50), "辉显:" + showGlow))
            //{
            //    showGlow = !showGlow;
            //    for (int i = 0; i < dplList.Count; i++)
            //    {
            //        dplList[i].gameObject.transform.GetChild(0).gameObject.SetActive(showGlow);
            //    }
            //}

            //if (GUI.Button(new Rect(Screen.width - 80, Screen.height / 2f - 40, 100, 50), "可拖拽:" + dragEnable))
            //{
            //    dragEnable = !dragEnable;
            //    if(dragEnable)
            //    {
            //        CameraDrag.GetInstance().OpenAutoCamera();
            //    }
            //    else
            //    {
            //        CameraDrag.GetInstance().StopAutoCamera();
            //    }
            //}
            //if (GUI.Button(new Rect(Screen.width - 160, Screen.height / 2f - 40, 100, 50), "用for:" + forEnable))
            //{
            //    forEnable = !forEnable;
            //    Shader.SetGlobalFloat("DynamicLightForOpen", forEnable ? 1 : 0);
            //}

            //if (GUI.Button(new Rect(Screen.width - 80, Screen.height / 2f - line2y, 100, 50), "全局本地:" + globalMode))
            //{
            //    globalMode = !globalMode;

            //    if(globalMode)
            //    {
            //        ClearMPB();
            //    }
            //    else
            //    {
            //        UpdateLocalLight();
            //    }
            //}
            //if (GUI.Button(new Rect(Screen.width - 80, Screen.height / 2f - line2y - 60, 100, 50), "只辉光" + !dynaimcLightOpen))
            //{
            //    dynaimcLightOpen = !dynaimcLightOpen;

            //    Shader.SetGlobalFloat("DynamicLightOpen", dynaimcLightOpen?1:0);
            //}

            ////if (GUI.Button(new Rect(Screen.width - 100, Screen.height/2f, 60, 50), "debug"))
            ////{
            ////    showDebug = !showDebug;
            ////}

            ////调点光源数量
            //if (GUI.Button(new Rect(Screen.width - 160, Screen.height / 2f - line1y, 60, 50), "+Point"))
            //{
            //    AddPointLight();
            //    UpdateLocalLight();
            //}
            //if (GUI.Button(new Rect(Screen.width - 160, Screen.height / 2f - line2y, 60, 50), "-Point"))
            //{
            //    MinusPointLight();
            //    UpdateLocalLight();
            //}
            //if (GUI.Button(new Rect(Screen.width - 240, Screen.height / 2f - line1y, 60, 50), "+Spot"))
            //{
            //    AddSpotLight();
            //    UpdateLocalLight();
            //}
            //if (GUI.Button(new Rect(Screen.width - 240, Screen.height / 2f - line2y, 60, 50), "-Spot"))
            //{
            //    MinusSpotLight();
            //    UpdateLocalLight();
            //}
            ////if (GUI.Button(new Rect(Screen.width / 2f - 100, Screen.height - 600, 60, 50), "+5 light"))
            ////{
            ////    for(int i = 0; i < dplObjs.Count; i++)
            ////    {
            ////        var obj = dplObjs[i].gameObject;
            ////        if(!obj.activeInHierarchy)
            ////        {
            ////            obj.SetActive(true);
            ////            return;
            ////        }
            ////    }
            ////}
            ////if (GUI.Button(new Rect(Screen.width / 2f - 100, Screen.height - 500, 60, 50), "-5 light"))
            ////{
            ////    for (int i = dplObjs.Count - 1; i >= 0; i--)
            ////    {
            ////        var obj = dplObjs[i].gameObject;
            ////        if (obj.activeInHierarchy)
            ////        {
            ////            obj.SetActive(false);
            ////            return;
            ////        }
            ////    }
            ////}
            ////int lightNum = 0;
            ////for (int i = 0; i < dplObjs.Count; i++)
            ////{
            ////    var obj = dplObjs[i].gameObject;
            ////    if (obj.activeInHierarchy)
            ////    {
            ////        lightNum += 5;
            ////    }
            ////}
            //GUI.Label(new Rect(Screen.width - 160, Screen.height / 2f - line0y, 60, 50), "PointNum:" + pointLightNum);
            //GUI.Label(new Rect(Screen.width - 240, Screen.height / 2f - line0y, 60, 50), "SpotNum:" + spotLightNum);

            ////调影响距离
            //if (GUI.Button(new Rect(Screen.width - 320, Screen.height / 2f - line1y, 60, 50), "+lightDis"))
            //{
            //    lightDistance++;
            //    UpdateLocalLight();
            //}
            //if (GUI.Button(new Rect(Screen.width - 320, Screen.height / 2f - line2y, 60, 50), "-lightDis"))
            //{
            //    lightDistance--;
            //    UpdateLocalLight();
            //}
            //GUI.Label(new Rect(Screen.width - 320, Screen.height / 2f - line0y, 60, 50), "Distance:" + lightDistance);

            ////调辉光长度
            //if (GUI.Button(new Rect(Screen.width - 400, Screen.height / 2f - line1y, 60, 50), "+glowDis"))
            //{
            //    glowDistance++;
            //    for (int i = 0; i < dplList.Count; i++)
            //    {
            //        dplList[i].glowDistance = glowDistance;
            //    }
            //    for (int i = 0; i < spotLightList.Count; i++)
            //    {
            //        var dpl = spotLightList[i];
            //        if (null != dpl)
            //        {
            //            dpl.transform.position = new Vector3(dpl.transform.position.x, dpl.transform.position.y + 1, dpl.transform.position.z);
            //        }
            //    }
            //}
            //if (GUI.Button(new Rect(Screen.width - 400, Screen.height / 2f - line2y, 60, 50), "-glowDis"))
            //{
            //    glowDistance--;
            //    for (int i = 0; i < dplList.Count; i++)
            //    {
            //        dplList[i].glowDistance = glowDistance;
            //    }
            //    for (int i = 0; i < spotLightList.Count; i++)
            //    {
            //        var dpl = spotLightList[i];
            //        if (null != dpl)
            //        {
            //            dpl.transform.position = new Vector3(dpl.transform.position.x, dpl.transform.position.y - 1, dpl.transform.position.z);
            //        }
            //    }
            //}
            //GUI.Label(new Rect(Screen.width - 400, Screen.height / 2f - line0y, 60, 50), "GlowDis:" + glowDistance);

            ////调spotAngle
            //if (GUI.Button(new Rect(Screen.width - 480, Screen.height / 2f - line1y, 60, 50), "+Angle"))
            //{
            //    spotAngle++;
            //    for (int i = 0; i < dplList.Count; i++)
            //    {
            //        dplList[i].spotAngle = spotAngle;
            //    }
            //}
            //if (GUI.Button(new Rect(Screen.width - 480, Screen.height / 2f - line2y, 60, 50), "-Angle"))
            //{
            //    spotAngle--;
            //    for (int i = 0; i < dplList.Count; i++)
            //    {
            //        dplList[i].spotAngle = spotAngle;
            //    }
            //}
            //GUI.Label(new Rect(Screen.width - 480, Screen.height / 2f - line0y, 60, 50), "Angle:" + spotAngle);

            //if (GUI.Button(new Rect(Screen.width - 560, Screen.height / 2f - line1y, 60, 50), "Atlas"))
            //{
            //    for (int i = 0; i < dplList.Count; i++)
            //    {
            //        dplList[i].textureIndex = 3;
            //        dplList[i].aniType = DynamicLight.AniType.ATLAS;
            //    }
            //}
            //if (GUI.Button(new Rect(Screen.width - 560, Screen.height / 2f - line2y, 60, 50), "Font"))
            //{
            //    for (int i = 0; i < dplList.Count; i++)
            //    {
            //        dplList[i].textureIndex = 4;
            //        dplList[i].aniType = DynamicLight.AniType.DYNAMICFONT;
            //    }
            //}

            //调强度
            //if (GUI.Button(new Rect(Screen.width / 2f, Screen.height - 400, 60, 50), "+1 强度"))
            //{
            //    lightIntensity++;
            //    var dpls = dplObjs[0].GetComponentsInChildren<DynamicLight>();
            //    for (int i = 0; i < dpls.Length; i++)
            //    {
            //        dpls[i].lightIntensity = lightIntensity;
            //    }
            //}
            //if (GUI.Button(new Rect(Screen.width / 2f, Screen.height - 300, 60, 50), "-1 强度"))
            //{
            //    lightIntensity--;
            //    var dpls = dplObjs[0].GetComponentsInChildren<DynamicLight>();
            //    for (int i = 0; i < dpls.Length; i++)
            //    {
            //        dpls[i].lightIntensity = lightIntensity;
            //    }
            //}
        }

#endif

        private void OnValidate()
        {
            UpdateTextureList();
            UpdateFontTexture();
        }

        private void Awake()
        {
            //一开始，实际的数量设定为0.但是数据数组直接搞到最大，这是个bug
            worldPosList.Clear();
            colorList.Clear();
            spotLightDir.Clear();
            spotLightRight.Clear();
            charaterUV.Clear();
            for (int i = 1; i <= MAXDPL; i++)
            {
                worldPosList.Add(Vector4.zero);
                colorList.Add(Vector4.zero);
                spotLightDir.Add(Vector4.zero);
                spotLightRight.Add(Vector4.zero);
                charaterUV.Add(Vector4.zero);
            }
            if (null != _font && null != _font.material && null != _font.material.mainTexture)
            {
                fontTexture = _font.material.mainTexture;
                fontTexture.wrapMode = TextureWrapMode.Clamp;
            }
            if(null != textureList && textureList.Count > 0)
            {
                Shader.SetGlobalTexture(DynamicLightTexture_1, textureList[0]);
            }
            if(globalMode)
            {
                Shader.SetGlobalFloat(DynamicPointLightNum, 0);
                Shader.SetGlobalVectorArray(DynamicPointPos, worldPosList);
                Shader.SetGlobalVectorArray(DynamicPointColor, colorList);
                Shader.SetGlobalVectorArray(DynamicPointDir, spotLightDir);
                Shader.SetGlobalVectorArray(DynamicPointRight, spotLightRight);
                Shader.SetGlobalVectorArray(DynamicCharaterUV, charaterUV);
            }
            if (null != fontTexture)
            {
                Shader.SetGlobalTexture(DynamicFontTexture, fontTexture);
            }

            gridCount = sceneSize / (int)gridSize;
            cullDistance2 = cullDistance * cullDistance;
            materialPropertyBlock = new MaterialPropertyBlock();
            mainCameraTr = UnityEngine.Camera.main.transform;

            _instance = this;
        }

        public void UpdateTextureList()
        {
            if (null != textureList && textureList.Count > 0)
            {
                Shader.SetGlobalTexture(DynamicLightTexture_1, textureList[0]);
            }
        }
        public void UpdateFontTexture()
        {
            if (null != _font && null != _font.material && null != _font.material.mainTexture)
            {
                fontTexture = _font.material.mainTexture;
                fontTexture.wrapMode = TextureWrapMode.Clamp;
            }
            if (null != fontTexture)
            {
                Shader.SetGlobalTexture(DynamicFontTexture, fontTexture);
            }
        }
        public void UpdateFont(Font font)
        {
            _font = font;
            UpdateFontTexture();
        }
        public Font GetFont()
        {
            return _font;
        }

        public void DynaimicLightTextChange(string spotText)
        {
            if (!string.IsNullOrEmpty(spotText))
            {
                if (null != _font)
                {
                    _font.RequestCharactersInTexture(spotText, 48, FontStyle.Normal);
                    if (null != fontTexture)
                    {
                        Shader.SetGlobalTexture(DynamicFontTexture, fontTexture);
                    }
                }
            }
        }

        public void AddDynamicPointLight(DynamicLight dpl, MeshRenderer rednerer)
        {
            if(null != dpl)
            {
                dplList.Add(dpl);
                dplRendererDic[dpl] = rednerer;
#if GAMEDEBUG
                if(null != dpl.GetTransform())
                {
                    if(!dplObjs.Contains(dpl.GetTransform().gameObject))
                    {
                        dplObjs.Add(dpl.GetTransform().gameObject);
                    }
                }
#endif
            }
        }
        
        public void RemoveDynamicPonitLight(DynamicLight dpl)
        {
            int index = dplList.IndexOf(dpl);
            if(index >= 0)
            {
                //从位置和颜色列表中删除
                //worldPosList.RemoveAt(index);
                //colorList.RemoveAt(index);

                //dpl.lightIntensity = 0f;
                dplList.Remove(dpl);
            }
        }

        private Dictionary<Renderer, HashSet<DynamicLight>> renderLightDic = new Dictionary<Renderer, HashSet<DynamicLight>>();
        public static MaterialPropertyBlock materialPropertyBlock = null;
        public const float gridSize = 16f;
        private int gridCount = 0;
        private void ClearMPB()
        {
            foreach (var item in renderLightDic)
            {
                var mr = item.Key;
                if(null != mr)
                {
                    mr.SetPropertyBlock(null);
                }

                var list = item.Value;
#if UNITY_EDITOR
                foreach(var dlItem in list)
                {
                    if(dlItem.moveType == DynamicLight.MoveType.DYNAMIC)
                    {
                        dlItem.effectedList.Clear();
                    }
                }
#endif
            }

            renderLightDic.Clear();
        }
        private void UpdateLocalLight()
        {
            if (globalMode)
            {
                return;
            }
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,UpdateLocalLight1");
#endif
#if UNITY_EDITOR
            if (null == materialPropertyBlock)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
#endif
            if (null == materialPropertyBlock)
            {
                return;
            }

            ClearMPB();
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,CheckDistance");
#endif
            if(Application.isPlaying && null != mainCameraTr)
            {
                //检测距离
                var mainCamPos = mainCameraTr.position;
                var mainCamForward = mainCameraTr.forward;
                float distance2 = 0f;
                DynamicLight dl;
                MeshRenderer mr;
                bool enableLight = false;
                Transform spotRenderTransform;
                float objToCamDirX, objToCamDirZ;
                float dotNonNormalize;
                Vector3 dlPositon;
                foreach (var kv in dplRendererDic)
                {
                    dl = kv.Key;
                    mr = kv.Value;
                    if(null != dl)
                    {
                        dlPositon = dl.GetTransform().position;
                        objToCamDirX = dlPositon.x - mainCamPos.x;
                        objToCamDirZ = dlPositon.z - mainCamPos.z;
                        dotNonNormalize = objToCamDirX * mainCamForward.x + objToCamDirZ * mainCamForward.z;
                        if(dotNonNormalize < 0)
                        {
                            enableLight = false;
                        }
                        else
                        {
                            distance2 = dlPositon.SqrDistance(mainCamPos);// Vector3.SqrMagnitude(dlPositon - mainCamPos);
                            enableLight = distance2 < cullDistance2;
                        }
                        enableLight = enableLight || dl.noCulling;
                        dl.enableLight = enableLight;
                        if ( null != mr)
                        {
                            mr.enabled = enableLight;
                        }
                        if (enableLight && null != mr)
                        {
                            spotRenderTransform = dl.GetSpotRenderTransform();
                            var up = spotRenderTransform.up;
                            Vector3 direction = mainCameraTr.position.Sub(spotRenderTransform.position);// mainCameraTr.position - spotRenderTransform.position;
                            Vector3 normal = Vector3.Cross(direction, up);
                            Vector3 normal2 = Vector3.Cross(up, normal);
                            Quaternion lookRotation = Quaternion.LookRotation(normal2, up);
                            spotRenderTransform.rotation = lookRotation;
                        }

                    }
                }
            }
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,UpdateLocalLight2");
#endif
            for (int i = 0; i < dplList.Count && i < MAXDPL; i++)
            {
                var dpl = dplList[i];
                if(null == dpl)
                {
                    continue;
                }
                if(dpl.showType == DynamicLight.ShowType.OPTIONAL || !dpl.enableLight)
                {
                    continue;
                }

                var needdis = dpl.lightRadius * dpl.lightRadius;
                var p1 = dpl.GetTransform().position;
                if(dpl.moveType == DynamicLight.MoveType.STATIC)
                {
#if GAMEDEBUG
                    UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,UpdateLocalLight2_1");
#endif
                    //静态灯光离线计算的对静态物体的影响
                    List<Renderer> mrArr = dpl.effectedList;
                    if(null != mrArr)
                    {
                        for(int j = 0; j < mrArr.Count && j < MAXRENDERER_PERLIGHT; j++)
                        {
                            var mr = mrArr[j];
                            if(null != mr && mr.enabled)
                            {
                                HashSet<DynamicLight> dlList;
                                if(!renderLightDic.TryGetValue(mr, out dlList))
                                {
                                    dlList = new HashSet<DynamicLight>();
                                    renderLightDic[mr] = dlList;
                                }
                                if (dlList.Count < MAXLIGHT_PERRENDER)
                                {
                                    dlList.Add(dpl);
                                }
                            }
                        }
                    }
#if GAMEDEBUG
                    UnityEngine.Profiling.Profiler.EndSample();
#endif
                }
                else if(dpl.moveType == DynamicLight.MoveType.DYNAMIC)
                {
#if GAMEDEBUG
                    UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,UpdateLocalLight2_2");
#endif
                    //那些能够移动的光源
                    //计算它影响的场景中的对象
                    var center = p1;
                    var extends = dpl.lightRadius * 2;
                    //计算出所在格子
                    int currL = Mathf.CeilToInt(center.z / gridSize) - 1;//当前行
                    var currC = Mathf.CeilToInt(center.x / gridSize) - 1;//当前列
                    int gridDataIndex = currL * gridCount + currC;
                    //Debug.LogError("===================gridDataIndex:" + gridDataIndex);
                    if (null != sceneGridData && sceneGridData.Count > gridDataIndex)
                    {
                        var rendererList = sceneGridData[gridDataIndex];
                        if (null != rendererList)
                        {
                            var rdList = rendererList.rendererList;
                            if (null != rdList)
                            {
                                //动态类型的不判断每个灯影响的最大数
                                for (int j = 0; j < rdList.Count /*&& j < MAXRENDERER_PERLIGHT*/; j++)
                                {
                                    var mr = rdList[j];
                                    if (null != mr && mr.enabled /*&& mr.gameObject.layer == GameDefine_GlobalVar.Art30Layer*/ && mr.bounds.SqrDistance(center) < needdis)
                                    {
                                        HashSet<DynamicLight> dlList;
                                        if (!renderLightDic.TryGetValue(mr, out dlList))
                                        {
                                            dlList = new HashSet<DynamicLight>();
                                            renderLightDic[mr] = dlList;
                                        }
                                        if (dlList.Count < MAXLIGHT_PERRENDER)
                                        {
                                            dlList.Add(dpl);
#if UNITY_EDITOR
                                            dpl.effectedList.Add(mr);
#endif
                                        }
                                    }
                                }
                            }
                        }
                    }


                    //int lineNum = Mathf.CeilToInt(extends / gridSize);//跨几行
                    //int columnNum = Mathf.CeilToInt(extends / gridSize);//跨几列
                    //for (int l = -lineNum; l <= lineNum; l++)
                    //{
                    //    for (int c = -columnNum; c <= columnNum; c++)
                    //    {
                    //        int r = currL + l;
                    //        int lie = currC + c;
                    //        if (r >= 0 && lie >= 0 && r < gridCount && lie < gridCount)
                    //        {
                    //            int gridDataIndex = r * gridCount + lie;
                    //            //Debug.LogError("===================gridDataIndex:" + gridDataIndex);
                    //            if (null != sceneGridData && sceneGridData.Count > gridDataIndex)
                    //            {
                    //                var rendererList = sceneGridData[gridDataIndex];
                    //                if (null != rendererList)
                    //                {
                    //                    var rdList = rendererList.rendererList;
                    //                    if (null != rdList)
                    //                    {
                    //                        for (int j = 0; j < rdList.Count; j++)
                    //                        {
                    //                            var mr = rdList[j];
                    //                            if (null != mr && mr.bounds.SqrDistance(center) < needdis)
                    //                            {
                    //                                List<DynamicLight> dlList;
                    //                                if (!renderLightDic.TryGetValue(mr, out dlList))
                    //                                {
                    //                                    dlList = GameUtil.PoolNewListDynamicLight();
                    //                                    renderLightDic[mr] = dlList;
                    //                                }
                    //                                if (!dlList.Contains(dpl))
                    //                                {
                    //                                    dlList.Add(dpl);
                    //                                }
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
#if GAMEDEBUG
                    UnityEngine.Profiling.Profiler.EndSample();
#endif
                }

#if GAMEDEBUG
                UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,UpdateLocalLight2_3");
#endif
                //开始计算灯光对动态物体的影响
                if(/*dpl.lightType != DynamicLight.LightType.SPOT_SKY && */dpl.effectCharacter)
                {
                    //to do
                }
#if GAMEDEBUG
                UnityEngine.Profiling.Profiler.EndSample();
#endif
            }
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif

#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,UpdateLocalLight3");
#endif
            foreach (var item in renderLightDic)
            {
                materialPropertyBlock.Clear();

                var dplList = item.Value;
                var mr = item.Key;
                if(null == mr || !mr.enabled)
                {
                    continue;
                }
                Texture2D dynamicTexture = null;
                int i = -1;
                foreach(var dplItem in dplList)
                {
                    i = i + 1;
                    var dpl = dplItem;
                    Vector3 pos = dpl.GetTransform().position;
                    Color col = dpl.lightColor * dpl.lightIntensity;
                    tempVector4.x = pos.x;
                    tempVector4.y = pos.y;
                    tempVector4.z = pos.z;
                    tempVector4.w = dpl.lightRadius * dpl.lightRadius;
                    worldPosList[i] = tempVector4;

                    tempVector4.x = col.r;
                    tempVector4.y = col.g;
                    tempVector4.z = col.b;
                    tempVector4.w = (float)dpl.lightType;
                    colorList[i] = tempVector4;
                    Vector3 lightDir = -dpl.GetTransform().forward;

                    tempVector4.x = lightDir.x;
                    tempVector4.y = lightDir.y;
                    tempVector4.z = lightDir.z;
                    tempVector4.w = Mathf.Cos(dpl.spotAngle / 2f * Mathf.Deg2Rad);
                    spotLightDir[i] = tempVector4;
                    
                    Vector3 lightRight = Vector3.Normalize(dpl.GetTransform().right);
                    tempVector4.x = lightRight.x;
                    tempVector4.y = lightRight.y;
                    tempVector4.z = lightRight.z;
                    tempVector4.w = dpl.atlasColumn * 1000 + ((int)dpl.aniType) * 100 + (dpl.textureIndex <= 0 ? 0 : dpl.textureIndex) * 10 + dpl.textureScale;
                    spotLightRight[i] = tempVector4;

                    //获取需要投影的texture
                    if (dpl.textureIndex > 0 && dpl.textureList.Count > 0 && dpl.aniType != DynamicLight.AniType.DYNAMICFONT)
                    {
                        if(dpl.spriteFPS > 0)
                        {
                            //自动播放
                            dynamicTexture = dpl.textureList[dpl.SpriteIndex];
                        }
                        else
                        {
                            //指定播放
                            if(dpl.textureList.Count >= dpl.textureIndex && null != dpl.textureList[dpl.textureIndex - 1])
                            {
                                dynamicTexture = dpl.textureList[dpl.textureIndex - 1];
                            }
                        }
                        if(null != dynamicTexture)
                        {
                            tempVector4.x = 0;
                            tempVector4.y = 0;
                            tempVector4.z = (float)dynamicTexture.width;
                            tempVector4.w = (float)dynamicTexture.height;
                            charaterUV[i] = tempVector4;
                        }
                    }
                    if (null == dynamicTexture)
                    {
                        if (null != textureList && textureList.Count > 0 && null != textureList[0])
                        {
                            tempVector4.x = 0;
                            tempVector4.y = 0;
                            tempVector4.z = (float)textureList[0].width;
                            tempVector4.w = (float)textureList[0].height;
                            charaterUV[i] = tempVector4;
                        }
                    }

                    //Debug.LogError("====lightDir:"+ lightDir + " :" + Mathf.Cos(dpl.spotAngle / 2f * Mathf.Deg2Rad));
                    if (!string.IsNullOrEmpty(dpl.spotText) && dpl.aniType == DynamicLight.AniType.DYNAMICFONT)
                    {
                        if (null != _font)
                        {
                            CharacterInfo ch;
                            _font.GetCharacterInfo(dpl.spotText[0], out ch, 48, FontStyle.Normal);
                            Vector2 uvBottomLeft = ch.uvBottomLeft;
                            Vector2 uvBottomRight = ch.uvBottomRight;
                            Vector2 uvTopLeft = ch.uvTopLeft;
                            Vector2 uvTopRight = ch.uvTopRight;
                            //float w = Mathf.Abs(ch.uvTopLeft.x - ch.uvBottomRight.x);
                            //float h = Mathf.Abs(ch.uvBottomLeft.y - ch.uvTopRight.y);
                            //Debug.LogError("====字uv:" + dpl.spotText[0] + " :" + w + " :" + h + " :" + w/h + ":" + ch.glyphWidth + " :" + ch.glyphHeight + ":" + ch.glyphWidth/ ch.glyphHeight);
                            //Debug.LogError("====minx:" + ch.minX + " :" + ch.maxX + " :" + ch.minY + " :" + ch.maxY);
                            if (uvTopLeft.x > uvBottomLeft.x)
                            {
                                //朝右
                                tempVector4.x = 10 + uvBottomLeft.x;
                                tempVector4.y = uvBottomLeft.y;
                                tempVector4.z = (float)ch.glyphWidth + uvTopRight.x;
                                tempVector4.w = (float)ch.glyphHeight + uvTopRight.y;
                                charaterUV[i] = tempVector4;
                            }
                            else if (uvTopLeft.y < uvBottomLeft.y)
                            {
                                //垂直镜像
                                tempVector4.x = 20 + uvBottomLeft.x;
                                tempVector4.y = uvBottomLeft.y;
                                tempVector4.z = (float)ch.glyphWidth + uvTopRight.x;
                                tempVector4.w = (float)ch.glyphHeight + uvTopRight.y;
                                charaterUV[i] = tempVector4;
                            }
                        }
                    }
                }
                materialPropertyBlock.SetFloat(DynamicPointLightNum, dplList.Count);
                materialPropertyBlock.SetVectorArray(DynamicPointPos, worldPosList);
                materialPropertyBlock.SetVectorArray(DynamicPointColor, colorList);
                materialPropertyBlock.SetVectorArray(DynamicPointDir, spotLightDir);
                materialPropertyBlock.SetVectorArray(DynamicPointRight, spotLightRight);
                materialPropertyBlock.SetVectorArray(DynamicCharaterUV, charaterUV);
                if(null == dynamicTexture)
                {
                    if (null != textureList && textureList.Count > 0)
                    {
                        Shader.SetGlobalTexture(DynamicLightTexture_1, textureList[0]);
                    }else
                    {
                        materialPropertyBlock.SetTexture(DynamicLightTexture_1, Texture2D.blackTexture);
                    }
                }
                else
                {
                    materialPropertyBlock.SetTexture(DynamicLightTexture_1, dynamicTexture);
                }
                //if (null != fontTexture)
                //{
                //    materialPropertyBlock.SetTexture(DynamicFontTexture, fontTexture);
                //}

                mr.SetPropertyBlock(materialPropertyBlock);
            }
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        // Update is called once per frame
        void Update()
        {
            if(!globalMode)
            {
                //Shader.SetGlobalFloat("DynamicPointLightNum", 0);
                UpdateLocalLight();
                return;
            }
            if (dplList.Count <= 0)
            {
                Shader.SetGlobalFloat(DynamicPointLightNum, 0);
                return;
            }
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,Update");
#endif
            for (int i = 0; i < dplList.Count && i < MAXDPL; i++)
            {
                var dpl = dplList[i];
                //dpl.Tick();
                Vector3 pos = dpl.GetTransform().position;
                Color col = dpl.lightColor * dpl.lightIntensity;
                tempVector4.x = pos.x;
                tempVector4.y = pos.y;
                tempVector4.z = pos.z;
                tempVector4.w = dpl.lightRadius * dpl.lightRadius;
                worldPosList[i] = tempVector4;

                tempVector4.x = col.r;
                tempVector4.y = col.g;
                tempVector4.z = col.b;
                tempVector4.w = (float)dpl.lightType;
                colorList[i] = tempVector4;
                Vector3 lightDir = -dpl.GetTransform().forward;

                tempVector4.x = lightDir.x;
                tempVector4.y = lightDir.y;
                tempVector4.z = lightDir.z;
                tempVector4.w = Mathf.Cos(dpl.spotAngle / 2f * Mathf.Deg2Rad);
                spotLightDir[i] = tempVector4;

                Vector3 lightRight = Vector3.Normalize(dpl.GetTransform().right);
                tempVector4.x = lightRight.x;
                tempVector4.y = lightRight.y;
                tempVector4.z = lightRight.z;
                tempVector4.w = dpl.atlasColumn * 1000 + ((int)dpl.aniType) * 100 + (dpl.textureIndex <= 0 ? 0 : dpl.textureIndex) * 10 + dpl.textureScale;
                spotLightRight[i] = tempVector4;
                //Debug.LogError("====lightDir:"+ lightDir + " :" + Mathf.Cos(dpl.spotAngle / 2f * Mathf.Deg2Rad));
                if (!string.IsNullOrEmpty(dpl.spotText))
                {
                    if (null != _font)
                    {
                        CharacterInfo ch;
                        _font.GetCharacterInfo(dpl.spotText[0], out ch, 48, FontStyle.Normal);
                        Vector2 uvBottomLeft = ch.uvBottomLeft;
                        Vector2 uvBottomRight = ch.uvBottomRight;
                        Vector2 uvTopLeft = ch.uvTopLeft;
                        Vector2 uvTopRight = ch.uvTopRight;
                        if (uvTopLeft.x > uvBottomLeft.x)
                        {
                            //朝右
                            tempVector4.x = 10 + uvBottomLeft.x;
                            tempVector4.y = uvBottomLeft.y;
                            tempVector4.z = uvTopRight.x;
                            tempVector4.w = uvTopRight.y;
                            charaterUV[i] = tempVector4;
                        }
                        else if (uvTopLeft.y < uvBottomLeft.y)
                        {
                            //垂直镜像
                            tempVector4.x = 20 + uvBottomLeft.x;
                            tempVector4.y = uvBottomLeft.y;
                            tempVector4.z = uvTopRight.x;
                            tempVector4.w = uvTopRight.y;
                            charaterUV[i] = tempVector4;
                        }
                    }
                }
            }
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif

            //for (int i = 1; i <= MAXTEXTURE; i++)
            //{
            //    if (textureList.Count >= (i - 1) && null != textureList[i - 1])
            //    {
            //        Shader.SetGlobalTexture("DynamicLightTexture_" + i, textureList[i - 1]);
            //    }
            //}
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.BeginSample("====DynamicLightManager,ShaderSetGlobal");
#endif
            Shader.SetGlobalFloat(DynamicPointLightNum, dplList.Count);
            Shader.SetGlobalVectorArray(DynamicPointPos, worldPosList);
            Shader.SetGlobalVectorArray(DynamicPointColor, colorList);
            Shader.SetGlobalVectorArray(DynamicPointDir, spotLightDir);
            Shader.SetGlobalVectorArray(DynamicPointRight, spotLightRight);
            Shader.SetGlobalVectorArray(DynamicCharaterUV, charaterUV);
            //if (null != fontTexture)
            //{
            //    Shader.SetGlobalTexture(DynamicFontTexture, fontTexture);
            //}
#if GAMEDEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        public DynamicLight GetDynamicLightByIndex(int index)
        {
            if(null != dplList)
            {
                int len = dplList.Count;
                DynamicLight dl;
                for(int i = 0; i < len; i++)
                {
                    dl = dplList[i];
                    if(null != dl && dl.LightIndex == index)
                    {
                        return dl;
                    }
                }
            }

            return null;
        }

        public void SetLightIntensity(float _instensity)
        {
            for (int i = 0; i < dplList.Count; i++)
            {
                dplList[i].lightIntensity = _instensity;
            }
        }
        public void SetLightSpotAngle(float _angle)
        {
            for (int i = 0; i < dplList.Count; i++)
            {
                dplList[i].spotAngle = _angle;
            }
        }
        public void SetLightPianBlendMode(int _SrcBlend, int _DstBlend)
        {
            for (int i = 0; i < dplList.Count; i++)
            {
                var pian = dplList[i].spotRenderer;
                if(null != pian)
                {
                    var mat = pian.material;
                    if(null != mat)
                    {
                        mat.SetInt("_SrcBlend", _SrcBlend);
                        mat.SetInt("_DstBlend", _DstBlend);
                    }
                }
            }
        }
    }


    [Serializable]
    public class RendererList
    {
        public List<Renderer> rendererList = new List<Renderer>();
        public void Add(Renderer rd)
        {
            if(null != rd)
            {
                rendererList.Add(rd);
            }
        }
    }

}