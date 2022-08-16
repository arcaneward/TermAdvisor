using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using System.Threading.Tasks;
using TermAdvisor.Model;
using System.Collections.ObjectModel;
using System.Net.Mail;

namespace TermAdvisor.Services
{
    public static class SQLite
    {
        public static SQLiteAsyncConnection db;
        public static bool Firstrun = true;
        public static async Task Database()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TermAdvisor_db.db");
            db = new SQLiteAsyncConnection(databasePath);
        }
        public static async Task CreateTables()
        {
            await db.DropTableAsync<Term>();
            await db.DropTableAsync<Course>();
            await db.DropTableAsync<Assessment>();
            await db.CreateTableAsync<Term>();
            await db.CreateTableAsync<Course>();
            await db.CreateTableAsync<Assessment>();
            if (Firstrun)
            {
                await TestData();
                Firstrun = false;
            }
        }
        public static async Task AddTerm(string name, DateTime startDate, DateTime endDate)
        {
            var term = new Term
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate
            };
            var id = await db.InsertAsync(term);
        }
        public static async Task RemoveTerm(int id)
        {
            await db.DeleteAsync<Term>(id);
        }
        public static async Task<IEnumerable<Term>> GetTerms()
        {
            await db.CreateTableAsync<Term>();
            var termsFilter = await db.QueryAsync<Term>("SELECT * FROM Terms");
            var terms = new ObservableCollection<Term>(termsFilter);
            return terms;
        }
        public static async Task TestData()
        {
            var term1 = new Term
            {
                Name = "Term 1",
                StartDate = DateTime.Today.AddDays(-5),
                EndDate = DateTime.Today.AddDays(+10),
            };
            await db.InsertAsync(term1);
            var course = new Course()
            {
                TermId = 1,
                Name = "Mobile Application Development",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(+9),
                Status = "In-Progress",
                InstructorName = "Lindsey Thornton",
                InstructorPhone = "555-867-5309",
                InstructorEmail = "lthor71@wgu.edu",
                Notes = "Enter notes here",
                NotificationOn = true
            };
            await db.InsertAsync(course);
            var perfAssess = new Assessment
            {
                CourseId = 1,
                Name = "Xamarin project",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(+6),
                Type = "Performance Assessment",
                NotificationOn = true
            };
            var objAssess = new Assessment
            {
                CourseId = 1,
                Name = "Practice test",
                StartDate = DateTime.Today.AddDays(+2),
                EndDate = DateTime.Today.AddDays(+3),
                Type = "Objective Assessment",
                NotificationOn = true
            };
            await db.InsertAsync(perfAssess);
            await db.InsertAsync(objAssess);
        }
        public static async Task AddCourse(int termId, string name, DateTime startDate, DateTime endDate, string status, string instructorName, string instructorPhone, string instructorEmail, string notes, bool notificationOn)
        {
            var course = new Course()
            {
                TermId = termId,
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                InstructorName = instructorName,
                InstructorPhone = instructorPhone,
                InstructorEmail = instructorEmail,
                Notes = notes,
                NotificationOn = notificationOn
            };
            var id = await db.InsertAsync(course);
        }
        public static async Task RemoveCourse(int id)
        {
            await db.DeleteAsync<Course>(id);
        }
        public static async Task<IEnumerable<Course>> GetCourseList(int termId)
        {
            await db.CreateTableAsync<Course>();
            var coursesFiltered = await db.QueryAsync<Course>($"SELECT * FROM COURSES WHERE TermId = '{termId}'");
            var courseList = new ObservableCollection<Course>(coursesFiltered);
            return courseList;
        }
        public static async Task AddAssessment(int courseId, string name, DateTime start, DateTime end, string type, bool notify)
        {
            var assessment = new Assessment()
            {
                CourseId = courseId,
                Name = name,
                StartDate = start,
                EndDate = end,
                Type = type,
                NotificationOn = notify
            };
            var id = await db.InsertAsync(assessment);
        }
        public static async Task RemoveAssessment(int id)
        {
            await db.DeleteAsync<Assessment>(id);
        }
        public static async Task<IEnumerable<Assessment>> GetAssessementList(int courseId)
        {
            await db.CreateTableAsync<Model.Assessment>();
            var assessmentFiltered = await db.QueryAsync<Assessment>($"SELECT * FROM Assessments WHERE CourseId = '{courseId}'");
            var assessmentList = new ObservableCollection<Assessment>(assessmentFiltered);
            return assessmentList;
        }
        public static bool CheckDate(DateTime start, DateTime end)
        {
            bool isValid = true;
            int compare = DateTime.Compare(start, end);
            if (compare > 0 || compare == 0)
            {
                isValid = false;
            }
            return isValid;

        }
        public static async Task<bool> AssessmentTypeAdd(string type, int courseId)
        {
            bool exists = false;
            var currentAssessments = await db.QueryAsync<Assessment>($"SELECT * FROM Assessments WHERE CourseId = '{courseId}' AND Type = '{type}'");
            if (currentAssessments.Count > 0)
            {
                exists = true;
            }
            return exists;
        }
        public static async Task<bool> AssessmentTypeEdit(string type, int courseId, int assessId)
        {
            bool exists = false;
            var currentAssessments = await db.QueryAsync<Assessment>($"SELECT * FROM Assessments WHERE CourseId = '{courseId}' AND Type = '{type}' AND Id != '{assessId}'");
            if (currentAssessments.Count > 0)
            {
                exists = true;
            }
            return exists;
        }
        public static bool AssessmentValidation(string name, DateTime start, DateTime end)
        {
            bool valid = false;
            if (String.IsNullOrEmpty(name))
            {
                valid = true;

            }
            return valid;
        }
        public static async Task<bool> AssessmentValidation2(int courseId)
        {
            bool bothAssessmentsExist = false;
            var currentAssessments = await db.QueryAsync<Assessment>($"SELECT * FROM Assessments WHERE (CourseId = '{courseId}') AND (Type LIKE '%Perf%' OR Type LIKE '%Obj%')");
            if (currentAssessments.Count == 2)
            {
                bothAssessmentsExist = true;
            }
            return bothAssessmentsExist;
        }
        public static bool CourseValidation(string name, string instN, string instP, string instE)
        {
            bool valid = false;
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(instN) || String.IsNullOrEmpty(instP) || String.IsNullOrEmpty(instE))
            {
                valid = true;
            }
            return valid;
        }
        public static async Task<bool> CourseValidation2(int termId)
        {
            bool isMax = false;
            var courseCount = await db.QueryAsync<Course>($"SELECT * FROM Courses WHERE TermId = '{termId}'");
            if (courseCount.Count >= 6)
            {
                isMax = true;
            }
            return isMax;
        }
        public static bool EmailValidation(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public static bool TermValidation(string name, DateTime start, DateTime end)
        {
            bool valid = false;
            if (String.IsNullOrEmpty(name) || start.Date == null || end.Date == null)
            {
                valid = true;
            }
            return valid;
        }
    }
}