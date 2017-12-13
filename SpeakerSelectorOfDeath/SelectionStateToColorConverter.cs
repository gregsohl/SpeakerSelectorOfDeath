using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SpeakerSelectorOfDeath
{
	public class SelectionStateToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var selectionState = (SelectionState) value;

			if (selectionState.Has(SelectionState.InSearchResult))
				return new SolidColorBrush(Colors.Pink);

			if (selectionState.Has(SelectionState.SameTimeConflict))
				return new SolidColorBrush(Colors.Coral);

			if (selectionState.Has(SelectionState.BackToBack))
				return new SolidColorBrush(Colors.BlueViolet);

			if (selectionState.Has(SelectionState.SpeakerNoSelection))
				return new SolidColorBrush(Colors.Yellow);

			if (selectionState.Has(SelectionState.SpeakerWithCountSelections))
				return new SolidColorBrush(Colors.DeepPink);

			return new SolidColorBrush(Colors.LightGreen);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}