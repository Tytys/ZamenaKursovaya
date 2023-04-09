using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ZamenaKursovaya
{

    public class TeacherNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string teacherName = value as string;
            if (string.IsNullOrEmpty(teacherName))
            {
                return "Substitute Teacher";
            }
            else
            {
                return teacherName;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        
    }

    public class Lesson
    {
        public string Name { get; set; }
        public string Room { get; set; }
        public string Time { get; set; }
        public string Teacher { get; set; }
        public int DayOfWeek { get; set; }
        public int LessonNumber { get; set; }
    }

    public class SchoolContext : DbContext
    {
        public DbSet<Lesson> Lessons { get; set; }
    }

    public class ScheduleViewModel
    {
        public ObservableCollection<Lesson>[] Schedule { get; set; }

        public ScheduleViewModel(string groupname)
        {
            Schedule = new ObservableCollection<Lesson>[5];
            for (int i = 0; i < Schedule.Length; i++)
            {
                Schedule[i] = new ObservableCollection<Lesson>();
                for (int j = 0; j < 4; j++)
                {
                    Schedule[i].Add(new Lesson());
                }
            }

            using (var db = new DBEntities())
            {
                var lessons = db.Lessons.ToList();
                foreach (var lesson in lessons)
                {
                    if (lesson.GroupName == groupname)
                    {

                        int row = GetRowForDay(lesson.DayOfWeek);
                        int col = GetColumnForLesson(lesson.LessonNumber);

                        Schedule[row][col].Name = lesson.Name;
                        Schedule[row][col].Room = lesson.Room;
                        Schedule[row][col].Time = lesson.Time;
                        Schedule[row][col].Teacher = lesson.Teacher;
                    }
                }
            }
        }

        private int GetRowForDay(int day)
        {
            switch (day)
            {
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 3:
                    return 2;
                case 4:
                    return 3;
                case 5:
                    return 4;
                default:
                    throw new ArgumentException("Invalid day of week");
            }
        }

        private int GetColumnForLesson(int lessonNumber)
        {
            switch (lessonNumber)
            {
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 3:
                    return 2;
                case 4:
                    return 3;
                default:
                    throw new ArgumentException("Invalid lesson number");
            }
        }
        public void ReplaceTeacherOnAbsence(DayOfWeek dayOfWeek, int lessonNumber)
        {
            Lesson lesson = Schedule[(int)dayOfWeek][lessonNumber];
            if (lesson != null && string.IsNullOrEmpty(lesson.Teacher))
            {
                lesson.Teacher = "Substitute Teacher";
            }
        }

        public void replace(string absentTeacher, string replacementTeacherPairName)
        {
            var ls = DBEntities.GetContext().Lessons.Select(x => x).Where(x => x.Name == replacementTeacherPairName).ToList();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (Schedule[i][j] != null && Schedule[i][j].Teacher == absentTeacher)
                    {
                        foreach (var item in ls)
                        {
                            if (item.LessonNumber == j+1)
                            {
                                MessageBox.Show("Данную пару нельзя поставить как замену");
                                break;
                            }
                        }
                        Schedule[i][j].Name = replacementTeacherPairName;
                    }
                }
            }          
            var scheduleView = CollectionViewSource.GetDefaultView(Schedule);
            scheduleView.Refresh();
        }
    }

}
