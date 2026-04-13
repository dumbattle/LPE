using LPE.Animation;

namespace LPE.Animation {
    public class AnimationPlayer<TTarget> : IAnimationPlayer {
        TTarget target;
        public IAnimationDefinition<TTarget> anim { get; private set; }
        bool doLoop;
        RenderTarget renderTarget = RenderTarget.none;

        bool firstFrameForFrame;
        int timer;
        int currentFrame;
        bool done;


        public void Set(TTarget target, IAnimationDefinition<TTarget> anim, bool doLoop) {
            this.target = target;
            this.anim = anim;
            this.doLoop = doLoop;

            timer = 1;
            currentFrame = -1;
            done = false;
            renderTarget = RenderTarget.spriteRenderer;
        }
     
        public void Update() {
            // chack valid
            if (renderTarget == RenderTarget.none || anim == null) {
                return;
            }

            // done and no loop => stop
            if (IsDone() && !doLoop) {
                return;
            }
            firstFrameForFrame = false;
            // step
            timer--;

            // done with current frame
            var s = new LoopSafety(100);
            while (timer <= 0) {
                s.Next();
                currentFrame++;
                firstFrameForFrame = true;

                // past last frame
                if (currentFrame >= anim.numFrames) {
                    // signal done
                    done = true;

                    // stop if no loop
                    if (!doLoop) {
                        return;
                    }
                    // loop
                    else {
                        currentFrame = 0;
                    }
                }

                // update state
                timer += anim.GetFrameDuration(currentFrame);
                anim.DrawFrame(currentFrame, target);
            }
        }

        public bool IsDone() {
            return done;
        }

        public AnimationFrameTags GetTags(bool firstFrameOnly) {
            if (firstFrameOnly && firstFrameForFrame) {
                return 0;
            }
            return anim?.GetFrameTags(currentFrame) ?? 0;
        }

        enum RenderTarget {
            none,
            spriteRenderer,
            image
        }
    }
}
