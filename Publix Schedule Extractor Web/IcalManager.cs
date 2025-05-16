using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Publix_Schedule_Extractor
{
	internal class IcalManager
	{
		public static Calendar AddEvent(CalDateTime startTime, CalDateTime endTime, string name, ref Calendar calendar)
		{
			var calendarEvent = new CalendarEvent
			{
				Summary = name,
				Start = new CalDateTime(startTime),
				End = new CalDateTime(endTime),
			};
			calendar.Events.Add(calendarEvent);
			return calendar;
		}
	}
}
