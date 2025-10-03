using System.IO;
using UnityEditor;
using UnityEngine;

namespace Citadel.Android.Tools
{
    internal static class MeshTools
    {
        [MenuItem("Tools/Disable read-write property on created lods")]
        private static void DisableReadWritePropertyOnCreatedLods()
        {
            var pathsToLodsAssets = Directory.GetFiles(Path.Combine("Assets", "Resources", "Prefabs"),"*lods.asset", SearchOption.TopDirectoryOnly);

            foreach (var pathToLodAsset in pathsToLodsAssets)
            {
                DisableMeshReadWrite(pathToLodAsset);
            }
            
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private static void DisableMeshReadWrite(string meshAssetPath)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);
            if (mesh != null)
            {
                MeshUtility.SetMeshCompression(mesh, ModelImporterMeshCompression.Off);
                mesh.UploadMeshData(true);
                EditorUtility.SetDirty(mesh);
            }
        }
    }
}