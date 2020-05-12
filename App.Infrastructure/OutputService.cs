using System;

using App.Core.Services;

using ShellProgressBar;

namespace App.Infrastructure
{
    public class OutputService : IOutputService
    {
        public object ProgressBar(int size, string? message = default)
        {
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Gray,
                ProgressCharacter = '\u2593',
                ProgressBarOnBottom = true
            };

            return new ProgressBar(size, message, options);
        }

        public void UpdateProgress(object progressBarObject, int amount)
        {
            if (progressBarObject is ProgressBar progressBar)
            {
                progressBar.Tick(progressBar.CurrentTick + amount);

                if (progressBar.Percentage == 100)
                {
                    progressBar.Dispose();
                }
            }
        }

        public void Display(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine($"{DateTime.Now:[yyyy-MM-dd HH:mm:ss]} {message}");
            }
        }
    }
}