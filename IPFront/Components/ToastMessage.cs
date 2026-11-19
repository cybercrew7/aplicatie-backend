namespace IPFront.Components
{
    /// <summary>Visual style of a <see cref="FeedbackToast"/>.</summary>
    public enum ToastKind
    {
        /// <summary>Green accent; dismisses itself after a few seconds.</summary>
        Success,

        /// <summary>Red accent; stays until the user dismisses it.</summary>
        Error
    }

    /// <summary>
    /// A single feedback notification. Create a new instance for every notification:
    /// <see cref="FeedbackToast"/> restarts its auto-dismiss timer when the instance changes,
    /// even if the text is the same as the previous one.
    /// </summary>
    public sealed class ToastMessage
    {
        /// <summary>Creates a notification.</summary>
        public ToastMessage(ToastKind kind, string title, string? message = null)
        {
            Kind = kind;
            Title = title;
            Message = message;
        }

        /// <summary>Success or error style.</summary>
        public ToastKind Kind { get; }

        /// <summary>Short headline, always shown.</summary>
        public string Title { get; }

        /// <summary>Optional detail line.</summary>
        public string? Message { get; }

        /// <summary>Convenience factory for a success notification.</summary>
        public static ToastMessage Success(string title, string? message = null) => new(ToastKind.Success, title, message);

        /// <summary>Convenience factory for an error notification.</summary>
        public static ToastMessage Error(string title, string? message = null) => new(ToastKind.Error, title, message);
    }
}
