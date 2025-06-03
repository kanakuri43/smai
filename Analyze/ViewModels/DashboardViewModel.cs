using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Analyze.Models;

namespace Analyze.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<int> _years;
        private int _selectedYear;
        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private int _period;
        private ObservableCollection<Calendar> _calendars;
        private Section _selectedSection;
        private Employee _selectedEmployee;
        private int _selectedProgressLevel;
        private ProgressLevel _progressLevelMin;
        private ProgressLevel _progressLevelMax;
        private ObservableCollection<ProgressLevel> _progressLevels;

        public ObservableCollection<int> Years
        {
            get { return _years; }
            set { SetProperty(ref _years, value); }
        }
        public int SelectedYear
        {
            get { return _selectedYear; }
            set { SetProperty(ref _selectedYear, value); }
        }
        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value); }
        }
        public ObservableCollection<Calendar> Calendars
        {
            get { return _calendars; }
            set { SetProperty(ref _calendars, value); }
        }
        public ObservableCollection<Section> Sections
        {
            get { return _sections; }
            set { SetProperty(ref _sections, value); }
        }
        public Section SelectedSection
        {
            get { return _selectedSection; }
            set { SetProperty(ref _selectedSection, value); }
        }
        public ObservableCollection<Employee> Employees
        {
            get { return _employees; }
            set { SetProperty(ref _employees, value); }
        }
        public ProgressLevel ProgressLevelMin
        {
            get { return _progressLevelMin; }
            set { SetProperty(ref _progressLevelMin, value); }
        }
        public int SelectedProgressLevel
        {
            get { return _selectedProgressLevel; }
            set { SetProperty(ref _selectedProgressLevel, value); }
        }
        public ProgressLevel ProgressLevelMax
        {
            get { return _progressLevelMax; }
            set { SetProperty(ref _progressLevelMax, value); }
        }
        public ObservableCollection<ProgressLevel> ProgressLevels
        {
            get { return _progressLevels; }
            set { SetProperty(ref _progressLevels, value); }
        }

        public DelegateCommand YearSelectionChanged { get; }
        public DelegateCommand MonthSelectionChanged { get; }
        public DelegateCommand SectionSelectionChanged { get; }
        public DelegateCommand EmployeeSelectionChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            YearSelectionChanged = new DelegateCommand(YearSelectionChangedExecute);
            MonthSelectionChanged = new DelegateCommand(MonthSelectionChangedExecute);
            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);

            // 年リスト
            int currentYear = DateTime.Now.Year;
            Years = new ObservableCollection<int>(Enumerable.Range(currentYear - 1, 3));
            this.SelectedYear = currentYear;

            using (var context = new AppDbContext())
            {
                // 今日の期を求める
                int todayDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                var c1 = context.Calendars.FirstOrDefault(c => c.Date == todayDate);
                if (c1 != null)
                {
                    this.Period = c1.Period;
                }
                // 期の最初の日付の年
                var c2 = context.Calendars
                           .Where(c => c.Period == this.Period)
                           .OrderBy(c => c.Date)
                           .FirstOrDefault();
                if (c2 != null)
                {
                    DateTime date = DateTime.ParseExact(c2.Date.ToString(), "yyyyMMdd", null);
                    this.SelectedYear = date.Year;
                }
                // 部署リスト
                Sections = new ObservableCollection<Section>(
                            context.Sections.Where(s => s.State == 0).ToList()
                        );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 21130);

                // 物権確度
                this.ProgressLevels = new ObservableCollection<ProgressLevel>(
                                context.ProgressLevels.Where(s => s.State == 0).ToList()
                            );

                var sortedProgressLevels = ProgressLevels
                    .Where(pl => pl.State == 0 && pl.Level <= 20 && pl.Level >= 1)
                    .OrderByDescending(pl => pl.Level)
                    .ToList();
                ProgressLevelMax = sortedProgressLevels[0];
                SelectedProgressLevel = 4;
                ProgressLevelMin = sortedProgressLevels[SelectedProgressLevel];
            }
            FetchEmployeeList();
            UpdateScreen();

        }
        private void FetchEmployeeList()
        {

            using (var context = new AppDbContext())
            {
                Employees = new ObservableCollection<Employee>(
                    context.Employees
                        .Where(e => e.SectionCode == this.SelectedSection.Code && e.State == 0)
                        .ToList()
                );
            }
        }
        private void UpdateScreen()
        {

        }
        private void YearSelectionChangedExecute()
        {
            UpdateScreen();
        }
        private void MonthSelectionChangedExecute()
        {
            UpdateScreen();
        }
        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
        }
        private void EmployeeSelectionChangedExecute()
        {
            UpdateScreen();
        }


    }
}
