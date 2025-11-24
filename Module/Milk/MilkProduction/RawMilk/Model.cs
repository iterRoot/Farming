using System.ComponentModel.DataAnnotations;
namespace FarmingApi.Modules.RawMilk;


public class RawMilkResponse
    {
        // Collection
        public DateTime CollectionDate { get; set; }
        public string Shift { get; set; }
        public decimal QtyLiters { get; set; }
        public string CollectorName { get; set; }
        public string MilkingMethod { get; set; }
        public string? MilkingStation { get; set; }

        // Quality Parameters
        public decimal Temperature { get; set; }
        public decimal FatPercent { get; set; }
        public decimal SNFPercent { get; set; }
        public decimal Density { get; set; }
        public decimal? ProteinPercent { get; set; }
        public decimal? LactosePercent { get; set; }
        public decimal Acidity { get; set; }
        public decimal FreezingPoint { get; set; }
        public decimal AddedWaterPercent { get; set; }
        public int BacteriaCount { get; set; }
        public int SomaticCellCount { get; set; }

        // Safety
        public bool IsAntibioticPositive { get; set; }
        public bool IsSpoiled { get; set; }
        public string? OdorCheck { get; set; }
        public string? ColorCheck { get; set; }
        public string? ContaminationNotes { get; set; }

        // Pricing
        public decimal RatePerLiter { get; set; }
        public decimal AmountPaid { get; set; }
        public string? SupplierId { get; set; }
        public string PaymentStatus { get; set; }

        // Storage
        public string? BatchNumber { get; set; }
        public bool TransferredToBulkTank { get; set; }
        public string? BulkTankNumber { get; set; }
        public DateTime? TransferTime { get; set; }
        public DateTime? CoolingTime { get; set; }
        public string ProcessingStatus { get; set; }

        // Cow Health
        public string? CowHealthStatus { get; set; }
        public string? UdderCondition { get; set; }
        public string? VeterinaryNotes { get; set; }
    }


    
    public class RawMilkRequest
    {
        // Collection
        public DateTime CollectionDate { get; set; }
        public string Shift { get; set; }
        public decimal QtyLiters { get; set; }
        public string CollectorName { get; set; }
        public string MilkingMethod { get; set; }
        public string? MilkingStation { get; set; }

        // Quality Parameters
        public decimal Temperature { get; set; }
        public decimal FatPercent { get; set; }
        public decimal SNFPercent { get; set; }
        public decimal Density { get; set; }
        public decimal? ProteinPercent { get; set; }
        public decimal? LactosePercent { get; set; }
        public decimal Acidity { get; set; }
        public decimal FreezingPoint { get; set; }
        public decimal AddedWaterPercent { get; set; }
        public int BacteriaCount { get; set; }
        public int SomaticCellCount { get; set; }

        // Safety
        public bool IsAntibioticPositive { get; set; }
        public bool IsSpoiled { get; set; }
        public string? OdorCheck { get; set; }
        public string? ColorCheck { get; set; }
        public string? ContaminationNotes { get; set; }

        // Pricing
        public decimal RatePerLiter { get; set; }
        public decimal AmountPaid { get; set; }
        public string? SupplierId { get; set; }
        public string PaymentStatus { get; set; }

        // Storage
        public string? BatchNumber { get; set; }
        public bool TransferredToBulkTank { get; set; }
        public string? BulkTankNumber { get; set; }
        public DateTime? TransferTime { get; set; }
        public DateTime? CoolingTime { get; set; }
        public string ProcessingStatus { get; set; }

        // Cow Health
        public string? CowHealthStatus { get; set; }
        public string? UdderCondition { get; set; }
        public string? VeterinaryNotes { get; set; }
    }


    