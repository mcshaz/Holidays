using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Nager.Date.ICalendar;
using Nager.Date.Website.Middleware;
using Nager.Date.Website.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nager.Date.Website.Controllers
{
    /// <summary>
    /// Public Holiday
    /// </summary>
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [TimestampSession]
    public class PublicHolidayController : Controller
    {
        [Route("Country/{countrycode}/{year}")]
        [Route("Country/{countrycode}")]
        public IActionResult Country(string countrycode, int year = 0)
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            if (!Enum.TryParse(countrycode, true, out CountryCode countryCode))
            {
                return NotFound();
            }

            var country = new Country.CountryProvider().GetCountry(countryCode.ToString());

            var item = new PublicHolidayInfo
            {
                Country = country.CommonName,
                CountryCode = countrycode,
                Year = year
            };

            return View(item);
        }

        [Route("Country/{countrycode}/{year}/csv")]
        public ActionResult DownloadCsv(string countrycode, int year = 0)
        {
            return DownloadFile(countrycode, year, FileTypes.csv);
        }

        [Route("Country/{countrycode}/{year}/ics")]
        public ActionResult DownloadIcs(string countrycode, int year = 0)
        {
            return DownloadFile(countrycode, year, FileTypes.ics);
        }
        private enum FileTypes { csv, ics }
        private ActionResult DownloadFile(string countrycode, int year, FileTypes fileType)
        {
            /* could do this directly to the stream like:
            this.Response.Headers.Add(HeaderNames.ContentDisposition, $"attachment; filename=\"{Path.GetFileName(filePath)}\"");
            this.Response.Headers.Add(HeaderNames.ContentType, "application/octet-stream");

            TextWriter tw = new StreamWriter(this.Response.Body);
            */
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            if (!Enum.TryParse(countrycode, true, out CountryCode countryCode))
            {
                return NotFound();
            }

            var items = DateSystem.GetPublicHolidays(year, countryCode).ToList();

            if (items.Count > 0)
            {
                var fileDownloadName = $"publicholiday.{countrycode}.{year}.{fileType}";
                using var memoryStream = new MemoryStream();
                using var streamWriter = new StreamWriter(memoryStream);
                if (fileType == FileTypes.csv)
                {
                    using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
                    var csvItems = items.Select(o => new PublicHolidayCsv(o)).ToArray();

                    csv.WriteRecords(csvItems);
                    streamWriter.Flush();

                    var csvData = memoryStream.ToArray();

                    var result = new FileContentResult(csvData, "application/octet-stream")
                    {
                        FileDownloadName = fileDownloadName
                    };

                    return result;
                }
                else if (fileType == FileTypes.ics)
                {
                    SerializeICalEvt.Serialize(streamWriter, items.Select(NagerHolToIcalEvt.Map));

                    streamWriter.Flush();
                    var icsData = memoryStream.ToArray();

                    var result = new FileContentResult(icsData, "text/calendar")
                    {
                        FileDownloadName = fileDownloadName
                    };

                    return result;
                } 

            }

            return NotFound();
        }
    }
}
