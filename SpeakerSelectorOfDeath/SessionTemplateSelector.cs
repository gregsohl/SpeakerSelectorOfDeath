using System.Windows;
using System.Windows.Controls;

namespace SpeakerSelectorOfDeath
{
	public class SessionTemplateSelector : DataTemplateSelector
	{

		/// <summary>
		/// initializes SessionTemplateSelector
		/// </summary>
		public SessionTemplateSelector()
		{
		}
		
		public DataTemplate NonNullTemplate { get; set; }
		

		

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
				return null;

			return NonNullTemplate;

			//return base.SelectTemplate(item, container);
		}
		
			
		
	}
}