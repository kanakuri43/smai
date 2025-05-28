using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Finally.Models;
using Microsoft.EntityFrameworkCore;

namespace Finally.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private ObservableCollection<int> _years;
        private int _selectedYear;
        private ObservableCollection<int> _months;
        private int _selectedMonth;

        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<ProgressLevel> _progressLevels;
        private ObservableCollection<Case> _cases;
        private ObservableCollection<LatestTotal> _latesstTotals;

        private Section _selectedSection;
        private Employee _selectedEmployee;
        private int _selectedProgressLevel;
        private ProgressLevel _progressLevelMin;
        private ProgressLevel _progressLevelMax;


        public ObservableCollection<int> Months
        {
            get { return _months; }
            set { SetProperty(ref _months, value); }
        }
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
        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set { SetProperty(ref _selectedMonth, value); }
        }

        public Section SelectedSection
        {
            get { return _selectedSection; }
            set { SetProperty(ref _selectedSection, value); }
        }

        public Employee SelectedEmployee
        {
            get { return _selectedEmployee; }
            set { SetProperty(ref _selectedEmployee, value); }
        }
        public ObservableCollection<Section> Sections
        {
            get { return _sections; }
            set { SetProperty(ref _sections, value); }
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
        public ObservableCollection<LatestTotal> LatestTotals
        {
            get { return _latesstTotals; }
            set { SetProperty(ref _latesstTotals, value); }
        }

        public DelegateCommand YearSelectionChanged { get; }
        public DelegateCommand MonthSelectionChanged { get; }
        public DelegateCommand SectionSelectionChanged { get; }
        public DelegateCommand EmployeeSelectionChanged { get; }
        public DelegateCommand SelectedProgressLevelChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            YearSelectionChanged = new DelegateCommand(YearSelectionChangedExecute);
            MonthSelectionChanged = new DelegateCommand(MonthSelectionChangedExecute);
            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);
            SelectedProgressLevelChanged = new DelegateCommand(SelectedProgressLevelChangedExecute);

            // 年リスト
            int currentYear = DateTime.Now.Year;
            Years = new ObservableCollection<int>(Enumerable.Range(currentYear - 1, 3));
            this.SelectedYear = currentYear;

            // 月リスト
            Months = new ObservableCollection<int>(Enumerable.Range(1, 12));
            this.SelectedMonth = DateTime.Now.Month;

            using (var context = new AppDbContext())
            {
                // 部署リスト
                Sections = new ObservableCollection<Section>(
                            context.Sections.Where(s => s.State == 0).ToList()
                        );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 11010);

                // 物権確度
                this.ProgressLevels = new ObservableCollection<ProgressLevel>(
                                context.ProgressLevels.Where(s => s.State == 0).ToList()
                            );

                var sortedProgressLevels = ProgressLevels
                    .Where(pl => pl.State == 0 && pl.Level <= 20 && pl.Level >= 1)
                    .OrderByDescending(pl => pl.Level)
                    .ToList();
                ProgressLevelMax = sortedProgressLevels[0];
                ProgressLevelMin = sortedProgressLevels[0];

            }

            FetchEmployeeList();

            ScreenUpdate();

        }
        private void ScreenUpdate()
        {
            // 社員未選択なら即return
            if (this.SelectedEmployee == null)
            {
                return;
            }

            using (var context = new AppDbContext())
            {

                var sql = @"
                            SELECT
                                CAL.月度 AS YearMonth
                                , TAR.売上目標 AS TargetSales
                                , TAR.粗利目標 AS TargetProfit
                                , S.社員コード AS EmployeeCode
                                , S.売上金額 AS FinishedSales
                                , S.粗利金額 AS FinishedProfit

                            FROM
                                (select 月度 FROM Mカレンダ WHERE 期 = 86 GROUP BY 月度) CAL 
                                LEFT JOIN ( 
                                    SELECT
                                        月度
                                        , SUM(売上実績) as 売上目標
                                        , SUM(粗利実績) as 粗利目標 
                                    FROM
                                        S進捗目標 
                                    WHERE
                                        進捗区分 = 1 
                                        AND 社員コード <> 0 
                                        and (社員コード = {0} OR (0 = {0})) 
                                        and 部門コード between + {1} and {1} 
                                    GROUP BY
                                        月度
                                ) AS TAR
                                    ON CAL.月度 = TAR.月度 
                                LEFT JOIN ( 
                                    SELECT
                                        D物件.受注月度
                                        , D物件担当.社員コード
                                        , ISNULL(SUM(D物件.売上金額), 0) AS 売上金額
                                        , ISNULL(SUM(D物件.粗利金額), 0) AS 粗利金額 
                                    FROM
                                        D物件 
                                        INNER JOIN D物件担当 
                                            ON D物件担当.物件連番 = D物件.連番 
                                            AND D物件担当.担当区分 = 1 
                                        LEFT JOIN M物件確度 
                                            ON M物件確度.コード = D物件.物件確度 
                                    WHERE
                                        D物件担当.社員コード = {0} 
                                        AND D物件.削除区分 = 0 
                                        AND M物件確度.物件確度区分 BETWEEN 30 AND 100 
                                    GROUP BY
                                        D物件担当.社員コード
                                        , D物件.受注月度
                                ) S 
                                    ON CAL.月度 = S.受注月度 
                        ";
                var lt = context.Database.SqlQueryRaw<LatestTotal>(
                                    sql,
                                    this.SelectedEmployee.Code,
                                    this.SelectedYear * 100 + this.SelectedMonth,
                                    this.ProgressLevelMin.Level,
                                    this.ProgressLevelMax.Level
                                ).ToList();
                if (lt == null)
                {
                    return;
                }
                else
                {
                    this.LatestTotals = new ObservableCollection<LatestTotal>(lt);
                }
            }

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

        private void YearSelectionChangedExecute()
        {
            ScreenUpdate();
        }
        private void MonthSelectionChangedExecute()
        {
            ScreenUpdate();
        }
        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
        }
        private void EmployeeSelectionChangedExecute()
        {
            ScreenUpdate();
        }
        private void SelectedProgressLevelChangedExecute()
        {


            ScreenUpdate();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
