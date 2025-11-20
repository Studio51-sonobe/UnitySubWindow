
using UnityEngine;
using SubWindows;

public class Example : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSplashScreen)]
	static void Initialize()
	{
		SubWindow.Initialize( 2);
	}
	void OnDestroy()
	{
		SubWindow.Terminate();
	}
	void Update()
	{
		if( Input.GetMouseButtonUp( 0) != false)
		{
			if( m_Window1.IsCreated == false)
			{
				m_Window1.Create();
			}
			else
			{
				m_Window1.Dispose();
			}
		}
		else if( Input.GetMouseButtonUp( 1) != false)
		{
			if( m_Window2.IsCreated == false)
			{
				m_Window2.Create();
			}
			else
			{
				m_Window2.Dispose();
			}
		}
	}
	[SerializeField]
	SubWindow m_Window1;
	[SerializeField]
	SubWindow m_Window2;
}

