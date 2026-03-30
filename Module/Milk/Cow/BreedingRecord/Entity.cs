// File: Module/Cow/BreedingRecord.cs
using System;
using FarmingApi.Core;

namespace FarmingApi.Modules.Cow
{
    public class BreedingRecord : AuditableEntity
    {
        public int CowId { get; set; }

        public DateTime ServiceDate { get; set; }         // date of insemination/service
        public int? ServiceByStaffId { get; set; }        // optional staff reference
        public string? ServiceType { get; set; }          // AI or natural mating
        public string? SireTag { get; set; }              // bull tag or semen batch
        public bool? PregnancyConfirmed { get; set; }
        public DateTime? PregnancyCheckDate { get; set; }
        public string? Notes { get; set; }
    }
}
