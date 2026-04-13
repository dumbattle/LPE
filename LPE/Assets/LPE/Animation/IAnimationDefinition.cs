namespace LPE.Animation {
    public interface IAnimationDefinition<TTarget> {
        int numFrames { get; }

        int GetFrameDuration(int frame);
        AnimationFrameTags GetFrameTags(int frame);

        void DrawFrame(int frame, TTarget target);
    }
}
