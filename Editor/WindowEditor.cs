
// using UnityEngine;
// using UnityEditor;
// using UnityEngine.EventSystems;
// using System.Linq;

// namespace MultiWindow.Editor
// {
// 	[CustomEditor( typeof( Window), true)]
// 	sealed class WindowEditor : UnityEditor.Editor
// 	{
// 		public override void OnInspectorGUI()
// 		{
// 			base.OnInspectorGUI();
			
// 			EditorGUILayout.Space();
			
// 			using( new GUILayout.HorizontalScope())
// 			{
// 				GUILayout.FlexibleSpace();
				
// 				using( new EditorGUI.DisabledGroupScope( Application.isPlaying != false))
// 				{
// 					if( GUILayout.Button( "Create Input Module") != false)
// 					{
// 						foreach( var target in targets)
// 						{
// 							if( target is Window window && window.m_InputModule == null)
// 							{
// 								EventSystem eventSystem = window.GetComponentInParent<EventSystem>();
// 								if( eventSystem != null)
// 								{
// 									window.m_InputModule = eventSystem.gameObject.AddComponent<WindowInputModule>();
// 									EditorGUIUtility.PingObject( eventSystem);
// 								}
// 								else
// 								{
// 									eventSystem = FindObjectsByType<EventSystem>( FindObjectsSortMode.None).FirstOrDefault();
// 									if( eventSystem != null)
// 									{
// 										window.m_InputModule = eventSystem.gameObject.AddComponent<WindowInputModule>();
// 										EditorGUIUtility.PingObject( eventSystem);
// 									}
// 								}
// 							}
// 						}
// 					}
// 				}
// 			}
// 		}
// 	}
// }
