namespace DockerAppStarter.Gui.ProcessStartup
{
    internal class StartProcessIndicatorCallbackContext(StartProcessIndicatorViewModel parent, CancellationToken cancellationToken = default)
    {
        public CancellationToken CancellationToken { get; set; } = cancellationToken;

        private StartProcessIndicatorViewModel Parent { get; } = parent;

        public void Complete(bool? status)
        {
            Parent.Complete(status);
        }
    }
}
