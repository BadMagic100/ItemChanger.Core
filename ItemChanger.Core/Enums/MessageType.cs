using System;

namespace ItemChanger.Enums;

/// <summary>
/// Enum used to communicate compatibility with different UIDef types.
/// </summary>
[Flags]
public enum MessageType
{
    /// <summary>No supported message types.</summary>
    None = 0,

    /// <summary>
    /// A message which shows a sprite and text without taking control.
    /// </summary>
    SmallPopup = 1 << 0,

    /// <summary>
    /// A message which takes control and shows a fullscreen popup.
    /// </summary>
    LargePopup = 1 << 1,

    /// <summary>
    /// A message which takes control and starts a dialog prompt, similar to speaking to an NPC.
    /// </summary>
    Dialog = 1 << 2,

    /// <summary>Represents any supported message type.</summary>
    Any = SmallPopup | LargePopup | Dialog,
}
