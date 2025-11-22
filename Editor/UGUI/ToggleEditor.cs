
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MultiWindow.UI.Editor
{
	[CustomEditor(typeof(Toggle), true)]
	[CanEditMultipleObjects]
	public class ToggleEditor : SelectableEditor
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
			m_GraphicProperty = serializedObject.FindProperty("graphic");
			m_GroupProperty = serializedObject.FindProperty("m_Group");
			m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
			m_OnValueChangedProperty = serializedObject.FindProperty("onValueChanged");
		}
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			serializedObject.Update();
			Toggle toggle = serializedObject.targetObject as Toggle;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( m_IsOnProperty);
			
			if( EditorGUI.EndChangeCheck())
			{
				if (!Application.isPlaying)
				{
					EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);
				}
				ToggleGroup group = m_GroupProperty.objectReferenceValue as ToggleGroup;
				toggle.isOn = m_IsOnProperty.boolValue;
				
				if( group != null && group.isActiveAndEnabled && toggle.IsActive())
				{
					if (toggle.isOn || (!group.AnyTogglesOn() && !group.allowSwitchOff))
					{
						toggle.isOn = true;
						group.NotifyToggleOn(toggle);
					}
				}
			}
			EditorGUILayout.PropertyField(m_TransitionProperty);
			EditorGUILayout.PropertyField(m_GraphicProperty);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(m_GroupProperty);
			
			if( EditorGUI.EndChangeCheck())
			{
				if (!Application.isPlaying)
				{
					EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);
				}
				ToggleGroup group = m_GroupProperty.objectReferenceValue as ToggleGroup;
				toggle.group = group;
			}
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_OnValueChangedProperty);
			serializedObject.ApplyModifiedProperties();
		}
		SerializedProperty m_OnValueChangedProperty;
		SerializedProperty m_TransitionProperty;
		SerializedProperty m_GraphicProperty;
		SerializedProperty m_GroupProperty;
		SerializedProperty m_IsOnProperty;
	}
}
