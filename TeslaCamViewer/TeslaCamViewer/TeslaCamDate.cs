using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeslaCamViewer
{
    public class TeslaCamDate
    {
        private const string DateFormatFromFilename1 = "yyyy-MM-dd_HH-mm";
        private const string DateFormatFromFilename2 = "yyyy-MM-dd_HH-mm-ss";

        private const string DisplayFormat = "M/d/yyyy h:mm tt";

        public string UTCDateString { get; private set; }
        public string DisplayValue
        {
            get
            {

                return LocalTimeStamp.ToString(DisplayFormat);
            }
        }
        public DateTime UTCTimeStamp
        {
            get
            {

                DateTime dt;
                if (DateTime.TryParseExact(UTCDateString, DateFormatFromFilename1, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    return dt;
                }
                else if (DateTime.TryParseExact(UTCDateString, DateFormatFromFilename2, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    return dt;
                }
                else
                { 
                    throw new Exception("Invalid date format: " + UTCDateString);
                }
            }
        }
        public DateTime LocalTimeStamp
        {
            get
            {

                return UTCTimeStamp;
            }
        }

        public TeslaCamDate(string DateString)
        {
            UTCDateString = DateString;
        }

    }
}
