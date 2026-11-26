using System.Globalization;
using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    public static class BackgroundTraceLine
    {
        public const string Header = "time;camera x;camera y;orthographic size;aspect;view min;view max;coverage min;coverage max;sprite min;sprite max;backdrop panel visible";

        public static string Format(float time, Vector2 cameraPosition, float orthographicSize, float aspect, ScreenRect view, ScreenRect coverage, ScreenRect sprite, bool backdropVisible) =>
            string.Join(";", Number(time), Number(cameraPosition.x), Number(cameraPosition.y), Number(orthographicSize), Number(aspect),
                Point(view.Min), Point(view.Max), Point(coverage.Min), Point(coverage.Max), Point(sprite.Min), Point(sprite.Max), backdropVisible ? "yes" : "no");

        public static bool ViewIsInside(ScreenRect view, ScreenRect sprite) =>
            view.Min.x >= sprite.Min.x && view.Min.y >= sprite.Min.y && view.Max.x <= sprite.Max.x && view.Max.y <= sprite.Max.y;

        static string Number(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        static string Point(Vector2 point) => Number(point.x) + "," + Number(point.y);
    }
}
