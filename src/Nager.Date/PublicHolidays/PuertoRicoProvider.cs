using System;
using System.Collections.Generic;
using System.Linq;
using Nager.Date.Contract;
using Nager.Date.Extensions;
using Nager.Date.Model;

namespace Nager.Date.PublicHolidays
{
    /// <summary>
    /// Puerto Rico
    /// </summary>
    public class PuertoRicoProvider : IPublicHolidayProvider
    {
        private readonly ICatholicProvider _catholicProvider;

        /// <summary>
        /// PuertoRicoProvider
        /// </summary>
        /// <param name="catholicProvider"></param>
        public PuertoRicoProvider(ICatholicProvider catholicProvider)
        {
            this._catholicProvider = catholicProvider;
        }

        ///<inheritdoc/>
        public IEnumerable<PublicHoliday> Get(int year)
        {
            var countryCode = CountryCode.PR;

            var secondMondayInJanuary = DateSystem.FindDay(year, Month.January, DayOfWeek.Monday, Occurrence.Second);
            var thirdMondayInJanuary = DateSystem.FindDay(year, Month.January, DayOfWeek.Monday, Occurrence.Third);
            var thirdMondayInFebruary = DateSystem.FindDay(year, Month.February, DayOfWeek.Monday, Occurrence.Third);
            var thirdMondayInApril = DateSystem.FindDay(year, Month.April, DayOfWeek.Monday, Occurrence.Third);
            var lastMondayInMay = DateSystem.FindLastDay(year, Month.May, DayOfWeek.Monday);
            var thirdMondayInJuly = DateSystem.FindDay(year, Month.July, DayOfWeek.Monday, Occurrence.Third);
            var firstMondayInSeptember = DateSystem.FindDay(year, Month.September, DayOfWeek.Monday, Occurrence.First);
            var secondMondayInOctober = DateSystem.FindDay(year, Month.October, DayOfWeek.Monday, Occurrence.Second);
            var fourthThursdayInNovember = DateSystem.FindDay(year, Month.November, DayOfWeek.Thursday, Occurrence.Fourth);

            var items = new List<PublicHoliday>();

            #region New Years Day with fallback

            var newYearsDay = new DateTime(year, 1, 1).Shift(saturday => saturday.AddDays(-1), sunday => sunday.AddDays(1));
            items.Add(new PublicHoliday(newYearsDay, "D??a de A??o Nuevo", "New Year's Day", countryCode));

            #endregion

            items.Add(new PublicHoliday(year, 1, 6, "D??a de Reyes", "Three Kings Day / Epiphany", countryCode));
            items.Add(new PublicHoliday(secondMondayInJanuary, "Natalicio de Eugenio Mar??a de Hostos", "Birthday of Eugenio Mar??a de Hostos", countryCode));
            items.Add(new PublicHoliday(thirdMondayInJanuary, "Natalicio de Martin Luther King, Jr.", "Martin Luther King, Jr. Day", countryCode));
            items.Add(new PublicHoliday(thirdMondayInFebruary, "D??a de los Presidentes", "Presidents' Day", countryCode));
            items.Add(new PublicHoliday(year, 2, 18, "Natalicio de Luis Mu??oz Mar??n", "Birthday of Luis Mu??oz Mar??n", countryCode));
            items.Add(new PublicHoliday(year, 3, 22, "D??a de la Abolici??n de Esclavitud", "Emancipation Day", countryCode));
            items.Add(this._catholicProvider.GoodFriday("Viernes Santo", year, countryCode));
            items.Add(new PublicHoliday(thirdMondayInApril, "Natalicio de Jos?? de Diego", "Birthday of Jos?? de Diego", countryCode));
            items.Add(new PublicHoliday(lastMondayInMay, "Recordaci??n de los Muertos de la Guerra", "Memorial Day", countryCode));

            #region Independence Day with fallback

            var independenceDay = new DateTime(year, 7, 4).Shift(saturday => saturday.AddDays(-1), sunday => sunday.AddDays(1));
            items.Add(new PublicHoliday(independenceDay, "D??a de la Independencia de los Estados Unidos", "Independence Day", countryCode));

            #endregion
            
            items.Add(new PublicHoliday(thirdMondayInJuly, "Natalicio de Don Luis Mu??oz Rivera", "Birthday of Don Luis Mu??oz Rivera", countryCode));
            items.Add(new PublicHoliday(year, 7, 25, "Constituci??n de Puerto Rico", "Puerto Rico Constitution Day", countryCode));
            items.Add(new PublicHoliday(year, 7, 27, "Natalicio de Dr. Jos?? Celso Barbosa", "Birthday of Dr. Jos?? Celso Barbosa", countryCode));
            items.Add(new PublicHoliday(firstMondayInSeptember, "D??a del Trabajo", "Labour Day", countryCode));
            items.Add(new PublicHoliday(secondMondayInOctober, "D??a de la Raza Descubrimiento de Am??rica", "Columbus Day", countryCode));

            #region Veterans Day with fallback

            var veteransDay = new DateTime(year, 11, 11).Shift(saturday => saturday.AddDays(-1), sunday => sunday.AddDays(1));
            items.Add(new PublicHoliday(veteransDay, "D??a del Veterano D??a del Armisticio", "Veterans Day", countryCode));

            #endregion
            
            items.Add(new PublicHoliday(year, 11, 19, "D??a del Descubrimiento de Puerto Rico", "Discovery of Puerto Rico", countryCode));
            items.Add(new PublicHoliday(fourthThursdayInNovember, "D??a de Acci??n de Gracias", "Thanksgiving Day", countryCode));
            items.Add(new PublicHoliday(year, 12, 24, "Noche Buena", "Christmas Eve", countryCode));
            items.Add(new PublicHoliday(year, 12, 25, "Navidad", "Christmas Day", countryCode));
            
            return items.OrderBy(o => o.Date);
        }

        ///<inheritdoc/>
        public IEnumerable<string> GetSources()
        {
            return new string[]
            {
                "https://en.wikipedia.org/wiki/Public_holidays_in_Puerto_Rico",
                "https://www.timeanddate.com/holidays/puerto-rico/2017#!hol=9",
                "http://www.puertorico.com/official-holidays/",
                "http://www.topuertorico.org/reference/holi.shtml"
            };
        }
    }
}
