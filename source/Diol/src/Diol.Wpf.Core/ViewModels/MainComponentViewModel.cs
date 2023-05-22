﻿using Diol.Core.DotnetProcesses;
using Diol.Wpf.Core.Features.Https;
using Diol.Wpf.Core.Features.Shared;
using Diol.Wpf.Core.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Diol.Wpf.Core.ViewModels
{
    public class MainComponentViewModel : BindableBase
    {
        private IProcessProvider dotnetService;
        private LoggerBuilder builder;
        private IEventAggregator eventAggregator;
        private IApplicationStateService applicationStateService;

        public MainComponentViewModel(
            IProcessProvider dotnetService,
            LoggerBuilder builder,
            HttpService httpService,
            IEventAggregator eventAggregator,
            IApplicationStateService applicationStateService)
        {
            this.dotnetService = dotnetService;
            this.builder = builder;

            this.eventAggregator = eventAggregator;
            this.applicationStateService = applicationStateService;

            this.eventAggregator
                .GetEvent<DebugModeRunnedEvent>()
                .Subscribe(DebugModeRunnedEventHandler, ThreadOption.UIThread);

            this.applicationStateService.Subscribe();
        }

        public ObservableCollection<HttpViewModel> HttpLogs { get; private set; } =
            new ObservableCollection<HttpViewModel>();

        private bool _canExecute = true;
        public bool CanExecute
        {
            get => this._canExecute;
            set => SetProperty(ref this._canExecute, value);
        }

        private DelegateCommand _startCommand = null;
        public DelegateCommand StartCommand =>
            _startCommand ?? (_startCommand = new DelegateCommand(StartExecute));

        private void StartExecute()
        {
            var processId = this.dotnetService.GetProcessId();

            if (!processId.HasValue)
            {
                Console.WriteLine($"Process id ({processId}) not found. Please try again");
                return;
            }

            var eventPipeEventSourceWrapper = this.builder
                .Build()
                .SetProcessId(processId.Value)
                .Build();

            Task.Run(() =>
            {
                this.CanExecute = false;
                eventPipeEventSourceWrapper.Start();
                this.CanExecute = true;
            }).ConfigureAwait(false);


        }

        private DelegateCommand _clearCommand = null;
        public DelegateCommand ClearCommand =>
            _clearCommand ?? (_clearCommand = new DelegateCommand(ClearExecute));

        private void ClearExecute()
        {
            this.eventAggregator
                .GetEvent<ClearDataEvent>()
                .Publish(string.Empty);

            this.eventAggregator
                .GetEvent<HttpItemSelectedEvent>()
                .Publish(string.Empty);
        }

        private DelegateCommand _settingsCommand = null;
        public DelegateCommand SettingsCommand =>
            _settingsCommand ?? (_settingsCommand = new DelegateCommand(SettingsExecute));

        private void SettingsExecute()
        {
            var version = string.Empty;
            var mode = string.Empty;

#if DEBUG
            mode = "DEBUG";
#else
            mode = "RELEASE";
#endif

#if NETSTANDARD
            version = "NETSTANDARD";
#elif NETFRAMEWORK
            version = "NETFRAMEWORK";
#elif NET
            version = "NET";
#elif NETCOREAPP
            version = "NETCOREAPP";
#endif
            MessageBox.Show(version, mode);
        }

        private void DebugModeRunnedEventHandler(bool obj)
        {
            StartExecute();
        }
    }
}
