using System;
using System.Text.RegularExpressions;

namespace Nager.Date.ICalendar
{
    public static class NagerHolToIcalEvt
    {
        public static ICalEvt Map(Nager.Date.Model.PublicHoliday ph)
        {
            // maybe a hack - doesn't account for fixed property,
            // the holiday would have been planned (created) before the inaugural date
            // and year 0 has nothing to do with early holidays
            var origin = ph.LaunchYear.HasValue
                ? new DateTime(ph.LaunchYear.Value, ph.Date.Month, ph.Date.Day)
                : DateTime.MinValue;
            // eventually should end up with language based resx files
            var htmlDescription = ph.Global
                    ? $"<h3>{ph.LocalName}</h3><p>{ph.CountryCode} National {ph.Type} Holiday</p>"
                    : $"<h3>{ph.LocalName}</h3><p>{ph.Type} Holiday <small style=\"color:#555\">(in {string.Join(", ", ph.Counties)})</small></p>";
            return new ICalEvt
            {
                DtStart = ph.Date,
                DtStamp = DateTime.UtcNow,
                Created = origin,
                LastModified = origin,
                UID = $"{(ph.Global ? ph.CountryCode : string.Join("&", ph.Counties))}.{ph.Date:yyyyMMdd}@mcshaz.com",
                Description = UltraBasicTagRemover(htmlDescription),
                HtmlDescription = htmlDescription,
                Summary = ph.LocalName
            };
        }
        // should use a parser but for such an ultra basic application where the html can be seen above seems overkill
        private static string UltraBasicTagRemover(string html)
        {
            html = Regex.Replace(html, @"<\/[^>]+>", "\n");
            return Regex.Replace(html, "<[^>]+>", string.Empty).TrimEnd('\n');
        }
    }
}
