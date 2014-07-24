using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reitit
{
    public class ReloadDisruptionsMessage { }

    public class DisruptionsLoader : ExtendedObservableObject
    {
        private static readonly TimeSpan MinimumRefreshInterval = TimeSpan.FromMinutes(1);

        private Task _load;
        private CancellationTokenSource _cancellationSource;

        public ObservableCollection<Disruption> Disruptions {get { return _disruptions;}}
        private ObservableCollection<Disruption> _disruptions = new ObservableCollection<Disruption>();

        private bool _hasLoaded = false;

        public DateTime Loaded { get { return _loaded; } }
        private DateTime _loaded;

        public bool Loading
        {
            get { return _loading; }
            set { Set(() => Loading, ref _loading, value); }
        }
        public bool _loading;

        public DisruptionsLoader()
        {
            _cancellationSource = new CancellationTokenSource();
            Messenger.Default.Register<ReloadDisruptionsMessage>(this, async m =>
            {
                await Load();
            });
        }

        private void UpdateObservable(Disruptions disruptions)
        {
            Disruptions.Clear();
            if (Disruptions != null)
            {
                Disruptions.AddRange(disruptions.SuddenDisruptions);
                Disruptions.AddRange(disruptions.AdvanceDisruptions);
            }
        }

        public async Task Load(bool force = false)
        {
            try
            {
                if (_load != null)
                {
                    if (force && !Loading)
                    {
                        _cancellationSource.Cancel();
                        _load = DoLoad(true);
                        await _load;
                        _load = null;
                    }
                    else
                    {
                        await _load;
                    }
                }
                else
                {
                    _load = DoLoad();
                    await _load;
                    _load = null;
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task DoLoad(bool force = false)
        {
            if (force || !_hasLoaded || DateTime.Now - Loaded < MinimumRefreshInterval)
            {
                Loading = true;
                try
                {
                    var disruptions = await App.Current.PoikkeusinfoClient.GetAsync(_cancellationSource.Token);
                    _loaded = DateTime.Now;
                    UpdateObservable(disruptions);
                    _hasLoaded = true;
                }
                finally
                {
                    Loading = false;
                }
            }
        }
    }
}
