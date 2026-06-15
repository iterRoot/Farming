using AutoMapper;

namespace FarmingApi.Modules.Administration.ElectronicFileManager;

public class ElectronicFileMapper : Profile
{
    public ElectronicFileMapper()
    {
        CreateMap<ElectronicFile, ElectronicFileResponse>()
            .ForMember(d => d.FileSizeDisplay,
                opt => opt.MapFrom(s => FormatSize(s.FileSizeBytes)))
            .ForMember(d => d.IsExpired,
                opt => opt.MapFrom(s => s.ExpiresAt.HasValue && s.ExpiresAt < DateTime.UtcNow))
            .ForMember(d => d.IsImage,
                opt => opt.MapFrom(s => IsImageMime(s.MimeType)))
            .ForMember(d => d.FileTypeIcon,
                opt => opt.MapFrom(s => GetIcon(s.FileExtension)))
            .ForMember(d => d.Versions,
                opt => opt.MapFrom(s => s.Versions.OrderByDescending(v => v.VersionNumber)));

        CreateMap<FileVersion, FileVersionResponse>();

        CreateMap<ElectronicFileRequest, ElectronicFile>()
            .ForMember(d => d.Id,             opt => opt.Ignore())
            .ForMember(d => d.FileCode,       opt => opt.Ignore())
            .ForMember(d => d.FileName,       opt => opt.Ignore())
            .ForMember(d => d.FileHash,       opt => opt.Ignore())
            .ForMember(d => d.DownloadCount,  opt => opt.Ignore())
            .ForMember(d => d.LastDownloadAt, opt => opt.Ignore())
            .ForMember(d => d.CurrentVersion, opt => opt.Ignore())
            .ForMember(d => d.IsArchived,     opt => opt.Ignore())
            .ForMember(d => d.UploadedAt,     opt => opt.Ignore())
            .ForMember(d => d.Versions,       opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,      opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,      opt => opt.Ignore())
            .ForMember(d => d.InActive,       opt => opt.Ignore());
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)             return $"{bytes} B";
        if (bytes < 1024 * 1024)     return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }

    private static bool IsImageMime(string mime) =>
        mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    private static string GetIcon(string ext) => ext.ToLower() switch
    {
        ".pdf"  => "📕",
        ".xlsx" or ".xls" => "📗",
        ".docx" or ".doc" => "📘",
        ".pptx" or ".ppt" => "📙",
        ".jpg"  or ".jpeg" or ".png" or ".gif" or ".webp" => "🖼️",
        ".zip"  or ".rar" or ".7z" => "🗜️",
        ".xml"  or ".json" => "📄",
        ".csv"  => "📊",
        ".mp4"  or ".avi" or ".mov" => "🎬",
        ".mp3"  or ".wav" => "🎵",
        _       => "📎",
    };
}