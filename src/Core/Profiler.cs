using System.Collections.Generic;
using Coroutine;
using ImGuiNET;
using OpenTK.Mathematics;
using ZargoEngine.Editor;
using ZargoEngine.Helper;

namespace ZargoEngine.Analysis
{
    public sealed class Profiler : EditorWindow
    {
        readonly float[] values = new float[11];

        public Profiler()
        {
            title = "Analysis";

            for (byte i = 0; i < values.Length; i++)
            {
                values[i] = 1 / Time.DeltaTime;
            }

            CoroutineHandler.Start(UpdateCoroutine());
        }

        IEnumerator<Wait> UpdateCoroutine()
        {
            while (true)
            {
                UpdateProperties();
                yield return new Wait(1);
            }
        }

        bool windowOpen = true;

        public override void DrawWindow()
        {
            if (ImGui.Begin("Analysis.", ref windowOpen, ImGuiWindowFlags.None))
            {
                ImGui.PushStyleColor(ImGuiCol.PlotLines, Color4.Green.ToSystem());
                ImGui.PlotHistogram($"fps: {values[^1]}", ref values[0], values.Length, 0 ," ", 0, 1000);
                ImGui.PopStyleColor();
            }
            ImGui.End();
        }

        private void UpdateProperties()
        {
            for (var i = 0; i < values.Length-1; i++)
            {
                values[i] = values[i + 1];
            }

            values[^1] = 1 / Time.DeltaTime;
        }

        protected override void OnGUI()
        {

        }
    }
}
