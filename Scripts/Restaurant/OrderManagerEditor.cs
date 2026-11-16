#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(OrderManager))]
public class OrderManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        OrderManager manager = (OrderManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("레시피 관리", EditorStyles.boldLabel);

        if (GUILayout.Button("프로젝트의 모든 Recipe 로드"))
        {
            // 프로젝트 내 모든 Recipe 에셋 검색
            string[] guids = AssetDatabase.FindAssets("t:Recipe");
            List<Recipe> recipes = new List<Recipe>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Recipe recipe = AssetDatabase.LoadAssetAtPath<Recipe>(path);
                if (recipe != null)
                    recipes.Add(recipe);
            }

            // possibleRecipes 프로퍼티에 할당
            SerializedProperty prop = serializedObject.FindProperty("possibleRecipes");
            prop.ClearArray();
            for (int i = 0; i < recipes.Count; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = recipes[i];
            }

            serializedObject.ApplyModifiedProperties();

            Debug.Log($"OrderManager에 {recipes.Count}개의 레시피를 로드했습니다.");
        }
    }
}
#endif
