using ControlzEx.Standard;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Split.Models;
using Split.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Split.ViewModels
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
        private ObservableCollection<LatesstAmount> _latesstAmounts;

        private Section _selectedSection;
        private Employee _selectedEmployee;

        private decimal _currentSalesTarget;
        private float _salesProgressRate;
        private float _salesPreviousRate;

        private decimal _currentProfitTarget;
        private float _profitProgressRate;
        private float _profitPreviousRate;

        private CollectionView _resultCollectionView;


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

        public decimal CurrentSalesTarget
        {
            get { return _currentSalesTarget; }
            set { SetProperty(ref _currentSalesTarget, value); }
        }
        public float SalesProgressRate
        {
            get { return _salesProgressRate; }
            set { SetProperty(ref _salesProgressRate, value); }
        }
        public float SalesPreviousRate
        {
            get { return _salesPreviousRate; }
            set { SetProperty(ref _salesPreviousRate, value); }
        }
        public decimal CurrentProfitTarget
        {
            get { return _currentProfitTarget; }
            set { SetProperty(ref _currentProfitTarget, value); }
        }
        public float ProfitProgressRate
        {
            get { return _profitProgressRate; }
            set { SetProperty(ref _profitProgressRate, value); }
        }
        public float ProfitPreviousRate
        {
            get { return _profitPreviousRate; }
            set { SetProperty(ref _profitPreviousRate, value); }
        }
        public ObservableCollection<LatesstAmount> LatestAmounts
        {
            get { return _latesstAmounts; }
            set { SetProperty(ref _latesstAmounts, value); }
        }


        public CollectionView ResultCollectionView
        {
            get { return _resultCollectionView; }
            set { SetProperty(ref _resultCollectionView, value); }
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

            // 月リスト
            Months = new ObservableCollection<int>(Enumerable.Range(1, 12));
            this.SelectedMonth = DateTime.Now.Month;
            this.SelectedMonth = 4;

            using (var context = new AppDbContext())
            {
                // 部署リスト
                Sections = new ObservableCollection<Section>(
                            context.Sections.Where(s => s.State == 0).ToList()
                        );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 11010);            

            }

            // 社員リスト 部署変更時に再度呼び出すので関数化
            FetchEmployeeList();
            this.SelectedEmployee = Employees.FirstOrDefault(e => e.Code == 253);


            FetchTargetAndResults();
        }

        private void FetchTargetAndResults()
        {
            // 社員未選択なら即return
            if (this.SelectedEmployee == null)
            {
                ResultCollectionView = new ListCollectionView(new ObservableCollection<WeeklyProgress>());
                return;
            }

            using (var context = new AppDbContext())
            {
                // 選択月の目標金額
                var target = context.Set<Target>()
                                    .FirstOrDefault(t => t.EmployeeCode == SelectedEmployee.Code && t.YearMonth == (SelectedYear * 100 + SelectedMonth));
                if (target != null)
                {
                    this.CurrentSalesTarget = target.SalesTarget;
                    this.CurrentProfitTarget = target.ProfitTarget;
                }


                // 選択月の 売上・粗利
                var weeklyProgress = context.Set<WeeklyProgress>()
                    .Where(wp => wp.EmployeeCode == this.SelectedEmployee.Code && wp.YearMonth == (this.SelectedYear * 100 + this.SelectedMonth))
                    .Select(wp => new WeeklyProgress
                    {
                        Date = (wp.Date % 100),
                        SalesOfRecorded = wp.SalesOfRecorded,   
                        SalesOfClosing = wp.SalesOfClosing,
                        SalesOfWish = wp.SalesOfWish,
                        ProfitOfRecorded = wp.ProfitOfRecorded,
                        ProfitOfClosing = wp.ProfitOfClosing,
                        ProfitOfWish = wp.ProfitOfWish
                    })
                    .ToList();
                ResultCollectionView = new ListCollectionView(new ObservableCollection<WeeklyProgress>(weeklyProgress));


                // 選択月の 達成率
                //var finalSalesRecord = weeklyProgress.OrderByDescending(wp => wp.Date).FirstOrDefault();
                //if (finalSalesRecord != null)
                //{
                //    if (CurrentSalesTarget > 0)
                //    {
                //        SalesProgressRate = ((float)(finalSalesRecord.SalesOfRecorded / CurrentSalesTarget) * 100);
                //    }
                //    else
                //    {
                //        SalesProgressRate = 0;
                //    }
                //}


                // 前年同月比
                //var previousYearProgress = context.Set<WeeklyProgress>()
                //                            .Where(wp => wp.EmployeeCode == this.SelectedEmployee.Code 
                //                                && wp.YearMonth == ((this.SelectedYear - 1) * 100 + this.SelectedMonth))
                //                            .OrderByDescending(wp => wp.Date) 
                //                            .FirstOrDefault();
                //if (previousYearProgress != null && finalSalesRecord != null)
                //{
                //    if (previousYearProgress.SalesOfRecorded > 0)
                //    {
                //        SalesPreviousRate = ((float)(finalSalesRecord.SalesOfRecorded / previousYearProgress.SalesOfRecorded) * 100);
                //    }
                //    else
                //    {
                //        SalesPreviousRate = 0;
                //    }
                //}
                //else
                //{
                //    SalesPreviousRate = 0;
                //}

                // 社員ごとの最終金額
                var sql = @"
                        SELECT
                            F.社員コード AS EmployeeCode
                            , F.FinishedSales
                            , F.FinishedProfit
                            , U.UnfinishedSales
                            , U.UnfinishedProfit 
                        FROM
                            ( 
                                SELECT
                                    D物件担当.社員コード
                                    , ISNULL(SUM(D物件.売上金額), 0) AS FinishedSales
                                    , ISNULL(SUM(D物件.粗利金額), 0) AS FinishedProfit 
                                FROM
                                    D物件 
                                    INNER JOIN D物件担当 
                                        ON D物件担当.物件連番 = D物件.連番 
                                        AND D物件担当.担当区分 = 1 
                                    LEFT JOIN M物件確度 
                                        ON M物件確度.コード = D物件.物件確度 
                                WHERE
                                    D物件担当.社員コード = {0} 
                                    AND D物件.売上月度 = {1} 
                                    AND D物件.削除区分 = 0 
                                    AND M物件確度.物件確度区分 BETWEEN 30 AND 100 
                                GROUP BY
                                    D物件担当.社員コード
                            ) F 
                            INNER JOIN ( 
                                SELECT
                                    D物件担当.社員コード
                                    , ISNULL(SUM(D物件.売上金額), 0) AS UnfinishedSales
                                    , ISNULL(SUM(D物件.粗利金額), 0) AS UnfinishedProfit 
                                FROM
                                    D物件 
                                    INNER JOIN D物件担当 
                                        ON D物件担当.物件連番 = D物件.連番 
                                        AND D物件担当.担当区分 = 1 
                                    LEFT JOIN M物件確度 
                                        ON M物件確度.コード = D物件.物件確度 
                                WHERE
                                    D物件担当.社員コード = {0}
                                    AND D物件.売上月度 = {1} 
                                    AND D物件.削除区分 = 0 
                                    AND M物件確度.物件確度区分 BETWEEN 0 AND 20 
                                GROUP BY
                                    D物件担当.社員コード
                            ) U 
                                ON F.社員コード = U.社員コード";
                var results = context.Database.SqlQueryRaw<LatesstAmount>(
                                    sql,
                                    this.SelectedEmployee.Code,
                                    this.SelectedYear * 100 + this.SelectedMonth
                                ).FirstOrDefault();
                if (results != null)
                {
                    // 結果をObservableCollectionに変換して保存
                    this.LatestAmounts = new ObservableCollection<LatesstAmount> { results };
                }

                // 達成率
                if (CurrentSalesTarget > 0)
                {
                    SalesProgressRate = ((float)(LatestAmounts[0].FinishedSales / CurrentSalesTarget) * 100);
                }
                else
                {
                    SalesProgressRate = 0;
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
            FetchTargetAndResults();
        }
        private void MonthSelectionChangedExecute()
        {
            FetchTargetAndResults();
        }
        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
        }
        private void EmployeeSelectionChangedExecute()
        {
            FetchTargetAndResults();
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
