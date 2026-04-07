/*
 * @file: MetadataExtractionService.cs
 * @description: Service for extracting metadata from audio files using TagLibSharp
 * @dependencies: TagLibSharp, ExtractedTrackMetadata model
 * @created: 2026-04-01
 */

using Innowise.Music.Admin.Models;

namespace Innowise.Music.Admin.Services;

public class MetadataExtractionService : IMetadataExtractionService
{
    private static readonly string[] SupportedExtensions = { ".mp3", ".aac", ".flac", ".wav" };
    private readonly ILogger<MetadataExtractionService> _logger;

    public MetadataExtractionService(ILogger<MetadataExtractionService> logger)
    {
        _logger = logger;
    }

    public async Task<ExtractedTrackMetadata> ExtractMetadataAsync(Stream fileStream, string fileName)
    {
        var metadata = new ExtractedTrackMetadata
        {
            FileName = fileName,
            AudioFormat = Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant()
        };
        
        // Read file into byte array
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        metadata.AudioData = memoryStream.ToArray();
        metadata.FileSize = metadata.AudioData.Length;
        
        // Extract metadata using TagLibSharp
        try
        {
            // Create a temporary file for TagLibSharp to read
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + Path.GetExtension(fileName));
            await File.WriteAllBytesAsync(tempFile, metadata.AudioData);
            
            try
            {
                using var file = TagLib.File.Create(tempFile);
                
                // Extract tag information
                if (file.Tag != null)
                {
                    metadata.Title = string.IsNullOrWhiteSpace(file.Tag.Title) 
                        ? Path.GetFileNameWithoutExtension(fileName) 
                        : file.Tag.Title;
                    
                    metadata.Performers = file.Tag.Performers ?? Array.Empty<string>();
                    metadata.Album = file.Tag.Album ?? string.Empty;
                    metadata.Genres = file.Tag.Genres ?? Array.Empty<string>();
                    metadata.Year = file.Tag.Year;
                    metadata.TrackNumber = file.Tag.Track;
                }
                else
                {
                    metadata.Title = Path.GetFileNameWithoutExtension(fileName);
                }
                
                // Extract audio properties
                if (file.Properties != null)
                {
                    metadata.Duration = (int)file.Properties.Duration.TotalSeconds;
                    metadata.Bitrate = file.Properties.AudioBitrate;
                    metadata.SampleRate = file.Properties.AudioSampleRate;
                }
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (Exception ex)
        {
            // If metadata extraction fails, use filename as title
            metadata.Title = Path.GetFileNameWithoutExtension(fileName);
            // Log the error but don't fail the entire operation
            _logger.LogWarning(ex, "Failed to extract metadata from {FileName}", fileName);
        }
        
        return metadata;
    }
    
    public async Task<IEnumerable<ExtractedTrackMetadata>> ExtractMetadataBatchAsync(
        IEnumerable<(Stream Stream, string FileName)> files)
    {
        var results = new List<ExtractedTrackMetadata>();
        var fileList = files.ToList();
        
        // Process files in parallel with limited concurrency
        var tasks = fileList.Select(async (file, index) =>
        {
            try
            {
                var metadata = await ExtractMetadataAsync(file.Stream, file.FileName);
                return (Index: index, Metadata: metadata, Success: true);
            }
            catch
            {
                return (Index: index, Metadata: new ExtractedTrackMetadata
                {
                    FileName = file.FileName,
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    AudioFormat = Path.GetExtension(file.FileName).TrimStart('.').ToUpperInvariant()
                }, Success: false);
            }
        });
        
        var taskResults = await Task.WhenAll(tasks);
        
        // Order results by original index to maintain file order
        foreach (var result in taskResults.OrderBy(r => r.Index))
        {
            results.Add(result.Metadata);
        }
        
        return results;
    }
}
