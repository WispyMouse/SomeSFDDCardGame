namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using UnityEngine;

    public class AnimationRunnerController : MonoBehaviour
    {
        Dictionary<IAnimationPuppet, Queue<AnimationQueueToken>> WaitingQueue { get; set; } = new Dictionary<IAnimationPuppet, Queue<AnimationQueueToken>>();

        class AnimationQueueToken
        {
            public bool ShouldStartAnimation = false;
            public bool FinishedAnimation = false;
        }

        public IEnumerator AnimateShoveAttack(IAnimationPuppet attacking, IAnimationPuppet beingAttacked)
        {
            AnimationQueueToken queueToken = new AnimationQueueToken();
            yield return this.YieldUntilAllActorsReady(queueToken, attacking, beingAttacked);

            Vector3 attackingPuppetOriginalPosition = attacking.OwnTransform.position;
            Vector3 beingAttackedPuppetOriginalPosition = beingAttacked.OwnTransform.position;

            const float DistanceToShoveTowards = 1.25f;
            const float AttackAnimationTime = .15f;
            Vector3 attackTowardsPoint = Vector3.MoveTowards(attackingPuppetOriginalPosition, beingAttackedPuppetOriginalPosition, DistanceToShoveTowards);

            float curAnimationTime = 0;
            do
            {
                curAnimationTime += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(attackingPuppetOriginalPosition, attackTowardsPoint, curAnimationTime / AttackAnimationTime);
                attacking.OwnTransform.position = newPosition;
                yield return new WaitForFixedUpdate();
            } while (curAnimationTime < AttackAnimationTime);

            if (beingAttacked.IsNotDestroyed)
            {
                const float DistanceToKnockAway = .5f;
                const float KnockbackAnimationTimeTotal = .15f;
                Vector3 knockbackAwayTowardsPoint = Vector3.MoveTowards(beingAttackedPuppetOriginalPosition, attackingPuppetOriginalPosition, -DistanceToKnockAway);

                curAnimationTime = 0;
                do
                {
                    if (!beingAttacked.IsNotDestroyed)
                    {
                        break;
                    }

                    curAnimationTime += Time.deltaTime;
                    Vector3 newPosition = Vector3.Lerp(beingAttackedPuppetOriginalPosition, knockbackAwayTowardsPoint, Mathf.PingPong(curAnimationTime * 2, KnockbackAnimationTimeTotal) / KnockbackAnimationTimeTotal);
                    beingAttacked.OwnTransform.position = newPosition;
                    yield return new WaitForFixedUpdate();
                } while (curAnimationTime < KnockbackAnimationTimeTotal);

                if (beingAttacked.IsNotDestroyed)
                {
                    beingAttacked.OwnTransform.position = beingAttackedPuppetOriginalPosition;
                }
            }

            const float ReturnAnimationTime = .05f;
            curAnimationTime = 0;
            do
            {
                curAnimationTime += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(attackTowardsPoint, attackingPuppetOriginalPosition, curAnimationTime / ReturnAnimationTime);
                attacking.OwnTransform.position = newPosition;
                yield return new WaitForFixedUpdate();
            } while (curAnimationTime < ReturnAnimationTime);

            attacking.OwnTransform.position = attackingPuppetOriginalPosition;

            queueToken.FinishedAnimation = true;
            this.RemoveTokenFromQueues(queueToken, attacking, beingAttacked);
        }

        public IEnumerator AnimateUpwardNod(IAnimationPuppet nodding)
        {
            AnimationQueueToken queueToken = new AnimationQueueToken();
            yield return this.YieldUntilAllActorsReady(queueToken, nodding);

            Vector3 noddingPuppetOriginalPosition = nodding.OwnTransform.position;

            const float NodUpwardsDistance = .8f;
            const float NodToTopTime = .1f;
            Vector3 nodTowardsPoint = noddingPuppetOriginalPosition + Vector3.up * NodUpwardsDistance;

            float curAnimationTime = 0;
            do
            {
                curAnimationTime += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(noddingPuppetOriginalPosition, nodTowardsPoint, curAnimationTime / NodToTopTime);
                nodding.OwnTransform.position = newPosition;
                yield return new WaitForFixedUpdate();
            } while (curAnimationTime < NodToTopTime);

            curAnimationTime = 0;

            const float ReturnAnimationTime = .05f;
            curAnimationTime = 0;
            do
            {
                curAnimationTime += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(nodTowardsPoint, noddingPuppetOriginalPosition, curAnimationTime / ReturnAnimationTime);
                nodding.OwnTransform.position = newPosition;
                yield return new WaitForFixedUpdate();
            } while (curAnimationTime < ReturnAnimationTime);

            nodding.OwnTransform.position = noddingPuppetOriginalPosition;

            queueToken.FinishedAnimation = true;
            this.RemoveTokenFromQueues(queueToken, nodding);
        }

        IEnumerator YieldUntilAllActorsReady(AnimationQueueToken queueToken, params IAnimationPuppet[] actors)
        {
            // For each actor, put them in the queue. Create a queue if necessary.
            foreach (IAnimationPuppet curPuppet in actors)
            {
                if (!this.WaitingQueue.TryGetValue(curPuppet, out Queue<AnimationQueueToken> tokenQueue))
                {
                    tokenQueue = new Queue<AnimationQueueToken>();
                    this.WaitingQueue.Add(curPuppet, tokenQueue);
                }

                tokenQueue.Enqueue(queueToken);
            }

            // Then, if there are exactly one item in each of the actor's queues, signal this token should start
            bool foundAnyNoneOne = false;
            foreach (IAnimationPuppet curPuppet in actors)
            {
                if (this.WaitingQueue[curPuppet].Count != 1)
                {
                    foundAnyNoneOne = true;
                    break;
                }
            }

            // If they're all one, immediately start the token
            if (!foundAnyNoneOne)
            {
                queueToken.ShouldStartAnimation = true;
            }

            // Otherwise, loop until this queue token is on top
            bool timeToStart = false;
            while (!timeToStart)
            {
                yield return new WaitForFixedUpdate();

                bool allAreReady = true;

                foreach (IAnimationPuppet curPuppet in actors)
                {
                    if (this.WaitingQueue[curPuppet].Peek() != queueToken)
                    {
                        allAreReady = false;
                        break;
                    }
                }

                if (allAreReady)
                {
                    timeToStart = true;
                    break;
                }
            }

            queueToken.ShouldStartAnimation = true;
        }

        void RemoveTokenFromQueues(AnimationQueueToken queueToken, params IAnimationPuppet[] actors)
        {
            foreach (IAnimationPuppet curPuppet in actors)
            {
                Queue<AnimationQueueToken> tokenQueue = this.WaitingQueue[curPuppet];
                
                if (tokenQueue.Peek() != queueToken)
                {
                    Debug.LogError($"Next animation token is expected to be the provided queueToken, but it isn't. Animations might be in an unexpected state.");
                }

                tokenQueue.Dequeue();
            }
        }
    }
}