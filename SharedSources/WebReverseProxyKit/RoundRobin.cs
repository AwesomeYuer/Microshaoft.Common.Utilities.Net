﻿#if NETCOREAPP
namespace Microshaoft.Web.ReverseProxyKit
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    /// <summary>
    ///     Represents a round robing collection of hosts.
    /// </summary>
    public class RoundRobin: IEnumerable<UpstreamHost>
    {
        private readonly HashSet<UpstreamHost> _hosts = new HashSet<UpstreamHost>();
        private UpstreamHost[] _distribution;
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private long _position = -1;

        /// <summary>
        ///     Initializes a new instance of <see cref="RoundRobin"/>
        /// </summary>
        /// <param name="hosts"></param>
        public RoundRobin(params UpstreamHost[] hosts)
        {
            foreach (var host in hosts)
            {
                _hosts.Add(host);
            }
            BuildDistribution();
        }

        public void Add(UpstreamHost upstreamHost)
        {
            _hosts.Add(upstreamHost);
            BuildDistribution();
        }

        /// <summary>
        ///     Gets the next upstream host
        /// </summary>
        /// <returns>An upstream host instance.</returns>
        public UpstreamHost Next()
        {
            _lockSlim.EnterReadLock();

            if (_distribution.Length == 1)
            {
                var singleHost = _distribution[0];
                _lockSlim.ExitReadLock();
                return singleHost;
            }
            var position = Interlocked.Increment(ref _position);
            var mod =  position % _distribution.Length;
            var upstreamHost = _distribution[mod];

            _lockSlim.ExitReadLock();

            return upstreamHost;
        }

        private void BuildDistribution()
        {
            _lockSlim.EnterWriteLock();
            var upstreamHosts = new List<UpstreamHost>();
            foreach (var upstreamHost in _hosts)
            {
                for (var i = 0; i < upstreamHost.Weight; i++)
                {
                    upstreamHosts.Add(upstreamHost);
                }
            }
            _distribution = upstreamHosts.ToArray();
            _lockSlim.ExitWriteLock();
        }

        public IEnumerator<UpstreamHost> GetEnumerator()
        {
            return _hosts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => _hosts.GetEnumerator();
    }
}
#endif