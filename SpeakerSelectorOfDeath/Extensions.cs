using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpeakerSelectorOfDeath
{
	public static class Extensions
	{
		public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
		{
			foreach (T t in items)
			{
				collection.Add(t);
			}
		}
		
	}
}