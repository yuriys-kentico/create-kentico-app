namespace App.Boilerplate.Core.Widgets
{
    public interface IUserWidgetsService
    {
        void RegisterAll();

        void Add(string identifier);

        void Remove(string identifier);
    }
}