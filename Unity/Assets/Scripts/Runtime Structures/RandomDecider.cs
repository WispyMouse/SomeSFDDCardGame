namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class RandomDecider<T>
    {
        public T ChooseRandomly(List<T> toChooseFrom)
        {
            if (toChooseFrom.Count == 0)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Cannot choose randomly from a list with no elements.", GlobalUpdateUX.LogType.RuntimeError);
                return default(T);
            }

            this.EliminateOptions(toChooseFrom);

            if (toChooseFrom.Count == 0)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Elimination protocol eliminated all possibilities. Implementors should not do that.", GlobalUpdateUX.LogType.RuntimeError);
                return default(T);
            }

            T chosen = this.ChooseOneRandomly(toChooseFrom);
            this.NoteAsChosen(chosen);
            return chosen;
        }

        protected virtual void EliminateOptions(List<T> fromList)
        {

        }

        protected virtual T ChooseOneRandomly(List<T> fromList)
        {
            int randomIndex = UnityEngine.Random.Range(0, fromList.Count);
            return fromList[randomIndex];
        }

        protected virtual void NoteAsChosen(T chosen)
        {

        }
    }
}