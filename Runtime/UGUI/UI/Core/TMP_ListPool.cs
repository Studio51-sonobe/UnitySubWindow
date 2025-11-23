using System.Collections.Generic;

namespace MultiWindow.UI
{
	internal static class TMP_ListPool<T>
	{      
		public static List<T> Get()
		{
			return s_ListPool.Get();
		}
		public static void Release(List<T> toRelease)
		{
			s_ListPool.Release(toRelease);
		}
		static readonly TMP_ObjectPool<List<T>> s_ListPool = new( null, l => l.Clear());
	}
}