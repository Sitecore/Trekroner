﻿using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Microsoft.ReverseProxy.Abstractions;
using Microsoft.ReverseProxy.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    using Sitecore.Trekroner.Proxy;

    public static class InMemoryConfigProviderExtensions
    {
        public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(new MicrosoftProxyConfigProvider(routes, clusters));
            return builder;
        }
    }
}

namespace Sitecore.Trekroner.Proxy
{
    public class MicrosoftProxyConfigProvider : IProxyConfigProvider
    {
        private volatile InMemoryConfig _config;

        public MicrosoftProxyConfigProvider(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            _config = new InMemoryConfig(routes, clusters);
        }

        public IProxyConfig GetConfig() => _config;

        public void Update(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            var oldConfig = _config;
            _config = new InMemoryConfig(routes, clusters);
            oldConfig.SignalChange();
        }

        private class InMemoryConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            public InMemoryConfig(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
            {
                Routes = routes;
                Clusters = clusters;
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            public IReadOnlyList<ProxyRoute> Routes { get; }

            public IReadOnlyList<Cluster> Clusters { get; }

            public IChangeToken ChangeToken { get; }

            internal void SignalChange()
            {
                _cts.Cancel();
            }
        }
    }
}