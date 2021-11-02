using System;
using System.Collections.Generic;
using System.IO;

namespace Nager.Date.ICalendar
{
    public static class SerializeICalEvt
    {
        private const string IcalEndline = "\r\n";
        public static void Serialize(TextWriter stream, IEnumerable<ICalEvt> evts)
        {
            WriteShortProperty(stream, "BEGIN", "VCALENDAR");
            WriteShortProperty(stream, "PRODID", "-//holidays.mcshaz.com//en");
            WriteShortProperty(stream, "VERSION", "2.0");
            WriteShortProperty(stream, "METHOD", "PUBLISH");
            foreach (var evt in evts)
            {
                WriteShortProperty(stream, "BEGIN", "VEVENT");
                WriteShortProperty(stream, "CLASS", "PUBLIC");
                WriteDate(stream, "DTSTART", evt.DtStart);
                WriteDateTime(stream, "DTSTAMP", evt.DtStamp);
                WriteShortProperty(stream, "UID", evt.UID);
                WriteDateTime(stream, "CREATED", evt.Created);
                WriteDateTime(stream, "LAST-MODIFIED", evt.LastModified);
                WriteShortProperty(stream, "SEQUENCE", "0");
                WriteShortProperty(stream, "STATUS", "CONFIRMED");
                WriteProperty(stream, "SUMMARY", evt.Summary);
                WriteProperty(stream, "DESCRIPTION", evt.Description);
                WriteProperty(stream, "X-ALT-DESC;FMTTYPE=text/html", evt.HtmlDescription);
                WriteShortProperty(stream, "TRANSP", "OPAQUE");
                WriteShortProperty(stream, "END", "VEVENT");
            }
            WriteShortProperty(stream, "END", "VCALENDAR");
        }
        private static void WriteDateTime(TextWriter stream, string property, DateTime dt)
        {
            stream.Write(property);
            stream.Write(':');
            const string IcalDTFmt = "yyyyMMddTHHmmss";
            stream.Write(dt.ToString(dt.Kind == DateTimeKind.Utc
                ? IcalDTFmt + 'Z'
                : IcalDTFmt));
            stream.Write(IcalEndline);
        }

        private static void WriteDate(TextWriter stream, string property, DateTime dt)
        {
            stream.Write(property);
            stream.Write(";VALUE=DATE:");
            stream.Write(dt.ToString("yyyyMMdd"));
            stream.Write(IcalEndline);
        }
        private static void WriteShortProperty(TextWriter stream, string property, string value)
        {
            stream.Write(property);
            stream.Write(':');
            stream.Write(value);
            stream.Write(IcalEndline);
        }
        private static void WriteProperty(TextWriter stream, string property, string value)
        {
            var lineLength = 75;
            var nameValue = property + ':' + value.Replace("\r", string.Empty).Replace("\n", "\\n");
            for (var i = 0; i < nameValue.Length; i += lineLength)
            {
                if (i > 0) {
                    stream.Write(' ');
                    if (i == lineLength) { --lineLength; }
                }
                var remainder = nameValue.Length - i;
                stream.Write(nameValue.Substring(i, remainder < lineLength ? remainder : lineLength));
                stream.Write(IcalEndline);
            }
        }
    }
}
