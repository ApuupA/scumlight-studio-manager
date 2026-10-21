using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// The finance archive: one ZIP per year with CSV lists, every issued invoice as HTML and all receipts -
    /// readable without the app. Written every night into the file storage, so the backup takes it along.
    /// </summary>
    public interface IFinanceArchiveService
    {
        /// <summary>Creates the archive of a year fresh, saves it and returns it for downloading.</summary>
        Task<FileDownload> CreateArchive(int year);

        /// <summary>
        /// Writes the archive of the current year - and in January to March also of the previous year,
        /// which is still being completed (late payments, receipts). Called every night.
        /// </summary>
        Task WriteCurrentArchives();

        /// <summary>The last backup and the archives that exist.</summary>
        Task<DataSafetyStatusDto> GetStatus();
    }
}
