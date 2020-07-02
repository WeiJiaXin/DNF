using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
using System;

[CustomPropertyDrawer(typeof(SortingLayerAttribute))]
public class SortingLayerDrawer : PropertyDrawer {
	const string NONE = "<None>";
 
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.propertyType != SerializedPropertyType.String) {
			EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
			return;
		}
		position = EditorGUI.PrefixLabel(position, label);
		string value = property.stringValue;
		if (string.IsNullOrEmpty (value))
			value = NONE;
		if (GUI.Button(position, value, EditorStyles.popup)) {
			Selector(property);
		}
	}
 
	void Selector(SerializedProperty property) {
		string[] layers = GetSortingLayerNames ();
 
		GenericMenu menu = new GenericMenu();
 
		bool isNone = string.IsNullOrEmpty (property.stringValue);
		menu.AddItem(new GUIContent(NONE), isNone, HandleSelect, new SpineDrawerValuePair(NONE, property));
 
		for (int i = 0; i < layers.Length; i++) {
			string name = layers[i];
			menu.AddItem(new GUIContent(name), name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
		}
		menu.ShowAsContext();
	}
 
	void HandleSelect(object val) {
		var pair = (SpineDrawerValuePair)val;
		if (pair.stringValue.Equals (NONE)) {
			pair.property.stringValue = "";
		} else {
			pair.property.stringValue = pair.stringValue;
		}
		pair.property.serializedObject.ApplyModifiedProperties();
	}
 
	// Get the sorting layer names
	public string[] GetSortingLayerNames() {
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
	}
}
public struct SpineDrawerValuePair {
	public string stringValue;
	public SerializedProperty property;

	public SpineDrawerValuePair (string val, SerializedProperty property) {
		this.stringValue = val;
		this.property = property;
	}
}