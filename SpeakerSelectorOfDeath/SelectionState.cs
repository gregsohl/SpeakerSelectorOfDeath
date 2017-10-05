using System;

namespace SpeakerSelectorOfDeath
{
	[Flags]
	public enum SelectionState
	{
		Default = 0,
		InSearchResult = 1,
		SameTimeConflict = 2,
		BackToBack = 4,
		SpeakerNoSelection = 8,
	}
}