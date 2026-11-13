using System.Text;
using Unity.Profiling;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Play modunda kare başına bellek ayırma, draw call ve kare süresini örnekler.
    /// Kullanım: PerfProbe.Start(); birkaç saniye oyna; PerfProbe.Report().
    /// Editor'de ölçülen değerler cihazdakinden farklıdır; önce/sonra karşılaştırması içindir.
    /// </summary>
    public static class PerfProbe
    {
        const int Capacity = 2000;

        static ProfilerRecorder _gc, _batches, _drawCalls, _setPass, _triangles, _mainThread;

        public static string Start()
        {
            Stop();
            _gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", Capacity);
            _batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", Capacity);
            _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", Capacity);
            _setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", Capacity);
            _triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", Capacity);
            _mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", Capacity);
            return "PerfProbe started";
        }

        public static string Report()
        {
            if (!_gc.Valid) return "PerfProbe not started";

            var text = new StringBuilder();
            text.Append("frames=").Append(_gc.Count);
            Append(text, "gcBytes/frame", _gc, 1.0);
            Append(text, "batches", _batches, 1.0);
            Append(text, "drawCalls", _drawCalls, 1.0);
            Append(text, "setPass", _setPass, 1.0);
            Append(text, "tris", _triangles, 1.0);
            Append(text, "mainThreadMs", _mainThread, 1e-6);
            Stop();
            return text.ToString();
        }

        static void Append(StringBuilder text, string label, ProfilerRecorder recorder, double scale)
        {
            int count = recorder.Count;
            if (count == 0) { text.Append(" | ").Append(label).Append("=n/a"); return; }

            // Ortanca da yazılır: editörün kendi işleri (derleme, pencere çizimi) tek tük dev
            // sıçramalar yapar ve ortalamayı bozar; oyunun olağan karesini ortanca gösterir.
            var values = new double[count];
            double sum = 0, max = 0;
            for (int i = 0; i < count; i++)
            {
                double value = recorder.GetSample(i).Value * scale;
                values[i] = value;
                sum += value;
                if (value > max) max = value;
            }

            System.Array.Sort(values);
            text.Append(" | ").Append(label)
                .Append(" med=").Append(values[count / 2].ToString("F2"))
                .Append(" avg=").Append((sum / count).ToString("F2"))
                .Append(" max=").Append(max.ToString("F2"));
        }

        static void Stop()
        {
            if (_gc.Valid) _gc.Dispose();
            if (_batches.Valid) _batches.Dispose();
            if (_drawCalls.Valid) _drawCalls.Dispose();
            if (_setPass.Valid) _setPass.Dispose();
            if (_triangles.Valid) _triangles.Dispose();
            if (_mainThread.Valid) _mainThread.Dispose();
        }
    }
}
