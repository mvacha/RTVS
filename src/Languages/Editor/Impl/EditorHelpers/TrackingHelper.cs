﻿using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.EditorHelpers
{
    public static class TrackingHelper
    {
        public static int GetCurrentPosition(this ITrackingPoint trackingPoint)
        {
            ITextSnapshot snapshot = trackingPoint.TextBuffer.CurrentSnapshot;
            return trackingPoint.GetPosition(snapshot);
        }

        public static int GetCurrentStart(this ITrackingSpan trackingSpan)
        {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetStartPoint(snapshot).Position;
        }

        public static int GetCurrentEnd(this ITrackingSpan trackingSpan)
        {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetEndPoint(snapshot).Position;
        }

        public static Span GetCurrentSpan(this ITrackingSpan trackingSpan)
        {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetSpan(snapshot).Span;
        }

        public static string GetCurrentText(this ITrackingSpan trackingSpan)
        {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetText(snapshot);
        }
    }
}

