using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Production;

// RawProduction.cs
public class Production : AuditableEntity
{
     public string CowId { get; set; } = null!;        // Reference to Cow table
    public DateTime ProductionDate { get; set; }      // Date of milk collection
    public string Shift { get; set; } = "Morning";    // Morning / Evening / Night

    public decimal MilkQuantity { get; set; }         // Liters collected
    public decimal MilkFat { get; set; }              // % Fat
    public decimal MilkSNF { get; set; }              // Solids-Not-Fat %
    public string? MilkQualityGrade { get; set; }     // Grade A/B/etc.

    public decimal Temperature { get; set; }          // Milk temperature
    public string? CollectedBy { get; set; }          // Employee name

    // Processing / Inventory
    public string? BatchNumber { get; set; }
    public bool TransferredToTank { get; set; }
    public string? TankNumber { get; set; }

    // Health & Compliance
    public bool IsAntibioticRestrictedMilk { get; set; }
    public string? CowHealthStatus { get; set; }
    public string? Notes { get; set; }
}
