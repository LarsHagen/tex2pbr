using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tex2pbr
{
    public class ImageProcessing
    {

        
        static int Repeat(int value, int length)
        {
            return (int)(value - (float)System.Math.Floor(value / (float)length) * length + 0.5f);
        }

        public static void Grayscale(Tex2pbrColor[] pixels, out float[] grayscale)
        {
            grayscale = new float[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                grayscale[i] = (pixels[i].r + pixels[i].g + pixels[i].b) / 3f;
            }
            //return grayscale;
        }

        public static void MedianFilter(float[] grayscale, int width, int height, float strength, out float[] returnArray)
        {
            returnArray = new float[grayscale.Length];

            float[] neighbors = new float[25];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int neighborIndex = 0;

                    for (int yy = -2; yy <= 2; yy++)
                    {
                        for (int xx = -2; xx <= 2; xx++)
                        {
                            int realX = (x + xx) % width;
                            int realY = (y + yy) % height;

                            int i = realY * width + realX;

                            neighbors[neighborIndex] = grayscale[i];
                            neighborIndex++;
                        }
                    }

                    System.Array.Sort(neighbors);
                    float value = neighbors[12];
                    returnArray[y * width + x] = Lerp(grayscale[y * width + x], value, strength);
                }
            }
        }
        public static void MedianFilter(Tex2pbrColor[] pixels, int width, int height, float strength, out Tex2pbrColor[] returnArray)
        {
            returnArray = new Tex2pbrColor[pixels.Length];

            Tex2pbrColor[] neighbors = new Tex2pbrColor[25];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int neighborIndex = 0;

                    for (int yy = -2; yy <= 2; yy++)
                    {
                        for (int xx = -2; xx <= 2; xx++)
                        {
                            int realX = Repeat(x + xx, width);
                            int realY = Repeat(y + yy, height);

                            int i = realY * width + realX;
                            
                            neighbors[neighborIndex] = pixels[i];
                            neighborIndex++;
                        }
                    }

                    System.Array.Sort(neighbors, (a, b) => (a.r + a.g + a.b).CompareTo(b.r + b.g + b.b));
                    Tex2pbrColor value = neighbors[12];
                    returnArray[y * width + x] = Lerp(pixels[y * width + x], value, strength);
                }
            }
        }

        public static void SurfaceBlur(float[] grayscale, int width, int height, int strength, out float[] returnArray)
        {
            returnArray = new float[grayscale.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int numNeighbors = 0;

                    float startValue = grayscale[y * width + x];
                    float newValue = 0;

                    for (int yy = -strength; yy <= strength; yy++)
                    {
                        for (int xx = -strength; xx <= strength; xx++)
                        {
                            int realX = Repeat(x + xx, width);
                            int realY = Repeat(y + yy, height);

                            int i = realY * width + realX;

                            if (System.Math.Abs(grayscale[i] - startValue) < 0.1)
                            {
                                newValue += grayscale[i];
                                numNeighbors++;
                            }
                            else
                            {
                                //newValue += startValue;
                                //numNeighbors++;
                            }


                        }
                    }

                    newValue /= (float)numNeighbors;

                    returnArray[y * width + x] = newValue;
                }
            }
        }
        public static void SurfaceBlur(Tex2pbrColor[] pixels, int width, int height, int strength, out Tex2pbrColor[] returnArray)
        {
            returnArray = new Tex2pbrColor[pixels.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int numNeighbors = 0;

                    Tex2pbrColor startValue = pixels[y * width + x];
                    Tex2pbrColor newValue = new Tex2pbrColor();

                    for (int yy = -strength; yy <= strength; yy++)
                    {
                        for (int xx = -strength; xx <= strength; xx++)
                        {
                            int realX = Repeat(x + xx, width);
                            int realY = Repeat(y + yy, height);

                            if (realX > -1 && realX < width && realY > -1 && realY < height)
                            {
                                int i = realY * width + realX;

                                if (System.Math.Abs(pixels[i].r - startValue.r) < 0.1 &&
                                    System.Math.Abs(pixels[i].g - startValue.g) < 0.1 &&
                                    System.Math.Abs(pixels[i].b - startValue.b) < 0.1)
                                {
                                    //newValue += pixels[i];
                                    newValue.r += pixels[i].r;
                                    newValue.g += pixels[i].g;
                                    newValue.b += pixels[i].b;

                                    numNeighbors++;
                                }
                                else
                                {
                                    //newValue += startValue;
                                    //numNeighbors++;
                                }
                            }

                        }
                    }

                    //newValue /= (float)numNeighbors;
                    newValue.r /= (float)numNeighbors;
                    newValue.g /= (float)numNeighbors;
                    newValue.b /= (float)numNeighbors;

                    returnArray[y * width + x] = newValue;
                }
            }
        }

        public static void Blur(float[] grayscale, int width, int height, int strength, out float[] returnArray)
        {
            /*returnArray = new float[grayscale.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int numNeighbors = 0;

                    float startValue = grayscale[y * width + x];
                    float newValue = 0;

                    for (int yy = -strength; yy <= strength; yy++)
                    {
                        for (int xx = -strength; xx <= strength; xx++)
                        {
                            int realX = Repeat(x + xx, width);
                            int realY = Repeat(y + yy, height);

                            int i = realY * width + realX;

                            newValue += grayscale[i];
                            numNeighbors++;
                        }
                    }

                    newValue /= (float)numNeighbors;

                    returnArray[y * width + x] = newValue;
                }
            }*/

            float[] kernel = new float[25]
            {
                1,4,7,4,1,
                4,16,26,16,4,
                7,26,41,26,7,
                4,16,26,16,4,
                1,4,7,4,1
            };

            returnArray = ApplyKernel(grayscale, kernel, width, height, 5, 5);
        }

        public static void Invert(float[] grayscale, out float[] returnArray)
        {
            returnArray = new float[grayscale.Length];
            for (int i = 0; i < grayscale.Length; i++)
            {
                returnArray[i] = 1f - grayscale[i];
            }
        }

        public static void NormalMap(float[] grayscale, int width, int height, int smoothness, float flatness, out Tex2pbrColor[] returnArray)
        {
            Tex2pbrColor[] initialNormalMap = new Tex2pbrColor[grayscale.Length];

            float[] neighbors = new float[9];

            //Initial normal map generation
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int neighborIndex = 0;

                    float value = grayscale[y * width + x];

                    for (int yy = -1; yy <= 1; yy++)
                    {
                        for (int xx = -1; xx <= 1; xx++)
                        {
                            int realX = Repeat(x + xx, width);
                            int realY = Repeat(y + yy, height);

                            int i = realY * width + realX;

                            neighbors[neighborIndex] = grayscale[i];

                            neighborIndex++;
                        }
                    }

                    Vector3 normal = new Vector3();
                    normal.x = 5f * -(neighbors[2] - neighbors[0] + 2f * (neighbors[5] - neighbors[3]) + neighbors[8] - neighbors[6]);
                    normal.y = 5f * -(neighbors[6] - neighbors[0] + 2f * (neighbors[7] - neighbors[1]) + neighbors[8] - neighbors[2]);
                    normal.z = flatness - value;
                    normal.Normalize();



                    initialNormalMap[y * width + x] = new Tex2pbrColor((normal.x + 1f) / 2f, (normal.y + 1f) / 2f, (normal.z + 1f) / 2f);
                }
            }

            //Surface blur normals based on heightmap
            returnArray = new Tex2pbrColor[grayscale.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int numNeighbors = 0;

                    float startValue = grayscale[y * width + x];
                    float newR = 0;
                    float newG = 0;
                    float newB = 0;

                    for (int yy = -smoothness; yy <= smoothness; yy++)
                    {
                        for (int xx = -smoothness; xx <= smoothness; xx++)
                        {
                            int realX = Repeat(x + xx, width);
                            int realY = Repeat(y + yy, height);

                            int i = realY * width + realX;

                            if (System.Math.Abs(grayscale[i] - startValue) < 0.2)
                            {
                                newR += initialNormalMap[i].r;
                                newG += initialNormalMap[i].g;
                                newB += initialNormalMap[i].b;
                                numNeighbors++;
                            }
                            else
                            {
                                //newValue += startValue;
                                //numNeighbors++;
                            }


                        }
                    }

                    newR /= (float)numNeighbors;
                    newG /= (float)numNeighbors;
                    newB /= (float)numNeighbors;

                    returnArray[y * width + x] = new Tex2pbrColor(newR, newG, newB);
                }
            }
        }

        public static void AutoContrast(float[] grayscale, out float[] returnArray)
        {
            returnArray = new float[grayscale.Length];
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i < grayscale.Length; i++)
            {
                if (grayscale[i] > max) max = grayscale[i];
                if (grayscale[i] < min) min = grayscale[i];
            }
            for (int i = 0; i < grayscale.Length; i++)
            {
                returnArray[i] = (grayscale[i] - min) / (max - min);
            }
        }

        public static void RemoveShadowsAndHighlight(Tex2pbrColor[] pixels, float strength, out Tex2pbrColor[] returnArray)
        {
            returnArray = new Tex2pbrColor[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                float lightLevel = (pixels[i].r + pixels[i].g + pixels[i].b) / 3f;
                //float affectValue = (lightLevel - 0.5f) / 3f;
                float affectValue = 0f;

                if (lightLevel > 0.7f)
                    affectValue = (lightLevel - 0.7f);// / 2.5f;
                if (lightLevel < 0.3f)
                    affectValue = (lightLevel - 0.3f);// / 2.5f;

                affectValue *= strength;

                returnArray[i] = new Tex2pbrColor(pixels[i].r - affectValue, pixels[i].g - affectValue, pixels[i].b - affectValue);

            }
        }

        public static void GrayscaleToPixels(float[] grayscale, out Tex2pbrColor[] pixels)
        {
            pixels = new Tex2pbrColor[grayscale.Length];
            for (int i = 0; i < grayscale.Length; i++)
            {
                pixels[i] = new Tex2pbrColor(grayscale[i], grayscale[i], grayscale[i]);
            }
        }

        public static Tex2pbrColor[] Blend(Tex2pbrColor[] input1, Tex2pbrColor[] input2, float t)
        {
            Tex2pbrColor[] returnArray = new Tex2pbrColor[input1.Length];
            for (int i = 0; i < input1.Length; i++)
            {
                returnArray[i] = Lerp(input1[i], input2[i], t);
            }
            return returnArray;
        }

        public static float[] Blend(float[] input1, float[] input2, float t)
        {
            float[] returnArray = new float[input1.Length];
            for (int i = 0; i < input1.Length; i++)
            {
                returnArray[i] = Lerp(input1[i], input2[i], t);
            }
            return returnArray;
        }

        public static void PickLowest(float[] grayscale1, float[] grayscale2, out float[] returnarray)
        {
            returnarray = new float[grayscale1.Length];
            for (int i = 0; i < grayscale1.Length; i++)
            {
                returnarray[i] = grayscale1[i] < grayscale2[i] ? grayscale1[i] : grayscale2[i];
            }
        }

        public static void CalculateOcclusion(float[] pixels, int width, int height, int spread, out float[] returnArray)
        {
            returnArray = new float[pixels.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = 0f;

                    float startValue = pixels[y * width + x];

                    int num = 0;

                    for (int xx = -spread; xx < spread; xx++)
                    {
                        for (int yy = -spread; yy < spread; yy++)
                        {
                            int realX = Repeat(x + xx, width);
                            int realY = Repeat(y + yy, height);
                            int i = realY * width + realX;


                            value += pixels[i];
                            num++;
                        }
                    }

                    value /= (float)num;
                    value = (startValue - value) * 4f + 0.9f;
                    if (value < 0f) value = 0f;
                    if (value > 1f) value = 1f;
                    
                    returnArray[y * width + x] = value;
                    
                }
            }
        }

        public static void CalculateMetallic(float[] height, Tex2pbrColor[] normal, float strength, out float[] returnArray)
        {
            float[] step1 = new float[height.Length];
            for (int i = 0; i < height.Length; i++)
            {
                float angle = Vector3.Angle(new Vector3(0, 0, 1), new Vector3(normal[i].r - 0.5f, normal[i].g - 0.5f, normal[i].b));
                angle = (90 - angle);
                step1[i] = angle * height[i];
            }

            AutoContrast(step1, out returnArray);

            for (int i = 0; i < returnArray.Length; i++)
            {
                returnArray[i] = Lerp((float)System.Math.Pow(returnArray[i], 2f), 0f, strength);
            }
        }

        static float Lerp(float a, float b, float t)
        {
            return a * (1f - t) + b * t;
        }
        static Tex2pbrColor Lerp(Tex2pbrColor a, Tex2pbrColor b, float t)
        {
            return new Tex2pbrColor(Lerp(a.r, b.r, t), Lerp(a.g, b.g, t), Lerp(a.b, b.b, t));
        }
        
        public static float[] ApplyKernel(float[] pixels, float[] kernel, int imageWidth, int imageHeight, int kernelWidth, int kernelHeight)
        {
            float strength = 0;
            foreach (var v in kernel)
                strength += v;

            float[] returnArray = new float[pixels.Length];

            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    float value = 0f;
                    for (int kernelX = 0; kernelX < kernelWidth; kernelX++)
                    {
                        for (int kernelY = 0; kernelY < kernelHeight; kernelY++)
                        {
                            int realX = Mathf.RoundToInt(Mathf.Repeat(x + (kernelY - kernelHeight / 2), imageWidth));
                            int realY = Mathf.RoundToInt(Mathf.Repeat(y + (kernelX - kernelWidth / 2), imageHeight));
                            int i = realY * imageWidth + realX;

                            int kernelI = kernelY * kernelWidth + kernelX;

                            value += pixels[i] * kernel[kernelI];
                        }
                    }
                    returnArray[y * imageWidth + x] = value / strength;
                }
            }

            return returnArray;
        }

    }
}
