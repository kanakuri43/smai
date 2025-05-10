using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using sale.Models;
using Split.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Split.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private int _selectedYear;
        private int _selectedMonth;
        private int _selectedSectionCode;
        private int _selectedEmployeeCode;
        private CollectionView _resultCollectionView;


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

        public int SelectedSectionCode
        {
            get { return _selectedSectionCode; }
            set { SetProperty(ref _selectedSectionCode, value); }
        }

        public int SelectedEmployeeCode
        {
            get { return _selectedEmployeeCode; }
            set { SetProperty(ref _selectedEmployeeCode, value); }
        }
        public CollectionView ResultCollectionView
        {
            get { return _resultCollectionView; }
            set { SetProperty(ref _resultCollectionView, value); }
        }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            using (var context = new AppDbContext())
            {
                
                int employeeCode = 239;

                // 売上・粗利
                var weeklyProgress = context.Set<WeeklyProgress>()
                    .Where(wp => wp.EmployeeCode == employeeCode && wp.YearMonth == 202504)
                    .Select(wp => new WeeklyProgress
                    {
                        Date = (wp.Date % 100),
                        SalesResult = wp.SalesResult,
                        SalesEstimate1 = wp.SalesEstimate1,
                        SalesEstimate2 = wp.SalesEstimate2,
                        ProfitResult = wp.ProfitResult,
                        ProfitEstimate1 = wp.ProfitEstimate1,
                        ProfitEstimate2 = wp.ProfitEstimate2
                    })
                    .ToList();
                ResultCollectionView = new ListCollectionView(new ObservableCollection<WeeklyProgress>(weeklyProgress));

            }
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
