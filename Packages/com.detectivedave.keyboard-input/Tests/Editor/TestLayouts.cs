namespace DetectiveDave.KeyboardInput.Tests
{
    /// <summary>
    /// A small layout covering every rule: neighbours, an L-shaped key with aliases, Caps Lock, an inert key,
    /// a duplicate code and an invalid code. Backquote / OEM1 copy the Hungarian macOS layout: the physical Backquote key is inert
    /// and stands in for OEM1. The board, in key units (y down):
    /// <code>
    ///   0    1  1.25 2  2.25 3    4    5    6      7
    /// 0 | A  | S      | Enter (2 x 1)  |       | OEM1 |
    /// 1 | Caps  | `  (inert) | ? | Ent|
    /// 2                             | Z  |   (Z only touches Enter at a corner)
    /// </code>
    /// </summary>
    static class TestLayouts
    {
        public const string Json = @"{
  ""id"": ""test"", ""name"": ""Test"", ""form"": ""iso"", ""digit"": 3, ""unitsWide"": 7, ""unitsHigh"": 3,
  ""keys"": [
    { ""code"": ""KeyA"", ""label"": ""A"", ""rects"": [ { ""x"": 0, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 0.5, ""y"": 0.5 } },
    { ""code"": ""KeyS"", ""label"": ""S"", ""rects"": [ { ""x"": 1, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 1.5, ""y"": 0.5 } },
    { ""code"": ""Enter"", ""label"": ""Enter"", ""rects"": [ { ""x"": 2, ""y"": 0, ""w"": 2, ""h"": 1 }, { ""x"": 3, ""y"": 1, ""w"": 1, ""h"": 1 } ],
      ""center"": { ""x"": 3.5, ""y"": 0.5 }, ""alsoCodes"": [ ""NumpadEnter"", ""NotAKey"" ] },
    { ""code"": ""CapsLock"", ""label"": ""Caps"", ""rects"": [ { ""x"": 0, ""y"": 1, ""w"": 1.25, ""h"": 1 } ], ""center"": { ""x"": 0.625, ""y"": 1.5 } },
    { ""code"": ""Backquote"", ""label"": ""0"", ""rects"": [ { ""x"": 1.25, ""y"": 1, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 1.75, ""y"": 1.5 },
      ""inert"": true, ""unverified"": true, ""note"": ""ISO only"" },
    { ""code"": ""KeyZ"", ""label"": ""Z"", ""rects"": [ { ""x"": 4, ""y"": 2, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 4.5, ""y"": 2.5 } },
    { ""code"": ""OEM1"", ""label"": ""í"", ""rects"": [ { ""x"": 6, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 6.5, ""y"": 0.5 },
      ""alsoCodes"": [ ""Backquote"" ] },
    { ""code"": ""KeyA"", ""label"": ""duplicate"", ""rects"": [ { ""x"": 9, ""y"": 9, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 9.5, ""y"": 9.5 } },
    { ""code"": ""Bogus"", ""label"": ""?"", ""rects"": [ { ""x"": 2.25, ""y"": 1, ""w"": 0.75, ""h"": 1 } ], ""center"": { ""x"": 2.6, ""y"": 1.5 } }
  ]
}";

        public static KeyboardLayout Load() => KeyboardLayout.Parse(Json);
    }
}
