using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Control;

namespace TrayMixer
{
    enum MediaAction { Previous, PlayPause, Next }

    sealed class MediaTrack
    {
        readonly GlobalSystemMediaTransportControlsSession session;
        readonly bool previous, next, toggle, play, pause;
        public readonly string Title, Artist;
        public readonly bool Playing;

        public MediaTrack(GlobalSystemMediaTransportControlsSession session,
            GlobalSystemMediaTransportControlsSessionPlaybackInfo playback, string title, string artist)
        {
            this.session = session;
            var controls = playback == null ? null : playback.Controls;
            if (controls != null)
            {
                previous = controls.IsPreviousEnabled;
                next = controls.IsNextEnabled;
                toggle = controls.IsPlayPauseToggleEnabled;
                play = controls.IsPlayEnabled;
                pause = controls.IsPauseEnabled;
            }
            Playing = playback != null && playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            Title = title;
            Artist = artist;
        }

        public bool CanExecute(MediaAction action)
        {
            if (action == MediaAction.Previous) return previous;
            if (action == MediaAction.Next) return next;
            return toggle || (Playing ? pause : play);
        }

        public Task<bool> Execute(MediaAction action)
        {
            if (action == MediaAction.Previous) return WindowsMedia.Completion(session.TrySkipPreviousAsync());
            if (action == MediaAction.Next) return WindowsMedia.Completion(session.TrySkipNextAsync());
            if (toggle) return WindowsMedia.Completion(session.TryTogglePlayPauseAsync());
            return WindowsMedia.Completion(Playing ? session.TryPauseAsync() : session.TryPlayAsync());
        }
    }

    sealed class WindowsMedia
    {
        GlobalSystemMediaTransportControlsSessionManager manager;

        public async Task<MediaTrack> ReadAsync()
        {
            if (manager == null) manager = await Completion(GlobalSystemMediaTransportControlsSessionManager.RequestAsync());
            var session = manager.GetCurrentSession();
            if (session == null || IsTeams(session.SourceAppUserModelId))
            {
                session = null;
                foreach (var candidate in manager.GetSessions())
                {
                    if (IsTeams(candidate.SourceAppUserModelId)) continue;
                    var playback = candidate.GetPlaybackInfo();
                    if (playback != null && playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                    {
                        session = candidate;
                        break;
                    }
                    if (session == null) session = candidate;
                }
            }
            if (session == null) return null;

            string title = null, artist = null;
            try
            {
                var properties = await Completion(session.TryGetMediaPropertiesAsync());
                if (properties != null) { title = properties.Title; artist = properties.Artist; }
            }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine(e); }
            return new MediaTrack(session, session.GetPlaybackInfo(), title, artist);
        }

        static bool IsTeams(string source)
        {
            if (string.IsNullOrEmpty(source)) return false;
            const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
            return source.StartsWith("MSTeams_", comparison)
                || source.Equals("com.squirrel.Teams.Teams", comparison)
                || source.Equals("Teams.exe", comparison)
                || source.Equals("ms-teams.exe", comparison)
                // Microsoft Teams installed as a Chrome web app.
                || source.EndsWith("._crx_ompifgpmddndfeacglnodjjndh", comparison);
        }

        // The inbox WinRT metadata is split by namespace. Use the operation's own
        // completion handler so the build does not require the SDK's merged Windows.winmd.
        internal static Task<T> Completion<T>(IAsyncOperation<T> operation)
        {
            var task = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            operation.Completed = (op, status) =>
            {
                if (status == AsyncStatus.Canceled) { task.TrySetCanceled(); return; }
                try { task.TrySetResult(op.GetResults()); }
                catch (Exception e) { task.TrySetException(e); }
            };
            return task.Task;
        }
    }
}
