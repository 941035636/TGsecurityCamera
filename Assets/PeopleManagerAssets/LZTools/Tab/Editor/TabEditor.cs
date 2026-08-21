using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ZTools
{
    [CustomEditor(typeof(Tab))]
    public class TabEditor : Editor
    {
        private Tab tab;
        private SerializedObject tabClass;//序列化
        private SerializedProperty isHoverImgActive, isHoverTxtColor, isHoverImgColor, hoverImage, hoverImageColor, hoverTxtColor, hoverImg, isOnTxtColor, isOnImgActive, isOnImgColor, tabChangeColorImg, changeTxtColor, imgColor, tabTxt, tabImg, isOn, showPanel, tabName, tabcontroller, multipleChoice;//定义类型，变量a，变量b
        private void OnEnable()
        {
            tabClass = new SerializedObject(target);
            isOnTxtColor = tabClass.FindProperty("isOnTxtColor");
            isOnImgActive = tabClass.FindProperty("isOnImgActive");
            isOnImgColor = tabClass.FindProperty("isOnImgColor");
            tabChangeColorImg = tabClass.FindProperty("tabChangeColorImg");
            changeTxtColor = tabClass.FindProperty("changeTxtColor");
            imgColor = tabClass.FindProperty("imgColor");
            isHoverImgActive = tabClass.FindProperty("isHoverImgActive");
            isHoverImgColor = tabClass.FindProperty("isHoverImgColor");
            hoverImage = tabClass.FindProperty("hoverImage");
            hoverImageColor = tabClass.FindProperty("hoverImageColor");
            tabTxt = tabClass.FindProperty("tabTxt");
            tabImg = tabClass.FindProperty("tabImg");
            isHoverTxtColor = tabClass.FindProperty("isHoverTxtColor");
            hoverTxtColor = tabClass.FindProperty("hoverTxtColor");
            isOn = tabClass.FindProperty("isOn");
            showPanel = tabClass.FindProperty("showPanel");
            tabName = tabClass.FindProperty("tabName");
            hoverImg = tabClass.FindProperty("hoverImg");
            tabcontroller = tabClass.FindProperty("tabcontroller");
            multipleChoice = tabClass.FindProperty("multipleChoice");
        }
        public override void OnInspectorGUI()
        {
            tabClass.Update();
            // 开始监测修改
            EditorGUI.BeginChangeCheck();
            tab = (Tab)target;
            EditorGUILayout.PropertyField(tabTxt);
            EditorGUILayout.PropertyField(multipleChoice);
            EditorGUILayout.PropertyField(isOn);
            // 鼠标移入状态设置
            EditorGUILayout.PropertyField(isHoverImgActive);
            if (isHoverImgActive.boolValue)
            {
                EditorGUILayout.PropertyField(hoverImg);
            }
            EditorGUILayout.PropertyField(isHoverTxtColor);
            if (isHoverTxtColor.boolValue)
            {
                EditorGUILayout.PropertyField(hoverTxtColor);
            }
            EditorGUILayout.PropertyField(isHoverImgColor);
            if (isHoverImgColor.boolValue)
            {
                EditorGUILayout.PropertyField(hoverImage);
                EditorGUILayout.PropertyField(hoverImageColor);
            }
            EditorGUILayout.PropertyField(isOnImgActive);
            if (isOnImgActive.boolValue)
            {
                EditorGUILayout.PropertyField(tabImg);
            }
            EditorGUILayout.PropertyField(isOnTxtColor);
            if (isOnTxtColor.boolValue)
            {
                EditorGUILayout.PropertyField(changeTxtColor);
            }
            EditorGUILayout.PropertyField(isOnImgColor);
            if (isOnImgColor.boolValue)
            {
                EditorGUILayout.PropertyField(tabChangeColorImg);
                EditorGUILayout.PropertyField(imgColor);
            }
            if (isOnTxtColor.boolValue || isOnImgActive.boolValue || isOnImgColor.boolValue)
            {
                EditorGUILayout.PropertyField(showPanel);
            }
            EditorGUILayout.PropertyField(tabcontroller);
            EditorGUILayout.PropertyField(tabName);
            //结束监测，判断是否有修改
            if (EditorGUI.EndChangeCheck())
            {
                tabClass.ApplyModifiedProperties();
            }
        }
    }
}
