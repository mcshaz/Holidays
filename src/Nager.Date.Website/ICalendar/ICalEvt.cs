using System;

namespace Nager.Date.ICalendar
{
    public class ICalEvt
    {
        public DateTime DtStart { get; set; }
        public DateTime DtStamp { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string UID { get; set; }
        public string Description { get; set; }
        public string HtmlDescription { get; set; }
        public string Summary { get; set; }
    }
}
