using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Finally.Models;

namespace Finally.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private ObservableCollection<int> _months;
        private int _selectedYear;
        private int _selectedMonth;
        private int _selectedSectionCode;
        private int _selectedEmployeeCode;
        private CollectionView _resultCollectionView;


        public ObservableCollection<int> Months
        {
            get { return _months; }
            set { SetProperty(ref _months, value); }
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

            Months = new ObservableCollection<int>(Enumerable.Range(1, 12));

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
