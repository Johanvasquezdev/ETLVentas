using System;
using System.Collections.Generic;

namespace ETLVentas.Data.Result
{
    public class EntityProcessResult
    {
        public string EntityName { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int InsertedRecords { get; set; }
        public int DuplicatedRecords { get; set; }
        public int RejectedRecords { get; set; }
        public int Errors { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}
