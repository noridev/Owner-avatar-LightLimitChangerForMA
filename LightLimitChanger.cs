﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using nadena.dev.modular_avatar.core;

public class LightLimitChanger : EditorWindow
{
    bool DefaultUse = false;
    bool IsValueSave = false;
    float defaultLightValue = 0.5f;
    float MaxLightValue = 1.0f;
    float MinLightValue = 0.0f;
    
    const string SHADER_KEY_LightMinLimit = "material._LightMinLimit";
    const string SHADER_KEY_LightMaxLimit = "material._LightMaxLimit";

    GameObject avater;

    [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
    static void CreateGUI()
    {
        GetWindow<LightLimitChanger>("LightLimitChanger");
    }
    private void OnGUI()
    {
        GUILayout.Label("アバターを選択");
        avater = (GameObject)EditorGUILayout.ObjectField("Avater", avater, typeof(GameObject), true);

        GUILayout.Label("\nパラメータ");
        DefaultUse = EditorGUILayout.Toggle("DefaultUse", DefaultUse);
        IsValueSave = EditorGUILayout.Toggle("SaveValue", IsValueSave);
        MaxLightValue = EditorGUILayout.FloatField("MaxLight", MaxLightValue);
        MinLightValue = EditorGUILayout.FloatField("MinLight", MinLightValue);
        defaultLightValue = EditorGUILayout.FloatField("DefaultLight", defaultLightValue);

        EditorGUI.BeginDisabledGroup(avater == null);
        if(GUILayout.Button("Generate"))
        {
            var path = EditorUtility.SaveFilePanelInProject("保存場所", "New Assets", "asset", "アセットの保存場所");
            if (path == null) return;

            path = new System.Text.RegularExpressions.Regex(@"\.asset").Replace(path, "");
            var saveName = System.IO.Path.GetFileNameWithoutExtension(path);

            GenerateAssets(path, saveName, avater);
        }
        EditorGUI.EndDisabledGroup();
    }
    private void GenerateAssets(string savePath, string saveName, GameObject avater) 
    {
        /////////////////////
        //アニメーションの生成
        var animLightChangeClip = new AnimationClip();
        var animLightDisableClip = new AnimationClip();

        //アニメーションをまとめて生成
        createAnimationClip(avater, animLightChangeClip, animLightDisableClip, avater.name);
        AssetDatabase.CreateAsset(animLightChangeClip, $"{savePath}_active.anim");
        AssetDatabase.CreateAsset(animLightDisableClip, $"{savePath}_deactive.anim");

        //////////////////////////////////
        //アニメーターコントローラーの生成
        var controller = AnimatorController.CreateAnimatorControllerAtPath($"{savePath}.controller");

        controller.parameters = new AnimatorControllerParameter[]
        {
            new AnimatorControllerParameter()
            {
                name = saveName,
                type = AnimatorControllerParameterType.Bool,
                defaultBool = this.DefaultUse
            },
            new AnimatorControllerParameter()
            {
                name = saveName + "_motion",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = this.defaultLightValue
            }
        };
        //レイヤーとステートの設定
        var layer = controller.layers[0];
        layer.name = saveName;
        layer.stateMachine.name = saveName;

        var lightActiveState = layer.stateMachine.AddState($"{saveName}_Active", new Vector3(300, 200));
        lightActiveState.motion = animLightChangeClip;
        lightActiveState.writeDefaultValues = false;
        lightActiveState.timeParameterActive = true;
        lightActiveState.timeParameter = saveName + "_motion";

        var lightDeActiveState = layer.stateMachine.AddState($"{saveName}_DeActive", new Vector3(300, 100));
        lightDeActiveState.motion = animLightDisableClip;
        lightDeActiveState.writeDefaultValues = false;
        layer.stateMachine.defaultState = lightDeActiveState;

        var toDeActiveTransition = lightActiveState.AddTransition(lightDeActiveState);
        toDeActiveTransition.exitTime = 0;
        toDeActiveTransition.duration = 0;
        toDeActiveTransition.hasExitTime = false;
        toDeActiveTransition.conditions = new AnimatorCondition[] {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.IfNot,
                    parameter = saveName,
                    threshold = 1,
                },
            };
        var toActiveTransition = lightDeActiveState.AddTransition(lightActiveState);
        toActiveTransition.exitTime = 0;
        toActiveTransition.duration = 0;
        toActiveTransition.hasExitTime = false;
        toActiveTransition.conditions = new AnimatorCondition[] {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.If,
                    parameter = saveName,
                    threshold = 1,
                },
            };

        AssetDatabase.SaveAssets();

        Debug.Log(savePath);
        Debug.Log(saveName);
    }
    private void createAnimationClip(GameObject parentObj, AnimationClip animActiveClip, AnimationClip animDeActiveClip, string avaterName)
    {
        Transform children = parentObj.GetComponent<Transform>();

        if (children.childCount == 0) return;

        foreach(Transform obj in children)
        {
            SkinnedMeshRenderer SkinnedMeshR = obj.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (SkinnedMeshR != null)
            {
                foreach(var mat in SkinnedMeshR.sharedMaterials)
                {
                    if(mat != null && (mat.shader.name.IndexOf("lilToon") != -1 || mat.shader.name.IndexOf("motchiri") != -1))
                    {
                        string path = getObjectPath(obj).Replace(avaterName + "/", "");
                        animActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMinLimit, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));
                        animActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMaxLimit, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));
                        
                        animDeActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMinLimit, new AnimationCurve(new Keyframe(0 / 60.0f, 0.0f), new Keyframe(1 / 60.0f, 0.0f)));
                        animDeActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMaxLimit, new AnimationCurve(new Keyframe(0 / 60.0f, 1.0f), new Keyframe(1 / 60.0f, 1.0f)));
                    }
                }
            }
            createAnimationClip(obj.gameObject, animActiveClip, animDeActiveClip, avaterName);
        }
    }
    /*
    private void setAnimationKey(GameObject obj, AnimationClip clip, string path, string shaderPropName)
    {
        clip.SetCurve(path, typeof(SkinnedMeshRenderer), shaderPropName, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));
    }
    */
    private string getObjectPath(Transform trans)
    {
        string path = trans.name;
        var parent = trans.parent;
        while (parent)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }
        return path;
    }
}
