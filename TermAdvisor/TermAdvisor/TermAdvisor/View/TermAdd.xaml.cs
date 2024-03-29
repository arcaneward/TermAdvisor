﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TermAdvisor.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TermAdd : ContentPage
    {
        public TermAdd()
        {
            InitializeComponent();
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            string name = nameEntry.Text;
            DateTime start = startDate.Date;
            DateTime end = endDate.Date;
            if (!Services.SQLite.CheckDate(start, end))
            {
                DisplayAlert($"Alert", "Start date must be before End date", "Ok");
            }
            else if (Services.SQLite.TermValidation(name, start, end))
            {
                DisplayAlert($"Alert", "Some fields are missing input", "Ok");
            }
            else
            {
                await Services.SQLite.AddTerm(name, start, end);
                await Navigation.PopAsync();
            }
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}