using System;
using UnityEditor;
using UnityEngine;

namespace StencilShadowGenerator.Core.Editor
{
    [CustomEditor(typeof(ShadowVolume))]
    [CanEditMultipleObjects]
    public class ShadowVolumeEditor : UnityEditor.Editor
    {
        private ShadowVolume _instance;
        private SerializedProperty _preGenerateMesh;
        private SerializedProperty _preGeneratedMesh;
        private SerializedProperty _extrudeDistance;
        private SerializedProperty _twoManifold;
        private SerializedProperty _shadowBias;

        private void OnEnable()
        {
            _instance = target as ShadowVolume;
            _preGenerateMesh = serializedObject.FindProperty("preGenerateMesh");
            _preGeneratedMesh = serializedObject.FindProperty("preGeneratedMesh");
            _extrudeDistance = serializedObject.FindProperty("extrudeDistance");
            _twoManifold = serializedObject.FindProperty("isTwoManifold");
            _shadowBias = serializedObject.FindProperty("shadowBias");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_extrudeDistance);
            EditorGUILayout.PropertyField(_shadowBias);
            EditorGUILayout.PropertyField(_preGenerateMesh);
            if (_preGenerateMesh.boolValue)
            {
                EditorGUILayout.PropertyField(_preGeneratedMesh);
                if (GUILayout.Button("Generate New Mesh"))
                {
                    string defaultName = _instance.GetComponent<MeshFilter>().sharedMesh.name;
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Select a location to save Mesh file...",
                        $"{defaultName}_ShadowVolumeMesh", "mesh", "");
                    if (String.IsNullOrEmpty(path))
                    {
                        Debug.LogWarning("Mesh path not chosen. Failed to generate.");
                        return;
                    }
                    EditorUtility.DisplayProgressBar("Generating Mesh", "Calculating Volume", 0.25f);
                    Mesh mesh = _instance.GenerateMesh();
                    EditorUtility.DisplayProgressBar("Generating Mesh", "Saving mesh to disk", 0.85f);
                    AssetDatabase.CreateAsset(mesh, path);
                    AssetDatabase.SaveAssets();
                    mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    _preGeneratedMesh.objectReferenceValue = mesh;
                    EditorUtility.ClearProgressBar();
                }
            }
            EditorGUILayout.PropertyField(_twoManifold);
            if (_twoManifold.boolValue)
            {
                GUI.color = Color.yellow;
                GUILayout.Label("WARNING: If a 2 manifold mesh is large, it may\n" +
                                "dramatically increase mesh generation time");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}