﻿using EntityModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace QPAS
{
    public class PerformanceReportPageViewModel : ViewModelBase
    {
        private string _toggleStratsText;
        private string _toggleTagsText;

        internal IDBContext Context;
        internal MainViewModel Parent;

        public ReportSettings ReportSettings { get; set; }

        public TradeFilterSettings TradeFilterSettings { get; set; }

        public ObservableCollection<CheckListItem<Strategy>> Strategies { get; set; }

        public ObservableCollection<CheckListItem<Tag>> Tags { get; set; }

        public ObservableCollection<Benchmark> Benchmarks { get; set; }

        public string ToggleStratsText
        {
            get { return _toggleStratsText; }
            set
            {
                if (value == _toggleStratsText) return;
                _toggleStratsText = value;
                OnPropertyChanged();
            }
        }

        public string ToggleTagsText
        {
            get { return _toggleTagsText; }
            set
            {
                if (value == _toggleTagsText) return;
                _toggleTagsText = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleAllStrategies { get; set; }

        public ICommand ToggleAllTags { get; set; }

        public ICommand GenerateReport { get; set; }

        public PerformanceReportPageViewModel(IDBContext context, IDialogService dialogService, MainViewModel parent)
            : base(dialogService)
        {
            Context = context;
            Parent = parent;

            ReportSettings = new ReportSettings();
            TradeFilterSettings = new TradeFilterSettings(Context);

            ToggleTagsText = "Select All";
            ToggleStratsText = "Select All";

            Strategies = new ObservableCollection<CheckListItem<Strategy>>();
            Tags = new ObservableCollection<CheckListItem<Tag>>();
            Benchmarks = new ObservableCollection<Benchmark>();

            CreateCommands();
        }

        private void CreateCommands()
        {
            ToggleAllTags = new RelayCommand(ToggleTags);
            ToggleAllStrategies = new RelayCommand(ToggleStrats);
            GenerateReport = new RelayCommand(GenReport);
        }

        private void ToggleStrats()
        {
            if (ToggleStratsText == "Select All")
            {
                foreach (CheckListItem<Strategy> item in Strategies)
                {
                    item.IsChecked = true;
                }
                ToggleStratsText = "Deselect All";
            }
            else
            {
                foreach (CheckListItem<Strategy> item in Strategies)
                {
                    item.IsChecked = false;
                }
                ToggleStratsText = "Select All";
            }
        }

        private void ToggleTags()
        {
            if (ToggleTagsText == "Select All")
            {
                foreach (CheckListItem<Tag> item in Tags)
                {
                    item.IsChecked = true;
                }
                ToggleTagsText = "Deselect All";
            }
            else
            {
                foreach (CheckListItem<Tag> item in Tags)
                {
                    item.IsChecked = false;
                }
                ToggleTagsText = "Select All";
            }
        }

        public override void Refresh()
        {
            //tags
            var selectedTags = Tags
                            .Where(x => x.IsChecked)
                            .Select(x => x.Item)
                            .ToList();
            Tags.Clear();

            foreach (var checkItem in Context
                .Tags
                .OrderBy(x => x.Name)
                .ToList()
                .Select(x => new CheckListItem<Tag>(x, selectedTags.Contains(x))))
            {
                Tags.Add(checkItem);
            }

            //strategies
            var selectedStrats = Strategies
                .Where(x => x.IsChecked)
                .Select(x => x.Item)
                .ToList();
            Strategies.Clear();

            foreach (var checkItem in Context
                .Strategies
                .OrderBy(x => x.Name)
                .ToList()
                .Select(x => new CheckListItem<Strategy>(x, selectedStrats.Contains(x))))
            {
                Strategies.Add(checkItem);
            }

            //benchmarks
            Benchmarks.Clear();
            foreach (Benchmark b in Context.Benchmarks.OrderBy(x => x.Name))
            {
                Benchmarks.Add(b);
            }
        }

        private void GenReport()
        {
            var selectedTags =
                Tags
                .Where(x => x.IsChecked)
                .Select(x => x.Item)
                .ToList();

            var selectedStrategies =
                Strategies
                .Where(x => x.IsChecked)
                .Select(x => x.Item)
                .ToList();

            var trades = TradeFiltering.Filter(selectedTags, selectedStrategies, Context, TradeFilterSettings);

            Parent.GenerateReportFromTrades.Execute(trades);
        }
    }
}