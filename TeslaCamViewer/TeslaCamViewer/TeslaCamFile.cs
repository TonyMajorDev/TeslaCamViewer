using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeslaCamViewer
{
    /// <summary>
    /// A single TeslaCam File
    /// </summary>
    public class TeslaCamFile
    {
        public enum CameraType
        {
            UNKNOWN,
            LEFT_REPEATER,
            FRONT,
            RIGHT_REPEATER
        }

        // Note: it appears that newer recordings include the seconds, so that segment is optional
        // filename regex example1: 2019-08-23_20-30-01-front.mp4
        // filename regex example2: 2019-04-12_08-45-front.mp4
        // filename regex example3: 2019-04-12_08-45-left_repeater.mp4
        // filename regex example4: 2019-04-12_08-45-right_repeater.mp4
        // filename regex example4: 2019-06-13_14-01-01-right_repeater.mp4
        private readonly string FileNameRegex = "(?<datetime>[0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{2}-[0-9]{2}(-[0-9]{2}){0,1})-(?<cam>[a-z_]*).mp4";
        public string FilePath { get; private set; }
        public string FileName { get { return System.IO.Path.GetFileName(FilePath); } }
        public TeslaCamDate Date { get; private set; }
        public CameraType CameraLocation { get; private set; }
        public string FileDirectory { get { return System.IO.Path.GetDirectoryName(FilePath); } }
        public Uri FileURI { get { return new Uri(this.FilePath); } }

        public TeslaCamFile(string FilePath)
        {
            this.FilePath = FilePath;
            var m = new System.Text.RegularExpressions.Regex(FileNameRegex).Matches(FileName);
            if (m.Count != 1) throw new Exception("Unexpected TeslaCam filename format '" + FileName + "'");
            this.Date = new TeslaCamDate(m[0].Groups["datetime"].Value);
            string cameraType = m[0].Groups["cam"].Value;
            if (cameraType == "front")
                CameraLocation = CameraType.FRONT;
            else if (cameraType == "left_repeater")
                CameraLocation = CameraType.LEFT_REPEATER;
            else if (cameraType == "right_repeater")
                CameraLocation = CameraType.RIGHT_REPEATER;
            else
                throw new Exception("Unexpected Camera Type: '" + cameraType + "'");
        }

    }
}
