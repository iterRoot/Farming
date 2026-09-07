using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

public class GoodsReceiptPOPdfDocument : IDocument
{
    private readonly GoodsReceiptPO _receipt;
    private readonly CompanyEntity? _company;

    public GoodsReceiptPOPdfDocument(GoodsReceiptPO receipt, CompanyEntity? company)
    {
        _receipt = receipt;
        _company = company;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(_company?.Name ?? "Company Name")
                    .FontSize(16).Bold();

                if (!string.IsNullOrWhiteSpace(_company?.Address))
                    col.Item().Text(_company!.Address).FontSize(9).FontColor(Colors.Grey.Darken1);

                if (!string.IsNullOrWhiteSpace(_company?.LocalEmail))
                    col.Item().Text(_company!.LocalEmail!).FontSize(9).FontColor(Colors.Grey.Darken1);
            });

            row.ConstantItem(200).Column(col =>
            {
                col.Item().AlignRight().Text("GOODS RECEIPT PO")
                    .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

                col.Item().AlignRight().Text($"Doc No: {_receipt.DocNum}")
                    .FontSize(10).Bold();

                col.Item().AlignRight().Text(_receipt.Status == "C" ? "Closed" : "Open")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(15).Column(col =>
        {
            col.Item().Element(ComposeVendorInfo);
            col.Item().PaddingTop(15).Element(ComposeLineItemsTable);
            col.Item().PaddingTop(15).Element(ComposeFooterBlock);
        });
    }

    private void ComposeVendorInfo(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("VENDOR").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                col.Item().Text(_receipt.Vendor.CardName).Bold();
                col.Item().Text($"Vendor Code: {_receipt.Vendor.Code}").FontSize(9);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Posting Date").FontSize(9).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().AlignRight().Text(_receipt.PostingDate?.ToString("dd/MM/yyyy") ?? "-");
                });
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Document Date").FontSize(9).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().AlignRight().Text(_receipt.DocDate?.ToString("dd/MM/yyyy") ?? "-");
                });
            });
        });
    }

    private void ComposeLineItemsTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);
                columns.RelativeColumn(4);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(1.3f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("#");
                header.Cell().Element(HeaderCellStyle).Text("Item");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Qty");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Price");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total");

                static IContainer HeaderCellStyle(IContainer c) => c
                    .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                    .Background(Colors.Blue.Darken2)
                    .Padding(6);
            });

            var i = 0;
            foreach (var line in _receipt.Lines)
            {
                i++;
                var idx = i;

                table.Cell().Element(BodyCellStyle(idx)).Text(idx.ToString());
                table.Cell().Element(BodyCellStyle(idx)).Text($"{line.ItemCode} — {line.ItemName}");
                table.Cell().Element(BodyCellStyle(idx)).AlignRight().Text(line.Quantity.ToString("N2"));
                table.Cell().Element(BodyCellStyle(idx)).AlignRight().Text(line.Price.ToString("N2"));
                table.Cell().Element(BodyCellStyle(idx)).AlignRight().Text(line.Total.ToString("N2"));
            }

            Func<IContainer, IContainer> BodyCellStyle(int rowIndex) => container => container
                .Background(rowIndex % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White)
                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(6);
        });
    }

    private void ComposeFooterBlock(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("REMARKS").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                col.Item().Text(string.IsNullOrWhiteSpace(_receipt.Remarks) ? "-" : _receipt.Remarks!);
            });

            row.ConstantItem(220).Column(col =>
            {
                SummaryLine(col, "Subtotal", _receipt.Lines.Sum(l => l.Total));
                SummaryLine(col, "Discount ($)", _receipt.Discount);
                SummaryLine(col, "Tax (%)", _receipt.Tax);
                SummaryLine(col, "Tax Amount", _receipt.TaxAmount);

                col.Item().PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Lighten1);

                col.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Grand Total").Bold();
                    r.RelativeItem().AlignRight().Text($"${_receipt.Total:N2}").Bold().FontSize(12);
                });
            });
        });
    }

    private static void SummaryLine(ColumnDescriptor col, string label, decimal value)
    {
        col.Item().Row(r =>
        {
            r.RelativeItem().Text(label).FontColor(Colors.Grey.Darken1);
            r.RelativeItem().AlignRight().Text(value.ToString("N2"));
        });
    }
}
