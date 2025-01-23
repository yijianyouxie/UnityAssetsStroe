using System;
using System.Collections.Generic;
using UnityEngine;
namespace Games.Manager
{
    [ExecuteInEditMode]
    public class DynamicLight : MonoBehaviour
    {
        [Header("默认都是-1.如果是需要控制属性的，请设置具体的值")]        
        public int LightIndex = -1;
        public enum LightType
        {
            POINT,
            SPOT,
            POINT_NOFADE,
            CYLINDER,
            SPOT_SKY,//专门照天空的,其余类型是照除了天空之外的对象
        }
        [SerializeField]
        private LightType _lightType = LightType.SPOT;
        public LightType lightType
        {
            get { return _lightType; }
            set
            {
                _lightType = value;
                if (_lightType == LightType.SPOT_SKY)
                {
                    noCulling = true;

                    FindSky();
                }
                else
                {
                    noCulling = false;
                }
            }
        }
        [SerializeField]
        private bool _noCulling = false;
        public bool noCulling
        {
            get { return _noCulling; }
            set { _noCulling = value; }
        }
        public enum ShowType
        {
            FORCESHOW,//投影必须显示的类型
            OPTIONAL,//投影可被优化的
        }
        [Header("是否必须显示投影")]
        public ShowType showType = ShowType.OPTIONAL;

        public enum MoveType
        {
            DYNAMIC,//可移动的，挂在人物身上的
            STATIC,//静态的，位置不能移动的
        }
        [Header("是否是动态移动的")]
        public MoveType moveType = MoveType.STATIC;
        [Header("是否自动更新灯光数据，静态不动的灯不用勾选")]
        public bool autoUpdateLightData = false;
        [Header("是否影响角色")]
        public bool effectCharacter = false;

        public Color lightColor;
        [Range(0f, 10f)]
        public float lightIntensity = 1f;
        ////灯光最大强度。最小强度是0
        //[Range(0f, 10f)]
        //public float maxLightIntensity = 1f;
        [Header("光源的照亮半径")]
        [Range(0f, 50f)]
        public float lightRadius = 5f;

        [Header("射灯的广角大小;点光源用不到")]
        [Range(0f, 60f)]
        public float spotAngle = 30f;

        [Header("辉光的近裁剪面长度，要小于辉光的长度")]
        [Range(0f, 50f)]
        public float glowNearClipDistance = 0f;
        [Header("辉光的长度")]
        [Range(0f, 2500f)]
        public float glowDistance = 5f;

        [Header("辉光alpha值")]
        [Range(0.1f, 100f)]
        public float glowAlpha = 1f;

        //[Header("spot灯是否辉光流动")]
        //public bool glowFlow = false;


        //[Header("是否影响x:地面;y:人物;z:地上物体;1影响;0不影响")]
        //public Vector4 influnceScope = new Vector4(1f, 1f, 1f, 1f);

        [Header("====序列图列表")]
        public List<Texture2D> textureList = new List<Texture2D>(4);
        //[Header("标记为重要，则有机会投影里边的图")]
        //public bool isImportant = false;
        [Header("FPS的大于1则会自动播放，否则选择textureIndex指定的图")]
        public float spriteFPS = 0;
        private int spriteIndex = 0;
        public int SpriteIndex
        {
            get { return spriteIndex; }
        }
        private float mDelta = 0f;

        [Header("spot灯投影的贴图索引,从1开始")]
        [Range(-1, 10)]
        public int textureIndex = -1;

        [Header("序列图的列数")]
        public int atlasColumn = 1;
        [Header("spot灯投影的贴图缩放比例")]
        [Range(0.01f, 9.9f)]
        public float textureScale = 1f;

        public enum AniType
        {
            NONE,
            DISTURB,//扰动
            UVFLOW_1,//uv流动速度1
            UVFLOW_2,//uv流动速度2
            ALPHA_CENTER,//alpha值变化
            ALPHA_TOP,//alpha值变化，从上往下
            ATLAS,//需要读图集
            DYNAMICFONT,//动态字体

        }
        [Header("动画类型")]
        [SerializeField]
        private AniType _aniType = 0;
        public AniType aniType
        {
            get { return _aniType; }
            set
            {
                _aniType = value;
                if(_aniType == AniType.DYNAMICFONT)
                {
                    textureIndex = 1;
                }
            }
        }
        [Header("DYNAMICFONT.spot灯投影的单个文字")]
        [SerializeField]
        private string _spotText = string.Empty;
        public string spotText
        {
            get { return _spotText; }
            set
            {
                if(_spotText != value)
                {
                    if (null != DynamicLightManager.Instance)
                    {
                        DynamicLightManager.Instance.DynaimicLightTextChange(value);
                    }
                }
                _spotText = value;
            }
        }
        private string lastSpotText = string.Empty;

        public List<Renderer> effectedList;

        //[Header("曲线的x轴是时间轴，可以定义持续时间。y轴是强度从0到最大的比例。")]
        //public AnimationCurve lightIntensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        ////流逝时间
        //private float elapseTime = 0f;

        private Transform trs;

        public MeshRenderer spotRenderer = null;
        private Transform spotRendererTrans = null;
        private MaterialPropertyBlock materialPropertyBlock = null;
        private Vector4 tempVector4 = Vector4.zero;
        private Vector3 tempVector3 = Vector3.zero;
        //private bool maxed = false;

        [NonSerialized]
        public bool enableLight = true;
        // Use this for initialization
        void Awake()
        {
            trs = transform;

            lastSpotText = _spotText;

            spotRendererTrans = null == spotRenderer ? null : spotRenderer.transform;

            if (null != DynamicLightManager.Instance)
            {
                DynamicLightManager.Instance.DynaimicLightTextChange(_spotText);
                materialPropertyBlock = DynamicLightManager.materialPropertyBlock;
            }

            if(lightType == LightType.SPOT_SKY)
            {
                noCulling = true;

                FindSky();
            }
            Font.textureRebuilt += UpdateDynaimicLightText;
            UpdateData();
        }
        void UpdateDynaimicLightText(Font font)
        {
            if(_aniType != AniType.DYNAMICFONT)
            {
                return;
            }
            if (null != DynamicLightManager.Instance && DynamicLightManager.Instance.GetFont() == font)
            {
                DynamicLightManager.Instance.DynaimicLightTextChange(_spotText);
            }
        }
        private void FindSky()
        {
            var skyCube = GameObject.Find("WeatherManager/SkyCube");
            if (null == skyCube)
            {
                skyCube = GameObject.Find("sky_01");
            }
            if (null != skyCube)
            {
                var rd = skyCube.GetComponent<Renderer>();
                if (null != rd && !effectedList.Contains(rd))
                {
                    effectedList.Add(rd);
                }
            }
        }

        private void OnValidate()
        {
            if(lastSpotText != _spotText)
            {
                if (null != DynamicLightManager.Instance)
                {
                    DynamicLightManager.Instance.DynaimicLightTextChange(_spotText);
                }
                lastSpotText = _spotText;
            }
            if(_lightType == LightType.SPOT_SKY)
            {
                noCulling = true;
                FindSky();
            }

            UpdateData();
        }

        private void Update()
        {
            if(moveType == MoveType.DYNAMIC || autoUpdateLightData)
            {
                UpdateData();
            }

            if(spriteFPS > 0 && null != textureList && textureList.Count > 1)
            {
                mDelta += Time.unscaledDeltaTime;
                float rate = 1f / spriteFPS;

                if (rate < mDelta)
                {
                    mDelta = (rate > 0f) ? mDelta - rate : 0f;

                    if (++spriteIndex >= textureList.Count)
                    {
                        spriteIndex = 0;
                    }
                }
            }
        }

        public void AddTexture(Texture2D t2d)
        {
            if(null != t2d)
            {
                if(null == textureList)
                {
                    textureList = new List<Texture2D>(4);
                }

                textureList.Add(t2d);
            }
        }

        public void UpdateData()
        {
#if UNITY_EDITOR
            if (null == materialPropertyBlock)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
#endif
            if (null == materialPropertyBlock || null == trs)
            {
                return;
            }

            if (_lightType == DynamicLight.LightType.SPOT || _lightType == LightType.SPOT_SKY)
            {
                if (null != spotRenderer)
                {
                    float rad = spotAngle / 2f * Mathf.Deg2Rad;
                    float quadH = glowDistance;
                    float quadw = quadH * Mathf.Tan(rad) * 2;
                    tempVector3.x = quadw;
                    tempVector3.y = quadH;
                    tempVector3.z = 1;
                    spotRenderer.transform.localScale = tempVector3;
                    tempVector3.x = 0;
                    tempVector3.y = 0;
                    tempVector3.z = quadH / 2;
                    spotRenderer.transform.localPosition = tempVector3;

                    materialPropertyBlock.Clear();
                    tempVector4.x = trs.position.x;
                    tempVector4.y = trs.position.y;
                    tempVector4.z = trs.position.z;
                    tempVector4.w = glowNearClipDistance >= glowDistance ? glowDistance * glowDistance : glowNearClipDistance * glowNearClipDistance;
                    materialPropertyBlock.SetVector("DynamicPointPos2", tempVector4);
                    Vector3 lightDir = -GetTransform().forward;
                    tempVector4.x = lightDir.x;
                    tempVector4.y = lightDir.y;
                    tempVector4.z = lightDir.z;
                    tempVector4.w = Mathf.Cos(rad);
                    Vector4 spotLightDir = tempVector4;
                    materialPropertyBlock.SetVector("DynamicPointDir2", spotLightDir);
                    Color c = lightColor * lightIntensity;
                    tempVector4.x = c.r;
                    tempVector4.y = c.g;
                    tempVector4.z = c.b;
                    tempVector4.w = glowDistance * glowDistance;
                    materialPropertyBlock.SetVector("DynamicPointColor2", tempVector4);
                    materialPropertyBlock.SetFloat("_AlphaScale", glowAlpha);
                    //materialPropertyBlock.SetFloat("_GlowFlow", dpl.glowFlow ? 1f : 0f);
                    spotRenderer.SetPropertyBlock(materialPropertyBlock);
                }
            }
        }

        public Transform GetTransform()
        {
            return trs;
        }
        
        public Transform GetSpotRenderTransform()
        {
            return spotRendererTrans;
        }

        private void OnEnable()
        {
            if (null != DynamicLightManager.Instance)
            {
                DynamicLightManager.Instance.AddDynamicPointLight(this, spotRenderer);
            }
        }

        private void OnDisable()
        {
            if (null != DynamicLightManager.Instance)
            {
                DynamicLightManager.Instance.RemoveDynamicPonitLight(this);
            }
        }

        private void OnDestroy()
        {
            if (null != DynamicLightManager.Instance)
            {
                DynamicLightManager.Instance.RemoveDynamicPonitLight(this);
            }
            Font.textureRebuilt -= UpdateDynaimicLightText;

            if(null != effectedList)
            {
                effectedList.Clear();
                effectedList = null;
            }
            if(null != textureList)
            {
                textureList.Clear();
                textureList = null;
            }
        }

        public void SetLightEnable(bool _enableLight)
        {
            enableLight = _enableLight;
        }

        private void OnDrawGizmos()
        {
            if(_lightType == LightType.POINT || _lightType == LightType.POINT_NOFADE)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(trs.position, lightRadius);
            }
            else if(_lightType == LightType.SPOT || _lightType == LightType.SPOT_SKY)
            {
                Gizmos.color = Color.green;
                Matrix4x4 matrix = Gizmos.matrix;
                Gizmos.matrix = trs.localToWorldMatrix;
                Gizmos.DrawFrustum(Vector3.zero, spotAngle, lightRadius, 0f, 1f);
                Gizmos.matrix = matrix;
                //Gizmos.DrawLine(trs.position, trs.position + 10*trs.forward);
                //Debug.LogError("======" + trs.position + " :" + (trs.forward));

                //辉光展示
                Gizmos.color = Color.red;
                Gizmos.DrawLine(trs.position + glowNearClipDistance * trs.forward, trs.position + glowDistance * trs.forward);

                //右方向展示
                //辉光展示
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(trs.position, trs.position + 2 * trs.right);
            }
            else if(_lightType == LightType.CYLINDER)
            {
                //Gizmos.dr
            }
        }
    }
}