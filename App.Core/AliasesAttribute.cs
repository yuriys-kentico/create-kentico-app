using System;

namespace App.Core
{
    public sealed class AliasesAttribute : Attribute
    {
        public string[] Aliases { get; set; }

        public AliasesAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }
    }
}