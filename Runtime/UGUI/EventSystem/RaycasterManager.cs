#if false
using System.Collections.Generic;

namespace MultiWindow.EventSystems
{
	public static class RaycasterManager
	{
		public static List<BaseRaycaster> GetRaycasters()
		{
			return s_Raycasters;
		}
		internal static void AddRaycaster( BaseRaycaster baseRaycaster)
		{
			if (s_Raycasters.Contains( baseRaycaster))
			{
				return;
			}
			s_Raycasters.Add( baseRaycaster);
		}
		internal static void RemoveRaycasters( BaseRaycaster baseRaycaster)
		{
			if (!s_Raycasters.Contains( baseRaycaster))
			{
				return;
			}
			s_Raycasters.Remove( baseRaycaster);
		}
		static readonly List<BaseRaycaster> s_Raycasters = new();
	}
}
#endif
