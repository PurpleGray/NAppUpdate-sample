using System;

namespace NAppUpdate_sample.Utils
{
    public class UpdateTaskInfo
    {
        public string FileDescription { get; set; }
        public string FileName { get; set; }
        public string FileVersion { get; set; }
        public long? FileSize { get; set; }
        public DateTime? FileDate { get; set; }
    }
}