using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TermAdvisor.Model;

namespace TermAdvisor.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CourseAdd : ContentPage
    {
        public Term SelectedTerm;

        public CourseAdd()
        {
            InitializeComponent();
        }

        public CourseAdd(Term term)
        {
            InitializeComponent();
            SelectedTerm = term;
        }

        protected override async void OnAppearing()
        {
            status.SelectedItem = "In-Progress";
            base.OnAppearing();
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            int termid = SelectedTerm.Id;
            string name = nameEntry.Text;
            DateTime start = startDate.Date;
            DateTime end = endDate.Date;
            string statuss = status.SelectedItem.ToString();
            string instN = instructorName.Text;
            string instP = instructorPhone.Text;
            string instE = instructorEmail.Text;
            string note = notes.Text;
            bool notify = notification.IsToggled;


            if (Services.SQLite.CourseValidation(name, instN, instP, instE))
            {
                DisplayAlert($"Alert", "Some fields are missing input", "Ok");
            }
            else if (!Services.SQLite.CheckDate(start, end))
            {
                DisplayAlert($"Alert", "Start date must be before End date", "Ok");
            }
            else if (!Services.SQLite.EmailValidation(instE))
            {
                DisplayAlert($"Alert", "Enter a valid email address", "Ok");
            }
            else
            {
                await Services.SQLite.AddCourse(termid, name, start, end, statuss, instN, instP, instE, note, notify);
                await Navigation.PopAsync();
            }
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}