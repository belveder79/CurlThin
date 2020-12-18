using System;
using System.Collections.Concurrent;
#if !UNITY
using NetUV.Core.Handles;
#endif
namespace CurlThin.HyperPipe
{
    internal class SocketPollMap : IDisposable
    {
#if !UNITY
        private readonly ConcurrentDictionary<IntPtr, Poll> _sockets
            = new ConcurrentDictionary<IntPtr, Poll>(new IntPtrEqualityComparer());
#endif
        public void Dispose()
        {
#if !UNITY
            foreach (var poll in _sockets.Values)
            {
                poll.Stop();
                poll.Dispose();
            }
#endif
        }
#if !UNITY
        public Poll GetOrCreatePoll(IntPtr sockfd, Loop loop)
        {
            if (!_sockets.TryGetValue(sockfd, out Poll poll))
            {
                poll = loop.CreatePoll(sockfd);
                _sockets.TryAdd(sockfd, poll);
            }

            return poll;
        }

        public void RemovePoll(IntPtr sockfd)
        {
            if (_sockets.TryRemove(sockfd, out Poll poll))
            {
                poll.Stop();
                poll.Dispose();
            }
        }
#endif
    }
}