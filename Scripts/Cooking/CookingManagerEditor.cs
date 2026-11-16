#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(CookingManager))]
public class CookingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CookingManager manager = (CookingManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("레시피 및 재료 관리", EditorStyles.boldLabel);

        // 프로젝트에서 모든 레시피 로드 버튼
        if (GUILayout.Button("프로젝트의 모든 레시피 로드"))
        {
            string[] guids = AssetDatabase.FindAssets("t:Recipe");
            List<Recipe> recipes = new List<Recipe>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Recipe recipe = AssetDatabase.LoadAssetAtPath<Recipe>(path);
                if (recipe != null)
                    recipes.Add(recipe);
            }

            SerializedProperty allRecipesProperty = serializedObject.FindProperty("allRecipes");
            allRecipesProperty.ClearArray();

            foreach (Recipe recipe in recipes)
            {
                allRecipesProperty.arraySize++;
                allRecipesProperty.GetArrayElementAtIndex(allRecipesProperty.arraySize - 1).objectReferenceValue = recipe;
            }

            serializedObject.ApplyModifiedProperties();

            Debug.Log($"{recipes.Count}개의 레시피를 로드했습니다.");
        }

        // 프로젝트에서 모든 재료 로드 버튼
        if (GUILayout.Button("프로젝트의 모든 재료 로드"))
        {
            string[] guids = AssetDatabase.FindAssets("t:Ingredient");
            List<Ingredient> ingredients = new List<Ingredient>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Ingredient ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(path);
                if (ingredient != null)
                    ingredients.Add(ingredient);
            }

            SerializedProperty ingredientAssetsProperty = serializedObject.FindProperty("ingredientAssets");
            ingredientAssetsProperty.ClearArray();

            foreach (Ingredient ingredient in ingredients)
            {
                ingredientAssetsProperty.arraySize++;
                ingredientAssetsProperty.GetArrayElementAtIndex(ingredientAssetsProperty.arraySize - 1).objectReferenceValue = ingredient;
            }

            serializedObject.ApplyModifiedProperties();

            Debug.Log($"{ingredients.Count}개의 재료를 로드했습니다.");
        }

        // 모든 레시피 해금 버튼
        if (GUILayout.Button("모든 레시피 해금"))
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("이 기능은 게임 실행 중에만 작동합니다.");
                return;
            }

            if (manager != null)
            {
                manager.UnlockAllRecipesForEditor();
            }
        }
    }
}
#endif