// File: Module/Cow/PregnancyCheck.cs
using System;
using FarmingApi.Core;

namespace FarmingApi.Modules.Cow
{
    public class PregnancyCheck : AuditableEntity
    {
        public int CowId { get; set; }

        public DateTime CheckedAt { get; set; }
        public bool IsPregnant { get; set; }
        public DateTime? ExpectedCalvingDate { get; set; }
        public string? Method { get; set; } // e.g., ultrasound, rectal palpation
        public string? PerformedBy { get; set; }
        public string? Notes { get; set; }
    }
}
