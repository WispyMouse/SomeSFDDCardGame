namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public abstract class EditModeTestBase
    {
        protected StatusEffect DebugStatus;

        [SetUp]
        public void SetUp()
        {
            StatusEffectImport debugStatusEffect = new StatusEffectImport()
            {
                Id = nameof(DebugStatus),
                Effects = new List<EffectOnProcImport>(),
                Name = nameof(DebugStatus)
            };
            StatusEffectDatabase.AddStatusEffectToDatabase(debugStatusEffect);
            DebugStatus = StatusEffectDatabase.GetModel(nameof(DebugStatus));
        }

        [TearDown]
        public void TearDown()
        {
            GlobalSequenceEventHolder.StopAllSequences();
            EnemyDatabase.ClearDatabase();
            StatusEffectDatabase.ClearDatabase();
        }
    }
}