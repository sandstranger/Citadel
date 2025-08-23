using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Tests {
    public class TestSaveLoad {
        private bool sceneLoaded = false;

        //private IEnumerator LoadSceneAsync(string sceneName) {
        //    var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        //    while (!asyncLoad.isDone) {
        //        yield return null;
        //    }
        //}

        public void RunBeforeAnyTests() {
            if (sceneLoaded) return;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("CitadelScene");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name == "CitadelScene") {
                // Unsubscribe from the event to avoid handling it multiple
                // times
                SceneManager.sceneLoaded -= OnSceneLoaded;
                sceneLoaded = true; // Indicate that the scene is loaded.
            }
        }
        
       
        private static int GetTotalDescendantCount(GameObject go) {
            int count = 0;
            foreach (Transform child in go.transform) {
                count++; // Count the direct child
                count += GetTotalDescendantCount(child.gameObject); // Recursively count its descendants
            }
            
            return count;
        }
        
        private string WriteHierarchyCounts() {
            List<GameObject> allParents = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
            int childCount = 0;
            GameObject childGO = null;
            StringBuilder s1 = new StringBuilder();
            s1.Append(Environment.NewLine);
            s1.Append("GameObject Hierarchy Counts::");
            for (int i=0;i<allParents.Count;i++) {
                s1.Append(Environment.NewLine);
                if (allParents[i] == null) continue;
                
                s1.Append(allParents[i].name);
                childCount = allParents[i].transform.childCount;
                if (childCount < 1) {  s1.Append(":0"); continue; }
                
                s1.Append(":");
                s1.Append(childCount.ToString());
                for (int j=0;j<childCount;j++) {
                    s1.Append(Environment.NewLine);
                    childGO = allParents[i].transform.GetChild(j).gameObject;
                    if (childGO == null) continue;
                    
                    s1.Append(allParents[i].name);
                    s1.Append(":");
                    s1.Append(childGO.name);
                    s1.Append(":");
                    int level2CountOfAllContained = GetTotalDescendantCount(childGO);
                    s1.Append(level2CountOfAllContained.ToString()); // 
                }
            }
            
            s1.Append(Environment.NewLine); // EOF newline for Linux systems.
            return s1.ToString();
        }
        
    }
}
