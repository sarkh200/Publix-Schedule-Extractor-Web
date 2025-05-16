using HtmlAgilityPack;
using System.Globalization;

namespace Publix_Schedule_Extractor
{
	internal class HTMLParser
	{
		public static void GetEventsFromFile(string htmlString, out List<(DateTime start, DateTime end)> shifts, out List<(DateTime start, DateTime end)> meals)
		{
			HtmlDocument html = new();
			html.LoadHtml(htmlString);

			HtmlNodeCollection shiftDetails = html.DocumentNode.SelectSingleNode("//div[@id=\"scheduleTab\"]").SelectNodes(".//div/div/div[@class=\"pb-3\"]/div[contains(@id,'shift-details')]");

			List<DateOnly> shiftDays = GetShiftDays(html, shiftDetails);

			Console.WriteLine("Shifts");
			shifts = ParseShiftTimes(GetShiftTimes(shiftDetails, ExtractorType.Shift), shiftDays);

			Console.WriteLine("Meals");
			meals = ParseShiftTimes(GetShiftTimes(shiftDetails, ExtractorType.Meal), shiftDays);
		}

		static DateOnly GetStartDate(HtmlDocument html)
		{
			string[] startDateString = html.DocumentNode.SelectSingleNode("//div[@id=\"redesignedSchedule\"]//span").InnerText.Trim().Split('/');

			return new(month: Int32.Parse(startDateString[0]), day: Int32.Parse(startDateString[1]), year: Int32.Parse(startDateString[2]));
		}

		static List<DateOnly> GetShiftDays(HtmlDocument html, HtmlNodeCollection shiftDetails)
		{
			List<DateOnly> shiftDays = [];

			DateOnly startDate = GetStartDate(html);

			foreach (var node in shiftDetails)
			{
				int dateNumber = Int32.Parse(node.ParentNode.SelectSingleNode(".//div[@class=\"calendar-day-of-month-number\"]").InnerText);

				DateOnly day = new(month: startDate.Month, day: dateNumber, year: startDate.Year);

				if (dateNumber < startDate.Day)
					day = day.AddMonths(1);

				shiftDays.Add(day);
			}

			return shiftDays;
		}

		static List<string> GetShiftTimes(HtmlNodeCollection shiftDetails, ExtractorType type)
		{
			List<string> shiftTimeStrings = [];

			string nodePath = "";
			if (type == ExtractorType.Shift)
				nodePath = ".//div[@class=\"col-xs-10\"]/div[2]/div[1]";
			else if (type == ExtractorType.Meal)
				nodePath = ".//div[@class=\"col-xs-10\"]/div[3]/div[1]/div[1]";

			foreach (var shift in shiftDetails)
			{
				HtmlNode? shiftTime = shift.SelectSingleNode(nodePath);
				if (shiftTime != null)
					shiftTimeStrings.Add(shiftTime.InnerText.Trim());
				else
					shiftTimeStrings.Add("null");
			}

			return shiftTimeStrings;
		}

		static void GetStartAndEndTimesOfShift(string shift, out TimeOnly startTime, out TimeOnly endTime)
		{
			string[] shiftStrings = shift.Split("-");
			shiftStrings[0] = shiftStrings[0].Trim();
			shiftStrings[1] = shiftStrings[1].Trim();

			string[] formats = [
				"h:mm tt",
						"hh:mm tt",
						"h tt",
						"hh tt"
			];

			var formatInfo = (DateTimeFormatInfo) CultureInfo.InvariantCulture.DateTimeFormat.Clone();
			formatInfo.AMDesignator = "a.m.";
			formatInfo.PMDesignator = "p.m.";

			startTime = TimeOnly.ParseExact(shiftStrings[0], formats, formatInfo);
			endTime = TimeOnly.ParseExact(shiftStrings[1], formats, formatInfo);
		}

		static List<(DateTime start, DateTime end)> ParseShiftTimes(List<string> shiftTimeStrings, List<DateOnly> shiftDays)
		{
			List<(DateTime start, DateTime end)> shiftTimes = [];

			for (int i = 0; i < shiftTimeStrings.Count; i++)
			{
				if (shiftTimeStrings[i] == "null")
					continue;
				string shift = shiftTimeStrings[i];
				GetStartAndEndTimesOfShift(shift, out TimeOnly startTime, out TimeOnly endTime);

				DateTime startDateTime = new(shiftDays[i], startTime);
				DateOnly endDate = shiftDays[i];

				if (endTime.CompareTo(startTime) < 0)
					endDate = endDate.AddDays(1);

				DateTime endDateTime = new(endDate, endTime);

				shiftTimes.Add((startDateTime, endDateTime));

				Console.WriteLine(shiftTimes.Last());
			}

			return shiftTimes;
		}

		enum ExtractorType
		{
			Meal,
			Shift
		}
	}
}
