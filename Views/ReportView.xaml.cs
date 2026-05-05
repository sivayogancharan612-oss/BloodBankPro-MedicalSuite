using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System;
using System.Linq;

namespace BloodBankPro.Views
{
    public partial class ReportView : UserControl
    {
        public ReportView() => InitializeComponent();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var raw = DatabaseHelper.GetBloodTypeReport();
            const double MaxBar = 360.0;
            int maxUnits = raw.Count > 0 ? Math.Max(raw.Max(x => x.UnitsInStock), 1) : 1;

            var rows = raw.Select(r => new
            {
                r.BloodType, r.ActiveDonors, r.TotalDonations,
                r.UnitsInStock, r.TotalRequests, r.FulfilledRequests,
                r.FulfillRateText, r.FulfillRateColor, r.StockColor,
                RateBarWidth = r.FulfillRate / 100.0 * 160.0,
                BarWidth     = r.UnitsInStock / (double)maxUnits * MaxBar
            }).ToList();

            DgReport.ItemsSource  = rows;
            StockBars.ItemsSource = rows;
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"DailyReport_{DateTime.Now:yyyyMMdd}.pdf",
                DefaultExt = ".pdf",
                Filter = "PDF Documents (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    GeneratePdfReport(dlg.FileName);
                    DatabaseHelper.Log("EXPORT", "Reports", "Daily PDF report generated and downloaded.");
                    MessageBox.Show("PDF Report generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GeneratePdfReport(string filePath)
        {
            var writer = new PdfWriter(filePath);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            document.Add(new Paragraph("Blood Bank Pro - Daily Operations Report")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                
            document.Add(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}")
                .SetFontSize(12)
                .SetMarginBottom(20));

            var table = new Table(new float[] { 1, 1, 1, 1 }).UseAllAvailableWidth();
            table.AddHeaderCell(new Cell().Add(new Paragraph("Blood Type").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Units in Stock").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Active Donors").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fulfilled Requests").SetFont(boldFont)));

            var reportData = DatabaseHelper.GetBloodTypeReport();
            foreach (var row in reportData)
            {
                table.AddCell(new Paragraph(row.BloodType));
                table.AddCell(new Paragraph(row.UnitsInStock.ToString()));
                table.AddCell(new Paragraph(row.ActiveDonors.ToString()));
                table.AddCell(new Paragraph(row.FulfilledRequests.ToString()));
            }

            document.Add(table);
            document.Close();
        }
    }
}
