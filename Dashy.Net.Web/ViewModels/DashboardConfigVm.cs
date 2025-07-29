namespace Dashy.Net.Web.ViewModels
{
    public class DashboardConfigVm
    {
        public int Id { get; set; } = 1;
        public string Title { get; set; } = "Dashy.Net";
        public List<SectionVm> Sections { get; set; } = new();
    }
}
