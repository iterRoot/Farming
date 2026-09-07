using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentRequest;

public class APDownPaymentRequestPdfDocument : IDocument
{
    private readonly APDownPaymentRequest _request;
    private readonly CompanyEntity?       _company;

    public APDownPaymentRequestPdfDocument(APDownPaymentRequest request, CompanyEntity? company)
    {
        _request = request;
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

            row.ConstantItem(210).Column(col =>
            {
                col.Item().AlignRight().Text("AP DOWN PAYMENT REQUEST")
                    .FontSize(15).Bold().FontColor(Colors.Blue.Darken2);

                col.Item().AlignRight().Text($"Doc No: {_request.DocNum}")
                    .FontSize(10).Bold();

                col.Item().AlignRight().Text(_request.Status == "C" ? "Closed" : "Open")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(15).Column(col =>
        {
            col.Item().Element(ComposeVendorInfo);
            col.Item().PaddingTop(20).Element(ComposeFooterBlock);
        });
    }

    private void ComposeVendorInfo(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("VENDOR").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                col.Item().Text(_request.Vendor.CardName).Bold();
                col.Item().Text($"Vendor Code: {_request.Vendor.Code}").FontSize(9);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Posting Date").FontSize(9).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().AlignRight().Text(_request.PostingDate?.ToString("dd/MM/yyyy") ?? "-");
                });
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Due Date").FontSize(9).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().AlignRight().Text(_request.DueDate?.ToString("dd/MM/yyyy") ?? "-");
                });
                if (_request.BaseEntry.HasValue)
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Base Document").FontSize(9).FontColor(Colors.Grey.Darken1);
                        r.RelativeItem().AlignRight().Text($"{_request.BaseType} #{_request.BaseEntry}");
                    });
                }
            });
        });
    }

    private void ComposeFooterBlock(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("REMARKS").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                col.Item().Text(string.IsNullOrWhiteSpace(_request.Remarks) ? "-" : _request.Remarks!);
            });

            row.ConstantItem(220).Column(col =>
            {
                col.Item().PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Lighten1);

                col.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Grand Total").Bold();
                    r.RelativeItem().AlignRight().Text($"${_request.Total:N2}").Bold().FontSize(12);
                });
            });
        });
    }
}
