using Microsoft.AspNetCore.Components.Forms;

namespace Innowise.Music.Admin.Models;

public class SelectedFile
{
    public string Name { get; set; } = string.Empty;
    public long Length { get; set; }
    public IBrowserFile? BrowserFile { get; set; }
}
