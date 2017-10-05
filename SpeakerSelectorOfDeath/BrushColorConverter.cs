using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SpeakerSelectorOfDeath
{
	public class BrushColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				{
					return new SolidColorBrush(Colors.Pink);
				}
			}
			return new SolidColorBrush(Colors.LightGreen);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}
}