using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tex2pbr
{
    public class Tex2pbrAboutWindow : EditorWindow
    {
        [SerializeField]
        Texture2D logo;
        [SerializeField]
        string info;

        [SerializeField]
        Vector2 scroll;

        [MenuItem("Help/About TEX2PBR...")]
        static void Init()
        {
            Tex2pbrAboutWindow window = (Tex2pbrAboutWindow)EditorWindow.GetWindow(typeof(Tex2pbrAboutWindow), true);
            window.titleContent = new GUIContent("About TEX2PBR", Resources.Load<Texture2D>("iconTiny"));

            
            window.maxSize = new Vector2(400f, 400f);
            window.minSize = window.maxSize;

            window.Show();

            window.logo = Resources.Load<Texture2D>("logo");

            window.info = System.IO.File.ReadAllText(Path.Combine(AssetDatabase.GetAssetPath(window.logo), @"..\..\..\README.TXT"));
        }
        
        void OnGUI()
        {
            GUILayout.Space(10);
            GUI.DrawTexture(GUILayoutUtility.GetRect(500, 120), logo, ScaleMode.ScaleToFit);
            GUILayout.Space(10);

            scroll = GUILayout.BeginScrollView(scroll);
            //bool temp = GUI.skin.textArea.wordWrap;
            //GUI.skin.textArea.wordWrap = true;
            EditorGUILayout.TextArea(info, GUI.skin.textArea, GUILayout.Width(this.minSize.x - 25));
            //GUI.skin.textArea.wordWrap = temp;
            GUILayout.EndScrollView();

            /*EditorGUILayout.HelpBox(
                "Version 0.1.0 - Copyright 2017\n" +
                "\n" + 
                "Feedback:\n" +
                " - mail:\tmail@laitch.com\n" + 
                " - forum:\tN/A"
                
                , MessageType.Info);*/


        }
        
    }
    
}
