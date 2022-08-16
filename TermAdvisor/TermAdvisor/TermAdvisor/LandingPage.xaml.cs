using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using TermAdvisor.Model;
using Plugin.LocalNotifications;

namespace TermAdvisor
{
    public partial class NavigationPage : ContentPage
    {
        private bool OnStart = true;
        public NavigationPage()
        {
            InitializeComponent();
        }
        private async void RefreshPage()
        {
            termListView.ItemsSource = await Services.SQLite.GetTerms();
        }
        private async void termListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var term = (Term)e.Item;
            await Navigation.PushAsync(new View.TermDetails(term));
        }
        private async void AddTerm_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new View.TermAdd());
        }
        protected override async void OnAppearing()
        {
            await Services.SQLite.Database();
            var terms = await Services.SQLite.GetTerms();
            if (OnStart && !terms.Any())
            {
                await Services.SQLite.CreateTables();
                await Alerts();
                OnStart = false;
            }
            else if (OnStart)
            {
                await Alerts();
                OnStart = false;
            }
            RefreshPage();
            base.OnAppearing();
        }
        private async Task Alerts()
        {
            var db = Services.SQLite.db;
            string notices = "";
            var today = DateTime.Today.ToString("MM/dd/yyyy");
            if (OnStart)
            {
                var terms = await Services.SQLite.GetTerms();
                foreach (Term term in terms)
                {
                    var courses = await Services.SQLite.GetCourseList(term.Id);
                    foreach (Course course in courses)
                    {
                        var start = course.StartDate.ToString("MM/dd/yyyy");
                        var end = course.EndDate.ToString("MM/dd/yyyy");
                        if (course.NotificationOn && today == start)
                        {
                            notices += $"{course.Name} starts on {course.StartDate.ToString("MM/dd/yyyy")}\n";
                        }
                        if (course.NotificationOn && today == end)
                        {
                            notices += $"{course.Name} ends on {course.EndDate.ToString("MM/dd/yyyy")}\n";
                        }
                        var assessments = await Services.SQLite.GetAssessementList(course.Id);
                        foreach (Assessment assessment in assessments)
                        {
                            var assessStart = assessment.StartDate.ToString("MM/dd/yyyy");
                            var assessEnd = assessment.EndDate.ToString("MM/dd/yyyy");
                            if (assessment.NotificationOn && today == assessStart)
                            {
                                notices += $"{assessment.Name} starts on {assessment.StartDate.ToString("MM/dd/yyyy")}\n";
                            }
                            if (assessment.NotificationOn && today == assessEnd)
                            {
                                notices += $"{assessment.Name} ends on {assessment.EndDate.ToString("MM/dd/yyyy")}\n";
                            }
                        }
                    }
                }
                if(!String.IsNullOrEmpty(notices))
                {
                    CrossLocalNotifications.Current.Show("Attention", $"{notices}");
                }
            }
            OnStart = false;
        }
    }
}
