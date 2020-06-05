namespace App.Boilerplate.Core.Models
{
    public class NodeRouteData
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string ControllerName { get; set; }

        public object Node { get; set; }
    }
}