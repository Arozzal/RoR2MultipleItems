using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx;
using R2API.Utils;
using System.Reflection;
using R2API.MiscHelpers;
using System.Windows;
using UnityEngine.UI;


namespace RoR2ModTest
{
    
    class StatsUI
    {
        static GameObject objRef = null;
        static Transform canRoot = null;
        private static string statsString = "";

        public static void initStatsDisplay(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            objRef = new GameObject("AnotherGameObject");
            canRoot = self.transform.GetChild(0);

            objRef.transform.SetParent(canRoot);
            objRef.transform.SetAsLastSibling();

            for(int i = 0; i < ModTest.biggestStage.Count; i++)
            {
                ModTest.biggestStage[i] = 0;
            }
            

            objRef.AddComponent<RectTransform>();
            objRef.GetComponent<RectTransform>().anchorMin = new Vector2(0.9f, 0.2f);
            objRef.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f, 0.6f);
            objRef.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            objRef.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            
            objRef.AddComponent<Text>();
            objRef.GetComponent<Text>().text = statsString;
            objRef.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        }


        public static void addStats(string name, string value, string color)
        {
            statsString += "<color=#" + color + ">" +  name + ": " + value + "</color>" + "\n";
        }

        public static void addSingleValue(string name, string color)
        {
            statsString += "<b>" + "<color=#" + color + ">" + name + "</color>" + "</b>" + "\n";
        }

        public static void applyValues()
        {
            objRef.GetComponent<Text>().text = statsString;
            statsString = "";
        }


        public static void addEmptyLine()
        {
            statsString += '\n';
        }
    }
}
