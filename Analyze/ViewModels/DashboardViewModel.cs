using Analyze.Models;
using Microsoft.EntityFrameworkCore;
using OxyPlot;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        private PlotModel _pieChartModel;

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

        public PlotModel PieChartModel
        {
            get { return _pieChartModel; }
            set { SetProperty(ref _pieChartModel, value); }
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
            CreatePieChart();
            UpdateScreen();
        }

        private void CreatePieChart()
        {
            var model = new PlotModel
            {
                Title = "売上構成比",
                Background = OxyColors.White
            };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 2.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            // ダミーデータ
            pieSeries.Slices.Add(new PieSlice("営業部", 45) { Fill = OxyColor.FromRgb(70, 130, 180) });
            pieSeries.Slices.Add(new PieSlice("開発部", 30) { Fill = OxyColor.FromRgb(255, 140, 0) });
            pieSeries.Slices.Add(new PieSlice("マーケティング部", 15) { Fill = OxyColor.FromRgb(50, 205, 50) });
            pieSeries.Slices.Add(new PieSlice("管理部", 7) { Fill = OxyColor.FromRgb(220, 20, 60) });
            pieSeries.Slices.Add(new PieSlice("その他", 3) { Fill = OxyColor.FromRgb(186, 85, 211) });

            model.Series.Add(pieSeries);
            PieChartModel = model;
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
            // データ更新時に円グラフも更新
            UpdatePieChart();
        }

        private void UpdatePieChart()
        {
            // 選択された年や部署に応じてデータを更新
            var pieSeries = PieChartModel.Series[0] as PieSeries;
            if (pieSeries != null)
            {
                pieSeries.Slices.Clear();

                using (var context = new AppDbContext())
                {
                    // SQLクエリで部署別の案件数を取得
                    string sql = @"
                        SELECT
                            MIN(M部門.名称) AS SectionName
                            , ISNULL(SUM(D物件.売上金額), 0) AS Sales 
                        FROM
                            D物件 
                            INNER JOIN D物件担当 
                                ON D物件担当.物件連番 = D物件.連番 
                                AND D物件担当.担当区分 = 1 
                            LEFT JOIN M社員 
                                ON D物件担当.社員コード = M社員.コード 
                            LEFT JOIN M部門 
                                ON M社員.部門コード = M部門.コード 
                            LEFT JOIN M物件確度 
                                ON M物件確度.コード = D物件.物件確度 
                        WHERE
                            SUBSTRING(CONVERT(VARCHAR, D物件.受注月度), 1, 4) = 2024 
                            AND D物件.削除区分 = 0 
                            AND M物件確度.物件確度区分 BETWEEN 30 AND 100 
                        GROUP BY
                            M社員.部門コード
                        ";

                    var pieData = context.Database.SqlQueryRaw<PieChartData>(
                        sql,
                        253,
                        this.SelectedYear * 100 + 1, // 仮で1月を設定
                        this.ProgressLevelMin?.Level ?? 1,
                        this.ProgressLevelMax?.Level ?? 20
                    ).ToList();

                    if (pieData != null && pieData.Any())
                    {
                        // SQLの結果から円グラフを作成
                        var colors = new OxyColor[]
                        {
                            OxyColor.FromRgb(70, 130, 180),   // SteelBlue
                            OxyColor.FromRgb(255, 140, 0),    // DarkOrange
                            OxyColor.FromRgb(50, 205, 50),    // LimeGreen
                            OxyColor.FromRgb(220, 20, 60),    // Crimson
                            OxyColor.FromRgb(186, 85, 211),   // MediumOrchid
                            OxyColor.FromRgb(255, 215, 0),    // Gold
                            OxyColor.FromRgb(106, 90, 205),   // SlateBlue
                            OxyColor.FromRgb(255, 69, 0)      // OrangeRed
                        };

                        for (int i = 0; i < pieData.Count && i < colors.Length; i++)
                        {
                            pieSeries.Slices.Add(new PieSlice(
                                pieData[i].SectionName,
                                (double)pieData[i].Sales
                            )
                            {
                                Fill = colors[i]
                            });
                        }
                    }
                    else
                    {
                        // データがない場合はダミーデータを表示
                        pieSeries.Slices.Add(new PieSlice("データなし", 1) { Fill = OxyColor.FromRgb(200, 200, 200) });
                    }
                }

                PieChartModel.InvalidatePlot(true);
            }
        }

        // SQLクエリ結果を受け取るためのクラス
        public class PieChartData
        {
            public string SectionName { get; set; }
            public decimal Sales { get; set; }
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
            UpdateScreen();
        }

        private void EmployeeSelectionChangedExecute()
        {
            UpdateScreen();
        }
    }
}