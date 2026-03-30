namespace FarmingApi.Modules.Sale.SaleInvoice;

public class SaleInvoiceListResponse
{
public string ItemsCode { get; set; } = null!;
    public string? ItemsName { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal DisPct { get; set; }
    public decimal DisSum { get; set; }

    public decimal PriceBfDis { get; set; }
    public decimal PriceAtDis { get; set; }
    public decimal PriceAtVat { get; set; }

    public decimal Quantity { get; set; }
    public decimal InvQty { get; set; }

    public decimal LineTotalLC { get; set; }
    public decimal LineTotalFC { get; set; }
    public decimal LineTotalSys { get; set; }

    public decimal TotalLC { get; set; }
    public decimal TotalFC { get; set; }
    public decimal TotalSys { get; set; }

    public decimal LineStatus { get; set; }
    public decimal ItemsCost { get; set; }
    public decimal GTotal { get; set; }

    public decimal NetPrice { get; set; }
    public decimal GrossPrice { get; set; }

    public int BaseEntry { get; set; }

    public string? UomName { get; set; }
    public string? UomCode { get; set; }

    public string? Price { get; set; }
    public string? PriceList { get; set; }

    public string? Types { get; set; }
    public string? FreeTxt { get; set; }
    public string? Remarks { get; set; }

}

public class SaleInvoiceListRequest
{
    public string ItemsCode { get; set; } = null!;
    public string? ItemsName { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal DisPct { get; set; }
    public decimal DisSum { get; set; }

    public decimal PriceBfDis { get; set; }
    public decimal PriceAtDis { get; set; }
    public decimal PriceAtVat { get; set; }

    public decimal Quantity { get; set; }
    public decimal InvQty { get; set; }

    public decimal LineTotalLC { get; set; }
    public decimal LineTotalFC { get; set; }
    public decimal LineTotalSys { get; set; }

    public decimal TotalLC { get; set; }
    public decimal TotalFC { get; set; }
    public decimal TotalSys { get; set; }

    public decimal LineStatus { get; set; }
    public decimal ItemsCost { get; set; }
    public decimal GTotal { get; set; }

    public decimal NetPrice { get; set; }
    public decimal GrossPrice { get; set; }

    public int BaseEntry { get; set; }

    public string? UomName { get; set; }
    public string? UomCode { get; set; }

    public string? Price { get; set; }
    public string? PriceList { get; set; }

    public string? Types { get; set; }
    public string? FreeTxt { get; set; }
    public string? Remarks { get; set; }

}
public class SaleInvoiceUpdateRequest
{
public string ItemsCode { get; set; } = null!;
    public string? ItemsName { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal DisPct { get; set; }
    public decimal DisSum { get; set; }

    public decimal PriceBfDis { get; set; }
    public decimal PriceAtDis { get; set; }
    public decimal PriceAtVat { get; set; }

    public decimal Quantity { get; set; }
    public decimal InvQty { get; set; }

    public decimal LineTotalLC { get; set; }
    public decimal LineTotalFC { get; set; }
    public decimal LineTotalSys { get; set; }

    public decimal TotalLC { get; set; }
    public decimal TotalFC { get; set; }
    public decimal TotalSys { get; set; }

    public decimal LineStatus { get; set; }
    public decimal ItemsCost { get; set; }
    public decimal GTotal { get; set; }

    public decimal NetPrice { get; set; }
    public decimal GrossPrice { get; set; }

    public int BaseEntry { get; set; }

    public string? UomName { get; set; }
    public string? UomCode { get; set; }

    public string? Price { get; set; }
    public string? PriceList { get; set; }

    public string? Types { get; set; }
    public string? FreeTxt { get; set; }
    public string? Remarks { get; set; }


}