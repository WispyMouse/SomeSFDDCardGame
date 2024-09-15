namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class DoNotRepeatRandomDecider<T> : RandomDecider<T>
    {
        public HashSet<T> AlreadyChosen = new HashSet<T>();

        protected override void EliminateOptions(List<T> fromList)
        {
            base.EliminateOptions(fromList);

            List<T> originalList = new List<T>(fromList);

            foreach (T item in this.AlreadyChosen)
            {
                fromList.Remove(item);
            }

            // If we've exhausted the options, reset the chosen list
            if (fromList.Count == 0)
            {
                fromList.AddRange(originalList);
                this.AlreadyChosen.Clear();
            }
        }

        protected override void NoteAsChosen(T chosen)
        {
            base.NoteAsChosen(chosen);

            this.AlreadyChosen.Add(chosen);
        }
    }
}
