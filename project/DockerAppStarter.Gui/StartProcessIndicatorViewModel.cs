// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.Windows.Threading;

namespace DockerAppStarter.Gui
{
    internal class StartProcessIndicatorViewModel
        : Observable
    {
        private readonly Dispatcher _dispatcher;
        private string _label;
        private double? _minimum;
        private double? _maximum;
        private double? _current;
        private bool? _status;
        private bool _isIndeterminate;
        private bool _isDependency;

        public StartProcessIndicatorViewModel(bool isDependency, string label, Func<StartProcessIndicatorCallbackContext, Task> callback, CancellationToken cancellationToken = default)
        {
            _isDependency = isDependency;
            _label = label;
            _dispatcher = Dispatcher.CurrentDispatcher;

            Callback = callback;

            Status = null;
            IsIndeterminate = true;
            Current = null;
            Minimum = 0.0;
            Maximum = 1.0;

            StartProcessIndicatorCallbackContext ctx = new(this, cancellationToken);

            _ = Task.Factory.StartNew(() => Callback(ctx), cancellationToken);
        }

        public StartProcessIndicatorViewModel(string label, Func<StartProcessIndicatorCallbackContext, Task> callback, CancellationToken cancellationToken = default)
            : this(false, label, callback, cancellationToken)
        {
            // Ignore
        }

        public bool IsDependency
        {
            get { return _isDependency; }
            set { _ = Set(ref _isDependency, value); }
        }

        public string Label
        {
            get { return _label; }
            set { _ = Set(ref _label, value); }
        }

        public bool? Status
        {
            get { return _status; }
            set { _ = Set(ref _status, value); }
        }

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set { _ = Set(ref _isIndeterminate, value); }
        }

        public double? Current
        {
            get { return _current; }
            set { _ = Set(ref _current, value); }
        }

        public double? Maximum
        {
            get { return _maximum; }
            set { _ = Set(ref _maximum, value); }
        }

        public double? Minimum
        {
            get { return _minimum; }
            set { _ = Set(ref _minimum, value); }
        }

        private Func<StartProcessIndicatorCallbackContext, Task> Callback { get; }

        public void Complete(bool? status)
        {
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.Invoke(() => Complete(status));

                return;
            }

            Status = status;
            IsIndeterminate = false;
            Current = Maximum;
        }
    }
}
