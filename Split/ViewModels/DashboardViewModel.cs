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
                // 例: 特定の年と月のデータを取得する
                int employeeCode = 8911;

                var weeklyProgressData = context.Set<WeeklyProgress>()
                    .Where(wp => wp.EmployeeCode == employeeCode && wp.YearMonth == 202504)
                    .Select(wp => new WeeklyProgress
                    {
                        Date = wp.Date,
                        DecisionSale = wp.DecisionSale,
                        EstimateSale1 = wp.EstimateSale1,
                        EstimateSale2 = wp.EstimateSale2
                    })
                    .ToList();

                ResultCollectionView = new ListCollectionView(new ObservableCollection<WeeklyProgress>(weeklyProgressData));
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
