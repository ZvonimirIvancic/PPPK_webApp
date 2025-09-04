namespace PrviProjekt.Services
{
    public interface IExportService
    {

            Task<string> ExportPacijentiToCSVAsync();


            Task<byte[]> ExportPacijentiToPDFAsync();


            Task<string> ExportPreglediToCSVAsync(int? pacijentId = null, DateTime? fromDate = null, DateTime? toDate = null);

            Task<string> ExportReceptiToCSVAsync(int? pacijentId = null, bool? activeOnly = null);


            Task<string> ExportMedicinskaDokumentacijaToCSVAsync(int? pacijentId = null, bool? activeOnly = null);


            Task<string> ExportSummaryReportToCSVAsync();
        
    }
}
