namespace Dashy.Net.Web.ViewModels
{
    public class DashboardConfigVm
    {
        public string Title { get; set; } = "Dashy.Net";
        public List<SectionVm> Sections { get; set; } = new();
    }
}
