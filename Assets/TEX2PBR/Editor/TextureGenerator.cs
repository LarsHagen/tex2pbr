using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tex2pbr
{
    [System.Serializable]
    public class TextureGenerator
    {
        public TextureGenerator()
        {
        }

        [SerializeField]
        int textureWidth;
        [SerializeField]
        int textureHeight;

        [SerializeField]
        Tex2pbrColor[] albedo_LowNoiseRemoval_LowShadowRemoval;
        [SerializeField]
        Tex2pbrColor[] albedo_HighNoiseRemoval_LowShadowRemoval;
        [SerializeField]
        Tex2pbrColor[] albedo_LowNoiseRemoval_HighShadowRemoval;
        [SerializeField]
        Tex2pbrColor[] albedo_HighNoiseRemoval_HighShadowRemoval;
        
        [SerializeField]
        float[] heightmap_sharp;
        [SerializeField]
        float[] heightmap_smooth;

        [SerializeField]
        Tex2pbrColor[] normal_NoDetails;
        [SerializeField]
        Tex2pbrColor[] normal_HighDetails;

        [SerializeField]
        float[] occlusion_LowSpread;
        [SerializeField]
        float[] occlusion_HighSpread;

        [SerializeField]
        float[] metallic_Low;
        [SerializeField]
        float[] metallic_High;

        ThreadedJob regenThread;
        /*ThreadedJob getAlbedoThread;
        ThreadedJob getHeightThread;
        ThreadedJob getNormalThread;
        ThreadedJob getOcclusionThread;
        ThreadedJob getMetallicThread;*/

        public bool working
        {
            get
            {
                return !(regenThread == null || regenThread.isDone);
            }
        }

        float _workingProgress;
        public float workingProgress
        {
            get
            {
                return _workingProgress;
            }
            private set
            {
                _workingProgress = Mathf.Clamp01(value);
            }
        }

        public Texture2D GetAlbedo(float noiseRemovalStrength, float shadowRemovalStrength)
        {
            Texture2D albedo = new Texture2D(textureWidth, textureHeight);
            SetTexture(albedo,
                ImageProcessing.Blend(
                    ImageProcessing.Blend(albedo_LowNoiseRemoval_LowShadowRemoval, albedo_HighNoiseRemoval_LowShadowRemoval, noiseRemovalStrength),
                    ImageProcessing.Blend(albedo_LowNoiseRemoval_HighShadowRemoval, albedo_HighNoiseRemoval_HighShadowRemoval, noiseRemovalStrength), 
                    shadowRemovalStrength
                )
            );
            return albedo;
        }
        public Texture2D GetHeight(float smoothStrength)
        {
            Texture2D height = new Texture2D(textureWidth, textureHeight);
            SetTexture(height, ImageProcessing.Blend(heightmap_sharp, heightmap_smooth, smoothStrength));
            return height;
        }
        public Texture2D GetNormal(float detailStrength)
        {
            Texture2D normal = new Texture2D(textureWidth, textureHeight);
            SetTexture(normal, ImageProcessing.Blend(normal_NoDetails, normal_HighDetails, detailStrength * 0.8f));
            return normal;
        }
        public Texture2D GetOcclusion(float occlusionSpread)
        {
            Texture2D occlusion = new Texture2D(textureWidth, textureHeight);
            SetTexture(occlusion, ImageProcessing.Blend(occlusion_LowSpread, occlusion_HighSpread, occlusionSpread));
            return occlusion;
        }
        public Texture2D GetMetallic(float metallicness)
        {
            Texture2D metallic = new Texture2D(textureWidth, textureHeight);
            SetTexture(metallic, ImageProcessing.Blend(metallic_Low, metallic_High, metallicness));
            return metallic;
        }

        public Texture2D GetRuntimeNormal(Texture2D normal)
        {
            Texture2D runtimeNormal = new Texture2D(normal.width, normal.height, TextureFormat.RGBA32, true);
            runtimeNormal.filterMode = FilterMode.Trilinear;
            runtimeNormal.wrapMode = TextureWrapMode.Repeat;
            Color[] pixels = normal.GetPixels();
            float r, g, b, a;
            for (int i = pixels.Length - 1; i >= 0; i--)
            {
                Color c = pixels[i];
                r = g = b = c.g;
                a = c.r;
                pixels[i] = new Color(r, g, b, a);
            }
            runtimeNormal.SetPixels(pixels);
            runtimeNormal.Apply(true);

            return runtimeNormal;
        }

        public void Update()
        {
            if (regenThread != null)
            {
                if (regenThread.isDone)
                {
                    if (regenThread.onDone != null) regenThread.onDone();
                    regenThread = null;
                }
            }
        }

        
        public void ReGenerate(Texture2D input, Action _onDone)
        {
            if (regenThread != null && !regenThread.isDone)
            {
                regenThread.Abort();
            }

            Texture2D readableTexture = new Texture2D(input.width, input.height);
            readableTexture.LoadImage(System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(input)), false);

            

            textureWidth = readableTexture.width;
            textureHeight = readableTexture.height;
            
            Tex2pbrColor[] rawPixels = UnityColorsToTex2pbrColors(readableTexture.GetPixels());

            regenThread = new ThreadedJob(() =>
            {
                workingProgress = 0f;

                #region albedoTextures
                ThreadedJob t_albedo_lowNoiseRemoval_lowShadowRemoval = new ThreadedJob(() =>
               {
                   ImageProcessing.MedianFilter(rawPixels, textureWidth, textureHeight, 0f, out albedo_LowNoiseRemoval_LowShadowRemoval);
                   ImageProcessing.RemoveShadowsAndHighlight(albedo_LowNoiseRemoval_LowShadowRemoval, 0f, out albedo_LowNoiseRemoval_LowShadowRemoval);
               });
               ThreadedJob t_albedo_highNoiseRemoval_lowShadowRemoval = new ThreadedJob(() =>
               {
                   ImageProcessing.MedianFilter(rawPixels, textureWidth, textureHeight, 1f, out albedo_HighNoiseRemoval_LowShadowRemoval);
                   ImageProcessing.RemoveShadowsAndHighlight(albedo_HighNoiseRemoval_LowShadowRemoval, 0f, out albedo_HighNoiseRemoval_LowShadowRemoval);
               });
               ThreadedJob t_albedo_lowNoiseRemoval_highShadowRemoval = new ThreadedJob(() =>
               {
                   ImageProcessing.MedianFilter(rawPixels, textureWidth, textureHeight, 0f, out albedo_LowNoiseRemoval_HighShadowRemoval);
                   ImageProcessing.RemoveShadowsAndHighlight(albedo_LowNoiseRemoval_HighShadowRemoval, 1f, out albedo_LowNoiseRemoval_HighShadowRemoval);
               });
               ThreadedJob t_albedo_highNoiseRemoval_highShadowRemoval = new ThreadedJob(() =>
               {
                   ImageProcessing.MedianFilter(rawPixels, textureWidth, textureHeight, 1f, out albedo_HighNoiseRemoval_HighShadowRemoval);
                   ImageProcessing.RemoveShadowsAndHighlight(albedo_HighNoiseRemoval_HighShadowRemoval, 1f, out albedo_HighNoiseRemoval_HighShadowRemoval);
               });

                #endregion

                #region heightTextures
                float[] heightBase = new float[0];
                ThreadedJob t_height_base = new ThreadedJob(() =>
                {
                    float[] grayscale;
                    ImageProcessing.Grayscale(albedo_HighNoiseRemoval_LowShadowRemoval, out grayscale);
                    ImageProcessing.AutoContrast(grayscale, out heightBase);
                });

                ThreadedJob t_height_sharp = new ThreadedJob(() =>
                {
                    ImageProcessing.SurfaceBlur(heightBase, textureWidth, textureHeight, 2, out heightmap_sharp);
                });
                ThreadedJob t_height_smooth = new ThreadedJob(() =>
                {
                    ImageProcessing.Blur(heightBase, textureWidth, textureHeight, 1, out heightmap_smooth);
                    ImageProcessing.SurfaceBlur(heightmap_smooth, textureWidth, textureHeight, 4, out heightmap_smooth);

                });
                #endregion

                #region normalTextures
                ThreadedJob t_normal_noDetails = new ThreadedJob(() =>
               {
                   ImageProcessing.NormalMap(heightmap_smooth, textureWidth, textureHeight, 2, 2f, out normal_NoDetails);
               });
               ThreadedJob t_normal_highDetails = new ThreadedJob(() =>
               {
                   float[] grayscaleNormal;
                   ImageProcessing.Grayscale(rawPixels, out grayscaleNormal);
                   ImageProcessing.NormalMap(grayscaleNormal, textureWidth, textureHeight, 0, 1f, out normal_HighDetails);
               });
                #endregion

                #region occlusionTextures
                ThreadedJob t_occlusion_lowSpread = new ThreadedJob(() =>
               {
                   ImageProcessing.CalculateOcclusion(heightmap_smooth, textureWidth, textureHeight, 5, out occlusion_LowSpread);
               });
               ThreadedJob t_occlusion_highSpread = new ThreadedJob(() =>
               {
                   ImageProcessing.CalculateOcclusion(heightmap_smooth, textureWidth, textureHeight, 15, out occlusion_HighSpread);
               });
                #endregion

                #region metallicTextures
                ThreadedJob t_metallic = new ThreadedJob(() =>
               {
                   ImageProcessing.CalculateMetallic(heightmap_smooth, normal_NoDetails, 0.5f, out metallic_Low);
                   ImageProcessing.Blur(metallic_Low, textureWidth, textureHeight, 1, out metallic_Low);
                   ImageProcessing.AutoContrast(metallic_Low, out metallic_High);
               });
                #endregion

                //we can start albo rightaway
                t_albedo_lowNoiseRemoval_lowShadowRemoval.Start();
                t_albedo_highNoiseRemoval_lowShadowRemoval.Start();
                t_albedo_lowNoiseRemoval_highShadowRemoval.Start();
                t_albedo_highNoiseRemoval_highShadowRemoval.Start();
                //High details grayscale can also be started rightaway
                t_normal_highDetails.Start();

                workingProgress = 0.1f;

                //t_height_base needs albedo_highNoiseRemoval_highShadowRemoval, so wait for that to be done
                while (!t_albedo_highNoiseRemoval_lowShadowRemoval.isDone) Thread.Sleep(1);
                t_height_base.Start();

                workingProgress = 0.2f;

                //t_height_sharp and t_height_smooth needs height_base, so wait for that to finish
                while (!t_height_base.isDone) Thread.Sleep(1);
                t_height_sharp.Start();
                t_height_smooth.Start();

                workingProgress = 0.4f;

                //t_normal_noDetails, t_occlusion_lowSpread, and t_occlusion_highSpread needs heightmap smooth to be finished
                while (!t_height_smooth.isDone) Thread.Sleep(1);
                t_normal_noDetails.Start();
                t_occlusion_lowSpread.Start();
                t_occlusion_highSpread.Start();

                workingProgress = 0.6f;

                //metallic needs normal
                while (!t_normal_noDetails.isDone) Thread.Sleep(1);
                t_metallic.Start();

                workingProgress = 0.8f;

                //Wait for any remaning threads to finish
                while (!t_albedo_highNoiseRemoval_highShadowRemoval.isDone) Thread.Sleep(1);
                while (!t_albedo_lowNoiseRemoval_highShadowRemoval.isDone) Thread.Sleep(1);
                while (!t_albedo_highNoiseRemoval_lowShadowRemoval.isDone) Thread.Sleep(1);
                while (!t_albedo_lowNoiseRemoval_lowShadowRemoval.isDone) Thread.Sleep(1);
                while (!t_normal_highDetails.isDone) Thread.Sleep(1);
                while (!t_height_sharp.isDone) Thread.Sleep(1);
                while (!t_occlusion_lowSpread.isDone) Thread.Sleep(1);
                while (!t_occlusion_highSpread.isDone) Thread.Sleep(1);
                while (!t_metallic.isDone) Thread.Sleep(1);

                workingProgress = 1f;
            });
            regenThread.onDone = _onDone;
            regenThread.Start();
        }
        
        void SetTexture(Texture2D texture, float[] pixels)
        {
            Color[] colorPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                colorPixels[i] = new Color(pixels[i], pixels[i], pixels[i]);
            }
            texture.SetPixels(colorPixels);
            texture.Apply(true);
        }
        void SetTexture(Texture2D texture, Tex2pbrColor[] pixels)
        {
            Color[] colorPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                colorPixels[i] = pixels[i].GetUnityColor();
            }
            texture.SetPixels(colorPixels);
            texture.Apply(true);
        }

        Tex2pbrColor[] UnityColorsToTex2pbrColors(Color[] input)
        {
            Tex2pbrColor[] output = new Tex2pbrColor[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = new Tex2pbrColor(input[i]);
            }
            return output;
        }
    }

    [System.Serializable]
    public class Tex2pbrColor
    {
        public float r;
        public float g;
        public float b;
        
        public Tex2pbrColor()
        {
            r = 0f;
            g = 0f;
            b = 0f;
        }

        public Tex2pbrColor(float _r, float _g, float _b)
        {
            r = _r;
            g = _g;
            b = _b;
        }

        public Tex2pbrColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
        }

        public Color GetUnityColor()
        {
            return new Color(r, g, b);
        }
    }
    public class ThreadedJob
    {
        public Action onDone;
        Action job;

        bool _isDone = false;
        object handle = new object();
        Thread thread = null;
        public bool isDone
        {
            get
            {
                bool tmp;
                lock (handle)
                {
                    tmp = _isDone;
                }
                return tmp;
            }
            set
            {
                lock (handle)
                {
                    _isDone = value;
                }
            }
        }

        public ThreadedJob(Action _job)
        {
            job = _job;
        }

        public void Start()
        {
            isDone = false;
            thread = new Thread(Run);
            thread.Start();

            //Run();
        }
        public void Abort()
        {
            thread.Abort();
        }
        
        private void Run()
        {
            if (job != null)
            {
                job();
            }

            isDone = true;
        }
        
    }
}
