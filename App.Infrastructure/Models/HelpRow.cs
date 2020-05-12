using System;
using System.Collections.Generic;
using System.Linq;

namespace App.Infrastructure.Models
{
    internal class HelpRow
    {
        public string Name { get; set; } = string.Empty;

        public string Aliases { get; set; } = string.Empty;

        public string Type { get; set; } = nameof(String);

        public string? Description { get; set; }
    }
}