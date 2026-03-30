// File: Module/Cow/CowStatusHistory.cs
using System;
using FarmingApi.Core;

namespace FarmingApi.Modules.Cow
{
    public class CowStatusHistory : AuditableEntity
    {
        public int CowId { get; set; }

        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? Reason { get; set; }
    }
}
