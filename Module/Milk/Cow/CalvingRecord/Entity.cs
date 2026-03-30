// File: Module/Cow/CalvingRecord.cs
using System;
using FarmingApi.Core;

namespace FarmingApi.Modules.Cow
{
    public class CalvingRecord : AuditableEntity
    {
        public int CowId { get; set; }

        public DateTime CalvedAt { get; set; }
        public int NumberOfCalves { get; set; } = 1;
        public string? CalfTagNumbers { get; set; } // comma-separated or JSON if multiple
        public string? DeliveryType { get; set; }   // normal, assisted, caesarean
        public string? Outcome { get; set; }        // alive, stillborn, weak
        public decimal? BirthWeightKg { get; set; } // optional average or first calf
        public string? Notes { get; set; }
    }
}
