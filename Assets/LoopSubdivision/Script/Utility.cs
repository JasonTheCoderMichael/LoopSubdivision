using System.IO;
using UnityEditor;
using UnityEngine;

namespace LoopSubdivision
{
    public class Utility
    {
        public static bool Vector3Equal(Vector3 v1, Vector3 v2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!FloatEqual(v1[i], v2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool FloatEqual(float f1, float f2)
        {
            return Mathf.Abs(f1 - f2) < Mathf.Epsilon;
        }

        // 从小到大排序 //
        public static void Sort(ref int value0, ref int value1, ref int value2)
        {
            int min = Mathf.Min(value0, Mathf.Min(value1, value2));
            int max = Mathf.Max(value0, Mathf.Max(value1, value2));
            int medium = value0 + value1 + value2 - min - max;
            value0 = min;
            value1 = medium;
            value2 = max;
        }

        public static bool IsOnSameEdge(NewVertexInfo info0, NewVertexInfo info1)
        {
            return info0 != null && info1 != null &&
                   (info0.linkedRealVertexIndex0 == info1.linkedRealVertexIndex0 &&
                    info0.linkedRealVertexIndex1 == info1.linkedRealVertexIndex1 ||
                    info0.linkedRealVertexIndex0 == info1.linkedRealVertexIndex1 &&
                    info0.linkedRealVertexIndex1 == info1.linkedRealVertexIndex0); 
            
            // 等同于以下逻辑，就是有点啰嗦 //
            // if (info0 != null && info1 != null)
            // {
            //     int index00 = info0.linkedRealVertexIndex0;
            //     int index01 = info0.linkedRealVertexIndex1;
            //     int index10 = info1.linkedRealVertexIndex0;
            //     int index11 = info1.linkedRealVertexIndex1;
            //     if (index00 == index10 && index01 == index11 ||
            //         index00 == index11 && index01 == index10)
            //     {
            //         return true;
            //     }
            // }
            // return false;
        }
        
        private static int[] m_separatorIndexs = new int[10];    // 最大10层深度文件夹 //
        public static void CreateDirectoryRecursively(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            int maxFolderDepth = 0;
            filePath = filePath.Replace("\\", "/");
            for (int i = 0; i < filePath.Length; i++)
            {
                if (filePath[i] == '/')
                {
                    m_separatorIndexs[maxFolderDepth] = i;
                    maxFolderDepth++;
                }
            }

            int curFolderDepth = 0;
            while (curFolderDepth < maxFolderDepth)
            {
                string folderPath = filePath.Substring(0, m_separatorIndexs[curFolderDepth]);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                curFolderDepth++;
            }
        }

        public static void SetParent(Transform child, Transform parent)
        {
            if (child == null || parent == null)
            {
                return;
            }
            
            child.SetParent(parent);
            child.localPosition = Vector3.zero;
            child.localEulerAngles = Vector3.zero;
            child.localScale = Vector3.one;
        }
        
        public static void Save(Mesh mesh, string name)
        {
            if (mesh == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            string filePath = "Assets/LoopSubdivision/ResultMesh/" + name + ".asset";
            CreateDirectoryRecursively(filePath);

            if (File.Exists(filePath))
            {
                AssetDatabase.DeleteAsset(filePath);
                AssetDatabase.Refresh();
            }
            AssetDatabase.CreateAsset(mesh, filePath);
        }
    }
}