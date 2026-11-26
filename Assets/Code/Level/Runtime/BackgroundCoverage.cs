using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    public static class BackgroundCoverage
    {
        public static ScreenRect Still(Vector2 homeCenter, float homeOrthographicSize, float aspect, float overscanShare) =>
            Overscanned(HomeFrustum(homeCenter, homeOrthographicSize, aspect), overscanShare);

        public static ScreenRect Zooming(Vector2 homeCenter, float homeOrthographicSize, float aspect, ScreenRect board, float zoomFactor, float overscanShare)
        {
            var home = HomeFrustum(homeCenter, homeOrthographicSize, aspect);
            var zoomedHalf = home.Size * 0.5f * zoomFactor;
            return Overscanned(home.Union(board.Expanded(zoomedHalf)), overscanShare);
        }

        static ScreenRect HomeFrustum(Vector2 center, float orthographicSize, float aspect) =>
            ScreenRect.Around(center, new Vector2(orthographicSize * aspect, orthographicSize));

        static ScreenRect Overscanned(ScreenRect rect, float overscanShare) => rect.Expanded(rect.Size * Mathf.Max(0f, overscanShare));
    }
}
