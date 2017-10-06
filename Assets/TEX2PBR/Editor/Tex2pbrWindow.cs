using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tex2pbr
{
    public class Tex2pbrWindow : EditorWindow
    {
        [SerializeField]
        Texture2D logo;

        [SerializeField]
        TextureGenerator textureGenerator;
        
        public Texture2D baseTexture;

        [SerializeField]
        Material preview;
        Editor previewEditor;

        [SerializeField]
        int selectedTab = 0;
        
        [SerializeField]
        Texture2D albedo;
        [SerializeField]
        Texture2D height;
        [SerializeField]
        Texture2D normal;
        [SerializeField]
        Texture2D occlusion;
        [SerializeField]
        Texture2D metallic;
        


        [SerializeField]
        public GenerationSettings generationSettings;

        Texture loadingIcon;

        [MenuItem("Window/TEX2PBR")]
        static void Init()
        {
            Tex2pbrWindow window = (Tex2pbrWindow)EditorWindow.GetWindow(typeof(Tex2pbrWindow));

            window.titleContent = new GUIContent("TEX2PBR", Resources.Load<Texture2D>("iconTiny"));
            window.name = "TEX2PBR";
            window.minSize = new Vector2(710, 580);
            window.Show();

            window.logo = (Texture2D)Resources.Load("logoSmall");
        }
        
        void OnEnable()
        {
            if (generationSettings == null) generationSettings = new GenerationSettings();

            if (textureGenerator == null)
                textureGenerator = new TextureGenerator();
        }

        void Update()
        {
            textureGenerator.Update();
            Repaint();

            string[] iconNames = new string[] { "WaitSpin00", "WaitSpin01", "WaitSpin02", "WaitSpin03", "WaitSpin04", "WaitSpin05", "WaitSpin06", "WaitSpin07", "WaitSpin08", "WaitSpin09", "WaitSpin10", "WaitSpin11" };
            loadingIcon = EditorGUIUtility.FindTexture(iconNames[(int)Mathf.Repeat(Mathf.RoundToInt((float)EditorApplication.timeSinceStartup * 10), 11)]);

        }

        void OnBaseTextureChange()
        {

            previewEditor = null;
            preview = null;

            Material exsistingMat = null;
            if (baseTexture != null)
            {
                exsistingMat = AssetDatabase.LoadAssetAtPath<Material>(Path.ChangeExtension(AssetDatabase.GetAssetPath(baseTexture), null) + ".mat");
            }
            if (exsistingMat != null)
            {
                preview = new Material(exsistingMat);
            }
            else
            {
                preview = new Material(Shader.Find("Standard"));
                preview.SetFloat("_GlossMapScale", 0.2f);
                preview.SetFloat("_BumpScale", 1f);
                preview.SetFloat("_Parallax", 0.05f);
                preview.SetFloat("_OcclusionStrength", 1f);
            }

            preview.EnableKeyword("_NORMALMAP");
            preview.EnableKeyword("_PARALLAXMAP");
            preview.EnableKeyword("_METALLICGLOSSMAP");

            textureGenerator.ReGenerate(baseTexture, OnTextureGeneratorDone);
        }

        #region gui

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            SelectedViewToolbar();
            MaterialView();

            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(300));

            SideBar();
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void SelectedViewToolbar()
        {
            GUILayout.Space(7);
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            selectedTab = GUILayout.Toolbar(selectedTab, new GUIContent[6] 
            {
                TabContent("Material", false),
                TabContent("Albedo", false),
                TabContent("Height", false),
                TabContent("Normal", false),
                TabContent("Occlusion", false),
                TabContent("Metallic", false)
            });
            GUILayout.EndHorizontal();
            GUILayout.Space(-6);
        }
        GUIContent TabContent(string tabName, bool isLoading)
        {
            Texture icon = null;

            if (isLoading)
            {
                icon = loadingIcon;
                tabName = " " + tabName;
            }

            GUIContent content = new GUIContent(tabName, icon);
            return content;
        }

        Rect content;
        void MaterialView()
        {

            if (baseTexture == null) return; //Nothing to show

            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint) //GUILayoutUtility.GetLastRect() is only correct when event is repaint
            {
                content = GUILayoutUtility.GetLastRect();
            }
            
            if (selectedTab == 0)
            {
                if (previewEditor == null && preview != null)
                    previewEditor = Editor.CreateEditor(preview);
                

                if (previewEditor != null)
                {
                    float previewSettingsHeight = 16f;

                    GUILayout.BeginArea(new Rect(content.x, content.y + content.height - previewSettingsHeight, content.width, previewSettingsHeight));
                    GUILayout.BeginHorizontal();
                    previewEditor.OnPreviewSettings();
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();

                    previewEditor.OnInteractivePreviewGUI(new Rect(content.x, content.y, content.width, content.height - previewSettingsHeight), EditorStyles.textArea);
                }
                //previewEditor.OnPreviewGUI(content, EditorStyles.helpBox);
            }
            else
            {
                if (textureGenerator.working)
                {
                    //float size = Mathf.Min(content.width - 10, content.height - 10);
                    content = new Rect(content.x + (content.width - loadingIcon.width) / 2f, content.y + (content.height - loadingIcon.height) / 2f, loadingIcon.width, loadingIcon.height);

                    GUI.DrawTexture(content, loadingIcon);
                }
                else
                {
                    Texture2D selectedTex = null;
                    switch (selectedTab)
                    {
                        case 0:
                            selectedTex = null;
                            break;
                        case 1:
                            selectedTex = albedo;
                            break;
                        case 2:
                            selectedTex = height;
                            break;
                        case 3:
                            selectedTex = normal;
                            break;
                        case 4:
                            selectedTex = occlusion;
                            break;
                        case 5:
                            selectedTex = metallic;
                            break;
                    }

                    if (selectedTex != null)
                    {
                        float size = Mathf.Min(content.width - 10, content.height - 10);
                        content = new Rect(content.x + (content.width - size) / 2f, content.y + (content.height - size) / 2f, size, size);

                        GUI.DrawTexture(content, selectedTex);
                    }
                }
            }
        }

        void SideBar()
        {
            GUILayout.Space(7);

            if (logo != null)
            {
                GUI.DrawTexture(GUILayoutUtility.GetRect(200, 60), logo, ScaleMode.ScaleToFit);
            }
            

            var newBase = EditorGUILayout.ObjectField("Input image", baseTexture, typeof(Texture2D), false) as Texture2D;
            if (newBase != baseTexture)
            {
                baseTexture = newBase;
                OnBaseTextureChange();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Texture generation settings", EditorStyles.boldLabel);
            if (textureGenerator.working)
            {
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, 15), textureGenerator.workingProgress, "Working");
            }
            else if (albedo == null)
            {
                GUILayout.Label("No textures, please add an input image above");
            }
            else
            {
                AlbedoSettings();
                HeightSettings();
                NormalSettings();
                OcclusionSettings();
                MetallicSettings();
            }
            GUILayout.EndVertical();


            //if (textureGenerator.working)
            //{
            //    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, 15), textureGenerator.workingProgress, "Working");
            //}
            //else
            //{
            //    if (GUILayout.Button("Generate", GUILayout.Height(30)) && baseTexture != null && IsTextureReadable(baseTexture))
            //    {
            //        textureGenerator.ReGenerate(baseTexture, OnTextureGeneratorDone);
            //    }
            //}
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Material settings", EditorStyles.boldLabel);
            if (preview != null)
            {
                preview.color = EditorGUILayout.ColorField("Color", preview.color);

                preview.SetFloat("_GlossMapScale", EditorGUILayout.Slider("Smoothness", preview.GetFloat("_GlossMapScale"), 0f, 1f));
                preview.SetFloat("_BumpScale", EditorGUILayout.Slider("Normal", preview.GetFloat("_BumpScale"), 0f, 1f));
                preview.SetFloat("_Parallax", EditorGUILayout.Slider("Height", preview.GetFloat("_Parallax"), 0f, 0.1f));
                preview.SetFloat("_OcclusionStrength", EditorGUILayout.Slider("Occlusion", preview.GetFloat("_OcclusionStrength"), 0f, 1f));

                preview.mainTextureScale = EditorGUILayout.Vector2Field("Tile", preview.mainTextureScale);
                preview.mainTextureOffset = EditorGUILayout.Vector2Field("Offset", preview.mainTextureOffset);
            }
            else
            {
                GUILayout.Label("No material");
            }
            GUILayout.EndVertical();
            
            if (GUILayout.Button("Export", GUILayout.Height(30)))
            {
                Export();
            }
        }

        void AlbedoSettings()
        {
            GUILayout.Label("Albedo", EditorStyles.centeredGreyMiniLabel);
            {
                var noiseRemovalStrength = EditorGUILayout.Slider("Noise removal", generationSettings.albedoNoiseRemovalStrength, 0f, 1f);
                var shadowRemovalStrength = EditorGUILayout.Slider("Shadow/Highlight removal", generationSettings.albedoShadowHighlightRemovalStrength, 0f, 1f);

                if (noiseRemovalStrength != generationSettings.albedoNoiseRemovalStrength  || shadowRemovalStrength != generationSettings.albedoShadowHighlightRemovalStrength)
                {
                    generationSettings.albedoNoiseRemovalStrength = noiseRemovalStrength;
                    generationSettings.albedoShadowHighlightRemovalStrength = shadowRemovalStrength;
                    OnTextureGeneratorAlbedoChanged();
                }
            }
        }
        void HeightSettings()
        {
            GUILayout.Label("Height", EditorStyles.centeredGreyMiniLabel);
            {
                var newHeightSmooth = EditorGUILayout.Slider("Smoothness", generationSettings.heightSmooth, 0f, 1f);
                
                if (newHeightSmooth != generationSettings.heightSmooth)
                {
                    generationSettings.heightSmooth = newHeightSmooth;
                    OnTextureGeneratorHeightChanged();
                }
            }
        }
        void NormalSettings()
        {
            GUILayout.Label("Normal", EditorStyles.centeredGreyMiniLabel);
            {
                var normalDetailStrength = EditorGUILayout.Slider("Details", generationSettings.normalDetailStrength, 0f, 1f);
                if (normalDetailStrength != generationSettings.normalDetailStrength)
                {
                    generationSettings.normalDetailStrength = normalDetailStrength;
                    OnTextureGeneratorNormalChanged();
                }
            }
        }
        void OcclusionSettings()
        {
            GUILayout.Label("Occlusion", EditorStyles.centeredGreyMiniLabel);
            {
                var occlusionSpread = EditorGUILayout.Slider("Spread", generationSettings.occlusionSpread, 0f, 1f);
                if (occlusionSpread != generationSettings.occlusionSpread)
                {
                    generationSettings.occlusionSpread = occlusionSpread;
                    OnTextureGeneratorOcclusionChanged();
                }
            }
        }
        void MetallicSettings()
        {
            GUILayout.Label("Metallic", EditorStyles.centeredGreyMiniLabel);
            {
                var metallicness = EditorGUILayout.Slider("Metallicness", generationSettings.metallicness, 0f, 1f);
                if (metallicness != generationSettings.metallicness)
                {
                    generationSettings.metallicness = metallicness;
                    OnTextureGeneratorMetallicChanged();
                }
            }
        }

        void Export()
        {
            //DestroyImmediate(previewEditor);

            //Save the files
            string assetPath = Path.ChangeExtension(AssetDatabase.GetAssetPath(baseTexture), null);

            File.WriteAllBytes(assetPath + "_albedo.png", albedo.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath + "_albedo.png");

            File.WriteAllBytes(assetPath + "_normal.png", normal.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath + "_normal.png");

            File.WriteAllBytes(assetPath + "_height.png", height.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath + "_height.png");

            File.WriteAllBytes(assetPath + "_occlusion.png", occlusion.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath + "_occlusion.png");

            File.WriteAllBytes(assetPath + "_metallic.png", metallic.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath + "_metallic.png");

            //Set normal texture to normalType and reimport
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath + "_normal.png");
            importer.textureType = TextureImporterType.NormalMap;
            AssetDatabase.ImportAsset(assetPath + "_normal.png");

            //Create Material
            Material material = new Material(preview);// AssetDatabase.LoadAssetAtPath<Material>(assetPath + ".mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
            }
           

            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "_albedo.png"));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "_normal.png"));
            material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "_metallic.png"));
            material.SetTexture("_OcclusionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "_occlusion.png"));
            material.SetTexture("_ParallaxMap", AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "_height.png"));

            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(material)))
                AssetDatabase.CreateAsset(material, assetPath + ".mat");

            AssetDatabase.Refresh();
            
        }
        
        #endregion

        void OnTextureGeneratorDone()
        {
            OnTextureGeneratorAlbedoChanged();
            OnTextureGeneratorHeightChanged();
            OnTextureGeneratorNormalChanged();
            OnTextureGeneratorOcclusionChanged();
            OnTextureGeneratorMetallicChanged();
        }

        public void OnTextureGeneratorAlbedoChanged()
        {
            albedo = textureGenerator.GetAlbedo(generationSettings.albedoNoiseRemovalStrength, generationSettings.albedoShadowHighlightRemovalStrength);
            preview.SetTexture("_MainTex", albedo);
        }
        public void OnTextureGeneratorHeightChanged()
        {
            height = textureGenerator.GetHeight(generationSettings.heightSmooth);
            preview.SetTexture("_ParallaxMap", height);
        }
        public void OnTextureGeneratorNormalChanged()
        {
            normal = textureGenerator.GetNormal(generationSettings.normalDetailStrength);
            preview.SetTexture("_BumpMap", textureGenerator.GetRuntimeNormal(normal));
        }
        public void OnTextureGeneratorOcclusionChanged()
        {
            occlusion = textureGenerator.GetOcclusion(generationSettings.occlusionSpread);
            preview.SetTexture("_OcclusionMap", occlusion);
        }
        public void OnTextureGeneratorMetallicChanged()
        {
            metallic = textureGenerator.GetMetallic(generationSettings.metallicness);
            preview.SetTexture("_MetallicGlossMap", metallic);
        }
        
    }
    
}
