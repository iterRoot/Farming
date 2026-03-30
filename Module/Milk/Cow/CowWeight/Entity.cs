// File: Module/Cow/CowWeight.cs
using System;
using FarmingApi.Core;

namespace FarmingApi.Modules.Cow
{
    public class CowWeight : AuditableEntity
    {
        public int CowId { get; set; }

        public DateTime MeasuredAt { get; set; }
        public decimal WeightKg { get; set; }

        public string? MeasuredBy { get; set; }
        public string? Notes { get; set; }
    }
}
