namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public abstract class EditModeTestBase
    {
        [TearDown]
        public void TearDown()
        {
            GlobalSequenceEventHolder.StopAllSequences();
            EnemyDatabase.ClearDatabase();
            StatusEffectDatabase.ClearDatabase();
        }
    }
}