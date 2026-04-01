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
            Console.WriteLine($"Error extracting metadata from {fileName}: {ex.Message}");
        }
        
        return metadata;
    }
    
    public async Task<IEnumerable<ExtractedTrackMetadata>> ExtractMetadataBatchAsync(
        IEnumerable<(Stream Stream, string FileName)> files)
    {
        var results = new List<ExtractedTrackMetadata>();
        
        foreach (var (stream, fileName) in files)
        {
            try
            {
                var metadata = await ExtractMetadataAsync(stream, fileName);
                results.Add(metadata);
            }
            catch (Exception ex)
            {
                // Add a placeholder result for failed extractions
                results.Add(new ExtractedTrackMetadata
                {
                    FileName = fileName,
                    Title = Path.GetFileNameWithoutExtension(fileName),
                    AudioFormat = Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant()
                });
                Console.WriteLine($"Failed to extract metadata from {fileName}: {ex.Message}");
            }
        }
        
        return results;
    }
}
