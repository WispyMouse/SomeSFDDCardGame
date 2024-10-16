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
    using UnityEngine.TestTools;

    public abstract class EditModeTestBase
    {
        protected StatusEffect DebugStatus;
        protected Element DebugElementOne;
        protected Element DebugElementTwo;

        protected const string DebugElementOneIconText = "<sprite index=0>";
        protected const string DebugElementTwoIconText = "<sprite index=1>";

        protected ReactionWindowContext PlayedFromHandContext;

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = false;

            StatusEffectImport debugStatusEffect = new StatusEffectImport()
            {
                Id = nameof(DebugStatus),
                Effects = new List<EffectOnProcImport>(),
                Name = nameof(DebugStatus)
            };
            StatusEffectDatabase.AddStatusEffectToDatabase(debugStatusEffect);
            DebugStatus = StatusEffectDatabase.GetModel(nameof(DebugStatus));

            ElementImport debugOneElementImport = new ElementImport()
            {
                Id = $"{nameof(DebugElementOne)}id".ToUpper(),
                Name = $"{nameof(DebugElementOne)}name".ToUpper()
            };
            ElementDatabase.AddElement(debugOneElementImport);
            DebugElementOne = ElementDatabase.GetElement(debugOneElementImport.Id);
            DebugElementOne.SpriteIndex = 0;

            ElementImport debugTwoElementImport = new ElementImport()
            {
                Id = $"{nameof(DebugElementTwo)}id".ToUpper(),
                Name = $"{nameof(DebugElementTwo)}name".ToUpper()
            };
            ElementDatabase.AddElement(debugTwoElementImport);
            DebugElementTwo = ElementDatabase.GetElement(debugTwoElementImport.Id);
            DebugElementTwo.SpriteIndex = 1;
        }

        [TearDown]
        public void TearDown()
        {
            GlobalSequenceEventHolder.StopAllSequences();
            CardDatabase.ClearDatabase();
            EnemyDatabase.ClearDatabase();
            ElementDatabase.ClearDatabase();
            StatusEffectDatabase.ClearDatabase();
            GlobalUpdateUX.PlayerMustMakeChoice.RemoveAllListeners();
            GlobalUpdateUX.PendingPlayerChoice = false;
        }
    }
}