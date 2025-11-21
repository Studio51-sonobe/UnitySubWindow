using UnityEngine;

public class GameObjectRotate : MonoBehaviour
{
	void Update()
	{
		m_Cube.localEulerAngles = new Vector3( 
			m_Cube.localEulerAngles.x, 
			(m_Cube.localEulerAngles.y + 1) % 360, 
			m_Cube.localEulerAngles.z);
	}
	[SerializeField]
	Transform m_Cube;
}
