namespace DockerAppStarter.Gui
{
    internal class StartProcessIndicatorCallbackContext
    {
        public StartProcessIndicatorCallbackContext(StartProcessIndicatorViewModel parent, CancellationToken cancellationToken = default)
        {
            Parent = parent;
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; set; }

        private StartProcessIndicatorViewModel Parent { get; }

        public void Complete(bool? status)
        {
            Parent.Complete(status);
        }
    }
}
