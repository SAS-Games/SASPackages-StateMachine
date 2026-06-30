using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    internal static class StateMachineDebugStyles
    {
        internal static readonly Color ActiveBorder = new Color(0.25f, 0.95f, 0.35f, 1f);
        internal static readonly Color Breakpoint = new Color(1f, 0.2f, 0.2f, 1f);
        internal static readonly Color ExitBreakpoint = new Color(1f, 0.55f, 0.12f, 1f);
        internal static readonly Color PausedBorder = new Color(1f, 0.58f, 0.12f, 1f);
        internal static readonly Color PauseGlyph = new Color(0.12f, 0.08f, 0.04f, 1f);
        internal static readonly Color LiveTransition = new Color(1f, 0.82f, 0.12f, 1f);
    }
}
