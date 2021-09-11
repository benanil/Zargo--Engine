using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Coroutine;
using ImGuiNET;
using OpenTK.Mathematics;
using ZargoEngine.Editor;
using ZargoEngine.Helper;

namespace ZargoEngine.Analysis
{
    public sealed class Profiler : EditorWindow
    {
        Process EngineProcess;

        Queue<int> ramUsageQueue = new Queue<int>();
        int[] ramUsageArray;
        int[] times => Enumerable.Range(0, ramUsageArray.Length).ToArray();

        public bool isActive;

        private float fps = 60;

        public static Profiler instance;

        public Profiler()
        {
            instance = this;
            // IntPtr ImPlotContext = ImPlot.CreateContext();
            // ImPlot.SetCurrentContext(ImPlotContext);

            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            EngineProcess = Process.GetCurrentProcess();
            CoroutineHandler.Start(UpdateCoroutine());
            title = "Analysis";
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

                ImGui.TextColored(Color4.Green.ToSystem(),"FPS: " + fps.ToString());
                //if (ImPlot.BeginPlot("Analysis","profiler"))
                //{ 
                //    //ImPlot.PlotLine("Ram Usage",ref times[0], ref ramUsageArray[0], ramUsageArray.Length);
                //}
                //ImPlot.EndPlot();
            }
            ImGui.End();
        }

        public void UpdateProperties()
        {
            //if (!isActive) return;
            //ramUsageQueue.Enqueue((int)(EngineProcess.WorkingSet64 / 1012 / 1024));
            //ramUsageArray = ramUsageQueue.ToArray();
            //
            //CoroutineHandler.InvokeLater(new Wait(20), () =>
            //{
            //    ramUsageQueue.Dequeue();
            //});
            fps = 1 / Time.DeltaTime;
        }

        protected override void OnGUI()
        {

        }
    }
}
