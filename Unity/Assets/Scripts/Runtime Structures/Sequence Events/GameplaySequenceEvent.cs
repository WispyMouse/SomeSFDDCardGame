namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class GameplaySequenceEvent
    {
        /// <summary>
        /// This action is run AFTER the AnimationDelegate has finished.
        /// </summary>
        public Action ConsequentialAction;

        public delegate IEnumerator SequenceAnimationDelegate();

        /// <summary>
        /// This animation happens leading UP TO the ConsequentialAction.
        /// Optional, may not be implemented for this SequenceEvent.
        /// </summary>
        /// <returns></returns>
        public SequenceAnimationDelegate AnimationDelegate;

        public GameplaySequenceEvent(Action consequentialAction, SequenceAnimationDelegate animationDelegate = null)
        {
            this.ConsequentialAction = consequentialAction;
            this.AnimationDelegate = animationDelegate;
        }
    }
}