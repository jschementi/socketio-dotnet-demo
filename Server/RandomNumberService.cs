using System;
using System.Collections.Generic;
using System.Threading;

namespace Server {
    public class RandomNumberService : ISubscriptionHandler {

        private static readonly object SubscriptionsLock = new object();

        private readonly string[] _events = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" };
        private readonly IDictionary<string, int> _subscriptions = new Dictionary<string, int>();
        
        private ITransportPublisher _publisher;
        private bool _shouldStop;

        public void Start(ITransportPublisher publisher) {
            _shouldStop = false;
            _publisher = publisher;
            StartEventsTicking();
        }

        public void Stop() {
            _shouldStop = true;
        }

        public void OnSubscribe(string eventName) {
            lock (SubscriptionsLock) {
                if (!_subscriptions.ContainsKey(eventName)) _subscriptions[eventName] = 0;
                _subscriptions[eventName]++;
            }
        }

        public void OnUnsubscribe(string eventName) {
            lock (SubscriptionsLock) {
                if (!_subscriptions.ContainsKey(eventName) || _subscriptions[eventName] == 0)
                    return;
                _subscriptions[eventName]--;
            }
        }

        private bool IsActive(string eventName) {
            return _subscriptions.ContainsKey(eventName) && _subscriptions[eventName] > 0;
        }

        private void StartEventsTicking() {
            foreach (var ev in _events) {
                var timerRandom = new Random();
                var doubleRandom = new Random();
                var liftedEv = ev;
                ThreadPool.QueueUserWorkItem(_ => {
                    while (!_shouldStop) {
                        if (IsActive(liftedEv)) {
                            var value = Convert.ToInt32(doubleRandom.NextDouble()*100).ToString();
                            var payload = new ChangeValuePayload {Name = liftedEv, Value = value};
                            _publisher.Publish(payload);
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 0, 0, Convert.ToInt32(timerRandom.NextDouble() * 100)));
                    }
                });
            }
        }
    }
}
