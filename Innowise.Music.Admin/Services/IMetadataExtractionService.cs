/*
 * @file: IMetadataExtractionService.cs
 * @description: Interface for metadata extraction from audio files
 * @dependencies: ExtractedTrackMetadata model
 * @created: 2026-04-01
 */

using Innowise.Music.Admin.Models;

namespace Innowise.Music.Admin.Services;

public interface IMetadataExtractionService
{
    Task<ExtractedTrackMetadata> ExtractMetadataAsync(Stream fileStream, string fileName);
    Task<IEnumerable<ExtractedTrackMetadata>> ExtractMetadataBatchAsync(
        IEnumerable<(Stream Stream, string FileName)> files);
}
