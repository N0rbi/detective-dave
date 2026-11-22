using DetectiveDave.MusicPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(ChordChart))]
    sealed class ChordChartInspector : Editor
    {
        const float TimelineHeight = 58f;
        const float LabelRowHeight = 14f;

        static readonly Color Background = new Color(0.16f, 0.16f, 0.18f);
        static readonly Color BarLine = new Color(1f, 1f, 1f, 0.25f);
        static readonly Color[] SegmentColors =
        {
            new Color(0.30f, 0.50f, 0.75f),
            new Color(0.35f, 0.65f, 0.45f),
            new Color(0.80f, 0.60f, 0.30f),
            new Color(0.75f, 0.35f, 0.40f)
        };

        public override void OnInspectorGUI()
        {
            var chart = (ChordChart)target;
            DrawTimeline(chart);
            DrawProblems(chart);
            DrawDefaultInspector();
            DrawSummary(chart);
        }

        static void DrawTimeline(ChordChart chart)
        {
            var rect = GUILayoutUtility.GetRect(0f, TimelineHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type != EventType.Repaint) return;
            EditorGUI.DrawRect(rect, Background);
            DrawSegments(chart, rect);
            DrawBarLines(chart, rect);
        }

        static void DrawSegments(ChordChart chart, Rect rect)
        {
            for (var index = 0; index < chart.Segments.Count; index++)
            {
                var segment = chart.Segments[index];
                var span = ChordTimelineLayout.SpanOf(segment.StartBeat, segment.EndBeat, chart.LoopLengthBeats);
                var block = new Rect(rect.x + span.Start * rect.width, rect.y + LabelRowHeight, span.Length * rect.width, rect.height - LabelRowHeight);
                EditorGUI.DrawRect(new Rect(block.x + 1f, block.y, Mathf.Max(block.width - 2f, 1f), block.height), SegmentColors[index % SegmentColors.Length]);
                GUI.Label(block, segment.ChordName, CenteredLabel());
            }
        }

        static void DrawBarLines(ChordChart chart, Rect rect)
        {
            var lines = ChordTimelineLayout.BarLines(chart.LoopLengthBars);
            for (var bar = 0; bar < lines.Count; bar++)
            {
                var x = rect.x + lines[bar] * rect.width;
                EditorGUI.DrawRect(new Rect(x, rect.y, 1f, rect.height), BarLine);
                if (bar < chart.LoopLengthBars) GUI.Label(new Rect(x + 2f, rect.y, 40f, LabelRowHeight), $"bar {bar + 1}", EditorStyles.miniLabel);
            }
        }

        static GUIStyle CenteredLabel() => new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };

        static void DrawProblems(ChordChart chart)
        {
            var loop = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicSetupPaths.LoopPath);
            foreach (var problem in ChordChartValidation.Problems(chart, loop ? loop.length : 0f))
                EditorGUILayout.HelpBox(problem, MessageType.Warning);
        }

        static void DrawSummary(ChordChart chart)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("In words", EditorStyles.boldLabel);
            foreach (var segment in chart.Segments) EditorGUILayout.LabelField(ChordChartSummary.SegmentLine(segment), EditorStyles.miniLabel);
            EditorGUILayout.Space();
            foreach (var pitch in chart.ClipPitches) EditorGUILayout.LabelField(ChordChartSummary.ClipLine(pitch, chart.Segments), EditorStyles.miniLabel);
        }
    }
}
