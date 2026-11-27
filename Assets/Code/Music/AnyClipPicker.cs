using System;
using DetectiveDave.OverworldPlay;

namespace DetectiveDave.MusicPlay
{
    public sealed class AnyClipPicker : IPianoPicker
    {
        readonly Func<int> next;

        public AnyClipPicker(PianoOrder order, IRandomSource random, int clipCount)
        {
            if (order == PianoOrder.InOrder)
            {
                var sequence = new SequentialPianoPicker(clipCount);
                next = () => sequence.Next();
            }
            else
            {
                var picker = new PianoPicker(random, clipCount);
                next = () => picker.Next();
            }
        }

        public int Next(ChordMoment moment) => next();
    }
}
