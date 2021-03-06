﻿using AgpWps.Model.Repositories;
using AgpWps.Model.Services;
using AgpWps.Model.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Wps.Client.Models;
using Wps.Client.Models.Data;
using Wps.Client.Services;

namespace AgpWps.Model.Factories
{
    public class ViewModelFactory : IViewModelFactory
    {

        private readonly IMapService _mapService;
        private readonly IDialogService _dialogService;
        private readonly IWpsClient _wpsClient;
        private readonly IContext _context;
        private readonly IRequestFactory _requestFactory;
        private readonly IServerRepository _serverRepository;
        private readonly ILoggerRepository _loggerRepository;

        public ViewModelFactory(IMapService mapService,
            IDialogService dialogService,
            IWpsClient wpsClient,
            IContext context,
            IRequestFactory requestFactory,
            IServerRepository serverRepository,
            ILoggerRepository loggerRepository)
        {
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _wpsClient = wpsClient ?? throw new ArgumentNullException(nameof(wpsClient));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
            _serverRepository = serverRepository ?? throw new ArgumentNullException(nameof(serverRepository));
            _loggerRepository = loggerRepository ?? throw new ArgumentNullException(nameof(loggerRepository));
        }

        public ProcessOfferingViewModel CreateProcessOfferingViewModel(string wpsUri, ProcessSummary sum)
        {
            var transmissionMode = sum.OutputTransmission.ToString().ToLower();
            if (transmissionMode.Equals("ValueReference", StringComparison.InvariantCultureIgnoreCase))
            {
                transmissionMode = "value reference";
            }

            return new ProcessOfferingViewModel(wpsUri, sum.Identifier, _dialogService, _wpsClient, _context, this)
            {
                ProcessName = sum.Identifier,
                TransmissionModes = transmissionMode,
                JobControlOptions = sum.JobControlOptions,
                Keywords = sum.Keywords != null ? string.Join(", ", sum.Keywords) : null,
                Model = sum.ProcessModel,
                Version = sum.ProcessVersion,
                Abstract = sum.Abstract,
            };
        }

        public DataInputViewModel CreateDataInputViewModel(Input input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var formatVms = input.Data.Formats.GroupBy(f => f.MimeType).Select(f =>
            {
                var schemas = f.Where(fs => !string.IsNullOrEmpty(fs.Schema)).Select(fs => fs.Schema).ToList();
                var encodings = f.Where(fe => !string.IsNullOrEmpty(fe.Encoding)).Select(fe => fe.Encoding).ToList();

                var formatVm = new FormatViewModel
                {
                    MimeType = f.Key,
                    Schemas = new ObservableCollection<string>(schemas),
                    SelectedSchema = schemas.FirstOrDefault(),
                    Encodings = new ObservableCollection<string>(encodings),
                    SelectedEncoding = encodings.FirstOrDefault()
                };

                return formatVm;
            }).ToList();

            var isOptional = input.MinimumOccurrences == 0;

            DataInputViewModel vm;

            if (input.Data is LiteralData ld)
            {
                vm = new LiteralInputViewModel();
            }
            else if (input.Data is BoundingBoxData bbd)
            {
                vm = new BoundingBoxInputViewModel(_mapService, _context, _dialogService);
            }
            else if (input.Data is ComplexData cd)
            {
                vm = new ComplexDataViewModel(_dialogService);
            }
            else
            {
                vm = new DataInputViewModel();
            }

            vm.IsOptional = isOptional;
            vm.ProcessName = input.Identifier;
            vm.Formats = new ObservableCollection<FormatViewModel>(formatVms);

            var defaultFormat = input.Data.Formats.FirstOrDefault(f => f.IsDefault);
            if (defaultFormat != null)
            {
                var selectedVm = formatVms.FirstOrDefault(v =>
                    v.MimeType.Equals(defaultFormat.MimeType, StringComparison.InvariantCultureIgnoreCase));
                if (selectedVm != null)
                {
                    selectedVm.SelectedSchema = defaultFormat.Schema;
                    selectedVm.SelectedEncoding = defaultFormat.Encoding;
                    vm.SelectedFormat = selectedVm;
                }
            }
            else
            {
                vm.SelectedFormat = vm.Formats.FirstOrDefault();
            }

            return vm;
        }

        public DataOutputViewModel CreateDataOutputViewModel(Output output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            var formatVms = output.Data.Formats.GroupBy(f => f.MimeType).Select(f =>
            {
                var schemas = f.Where(fs => !string.IsNullOrEmpty(fs.Schema)).Select(fs => fs.Schema).ToList();
                var encodings = f.Where(fe => !string.IsNullOrEmpty(fe.Encoding)).Select(fe => fe.Encoding).ToList();

                var formatVm = new FormatViewModel
                {
                    MimeType = f.Key,
                    Schemas = new ObservableCollection<string>(schemas),
                    SelectedSchema = schemas.FirstOrDefault(),
                    Encodings = new ObservableCollection<string>(encodings),
                    SelectedEncoding = encodings.FirstOrDefault()
                };

                return formatVm;
            }).ToList();

            DataOutputViewModel outputVm;
            if (output.Data is LiteralData)
            {
                outputVm = new LiteralDataOutputViewModel();
            }
            else
            {
                outputVm = new FileOutputViewModel(_dialogService);
            }

            outputVm.Identifier = output.Identifier;
            outputVm.Formats = new ObservableCollection<FormatViewModel>(formatVms);

            var defaultFormat = output.Data.Formats.FirstOrDefault(f => f.IsDefault);
            if (defaultFormat != null)
            {
                var selectedVm = formatVms.FirstOrDefault(v =>
                    v.MimeType.Equals(defaultFormat.MimeType, StringComparison.InvariantCultureIgnoreCase));
                if (selectedVm != null)
                {
                    selectedVm.SelectedSchema = defaultFormat.Schema;
                    selectedVm.SelectedEncoding = defaultFormat.Encoding;
                    outputVm.SelectedFormat = selectedVm;
                }
            }
            else
            {
                outputVm.SelectedFormat = outputVm.Formats.FirstOrDefault();
            }

            return outputVm;
        }

        public ExecutionBuilderViewModel CreateExecutionBuilderViewModel(string wpsUri, string processId)
        {
            return new ExecutionBuilderViewModel(wpsUri,
                processId,
                _wpsClient,
                _context,
                this,
                _requestFactory,
                _dialogService,
                _loggerRepository);
        }

        public ServerViewModel CreateServerViewModel(string serverUrl)
        {
            return new ServerViewModel(serverUrl, _serverRepository);
        }
    }
}
