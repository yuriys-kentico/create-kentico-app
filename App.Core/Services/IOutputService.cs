namespace App.Core.Services
{
    public interface IOutputService
    {
        object ProgressBar(int size, string? message = null);

        void UpdateProgress(object progressBarObject, int amount);

        void Display(string message);
    }
}