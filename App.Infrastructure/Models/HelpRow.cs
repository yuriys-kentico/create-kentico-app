namespace App.Infrastructure.Models
{
    internal class HelpRow
    {
        public string Name { get; set; } = string.Empty;

        public string? Aliases { get; set; }

        public string? Type { get; set; }

        public string? Required { get; set; }

        public string? Description { get; set; }
    }
}