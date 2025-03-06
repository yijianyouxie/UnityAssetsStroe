using Games.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Games.Util
{
    public class PingPangMulti : MonoBehaviour
    {

        public DynamicLight dpl;

        public bool pingPangColor = false;
        public Color colorFrom;
        public Color colorTo;
        public float colorStep = 0.1f;
        private float colorLerpValue = 0f;

        public bool pingPangAngle = false;
        public float maxAngle = 30f;
        public float minAngle = 25f;
        public float angleStep = 1f;
        private float angleLerpValue = 0f;

        //辉光强度
        public bool pingpangGlowAlpha = false;
        public float maxGlowAlpha = 1f;
        public float minGlowAlpha = 0.9f;
        public float glowAlphaStep = 0.01f;
        private float glowAlphaLerpValue = 0f;

        //光照强度
        public bool pingpangLightIns = false;
        public float maxLightIns = 1.8f;
        public float minLightIns = 1f;
        public float lightInsStep = 0.01f;
        private float lightInsLerpValue = 0f;

        //贴图索引
        public bool pingpangTextureIndex = false;
        public float textureIndexStep = 0.01f;
        public int minTexIndex = 1;
        public int maxTexIndex = 1;
        private float textureIndexLerpValue = -1f;

        //贴图缩放
        public bool pingpangTextureScale = false;
        public float textureScaleStep = 0.01f;
        public float maxTexScale = 1;
        public float minTexScale = 1;
        private float textureScaleLerpValue = -1f;

        //文字索引
        public bool pingpangTextIndex = false;
        public string spotText = string.Empty;
        public float textIndexStep = 0.01f;
        public int minTextIndex = 0;
        public int maxTextIndex = 0;
        private float textIndexLerpValue = 0f;

        // Use this for initialization
        void Start()
        {
            if (null == dpl)
            {
                dpl = GetComponent<DynamicLight>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (null != dpl)
            {
                if (pingPangColor)
                {
                    colorLerpValue += colorStep;
                    if (colorLerpValue >= 1f || colorLerpValue <= 0f)
                    {
                        colorStep *= -1;
                    }

                    dpl.lightColor = Color.Lerp(colorFrom, colorTo, colorLerpValue);
                }

                if (pingPangAngle)
                {
                    angleLerpValue += angleStep;
                    if (angleLerpValue >= 1f || angleLerpValue <= 0f)
                    {
                        angleStep *= -1;
                    }

                    dpl.spotAngle = Mathf.Lerp(minAngle, maxAngle, angleLerpValue);
                }

                //辉光强度
                if (pingpangGlowAlpha)
                {
                    glowAlphaLerpValue += glowAlphaStep;
                    if (glowAlphaLerpValue >= 1f || glowAlphaLerpValue <= 0f)
                    {
                        glowAlphaStep *= -1;
                    }

                    dpl.glowAlpha = Mathf.Lerp(minGlowAlpha, maxGlowAlpha, glowAlphaLerpValue);
                }

                //光强度
                if (pingpangLightIns)
                {
                    lightInsLerpValue += lightInsStep;
                    if (lightInsLerpValue >= 1f || lightInsLerpValue <= 0f)
                    {
                        lightInsLerpValue = Mathf.Clamp(lightInsLerpValue, 0f, 1f);
                        lightInsStep *= -1;
                    }

                    dpl.lightIntensity = Mathf.Lerp(minLightIns, maxLightIns, lightInsLerpValue);
                }

                //贴图索引
                if (pingpangTextureIndex)
                {
                    textureIndexLerpValue += textureIndexStep;
                    if (textureIndexLerpValue >= 1f /*|| textureIndexLerpValue <= 0f*/)
                    {
                        textureIndexLerpValue = 0;
                    }

                    dpl.textureIndex = (int)Mathf.Lerp(minTexIndex, maxTexIndex, textureIndexLerpValue);
                }

                //贴图缩放
                if (pingpangTextureScale)
                {
                    textureScaleLerpValue += textureScaleStep;
                    if (textureScaleLerpValue >= 1f || textureScaleLerpValue <= 0f)
                    {
                        textureScaleLerpValue = Mathf.Clamp(textureScaleLerpValue, 0f, 1f);
                        textureScaleStep *= -1;
                    }

                    dpl.textureScale = Mathf.Lerp(minTexScale, maxTexScale, textureScaleLerpValue);
                }

                //文本索引
                if (pingpangTextIndex)
                {
                    textIndexLerpValue += textIndexStep;
                    if (textIndexLerpValue >= maxTextIndex)
                    {
                        textIndexLerpValue = minTextIndex;
                    }

                    dpl.spotText = spotText[(int)textIndexLerpValue].ToString();
                    if (null != DynamicLightManager.Instance)
                    {
                        DynamicLightManager.Instance.DynaimicLightTextChange(dpl.spotText);
                    }
                }
            }
        }
    }
}