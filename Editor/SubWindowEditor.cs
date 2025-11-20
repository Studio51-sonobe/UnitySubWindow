
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Linq;

namespace SubWindows.Editor
{
	[CustomEditor( typeof( SubWindow), true)]
	sealed class SubWindowEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			EditorGUILayout.Space();
			
			using( new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				
				using( new EditorGUI.DisabledGroupScope( Application.isPlaying != false))
				{
					if( GUILayout.Button( "Create Input Module") != false)
					{
						foreach( var target in targets)
						{
							if( target is SubWindow subWindow && subWindow.m_InputModule == null)
							{
								EventSystem eventSystem = subWindow.GetComponentInParent<EventSystem>();
								if( eventSystem != null)
								{
									subWindow.m_InputModule = eventSystem.gameObject.AddComponent<WindowsInputModule>();
									EditorGUIUtility.PingObject( eventSystem);
								}
								else
								{
									eventSystem = FindObjectsByType<EventSystem>( FindObjectsSortMode.None).FirstOrDefault();
									if( eventSystem != null)
									{
										subWindow.m_InputModule = eventSystem.gameObject.AddComponent<WindowsInputModule>();
										EditorGUIUtility.PingObject( eventSystem);
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
