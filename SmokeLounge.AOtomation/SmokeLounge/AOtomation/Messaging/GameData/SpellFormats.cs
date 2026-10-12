// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellFormats.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellFormats type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;

    using AOSharp.Common.GameData;

    using F = AOSharp.Common.GameData.SpellFunction;
    using P = AOSharp.Common.GameData.SpellPropertyOperator;

    /// <summary>
    /// The client's spell formats: each function's arguments after the four every function has. A function not here
    /// fails the read. ApplyTexture's own arguments follow a layout of their own (see <see cref="SpellData.TextureMarker"/>).
    /// </summary>
    public static class SpellFormats
    {
        #region Constants

        /// <summary>Every format's version.</summary>
        public const int Version = 4;

        /// <summary>The text slots a spell has.</summary>
        public const int StringCount = 3;

        /// <summary>What ApplyTexture's marker is from 100 on, the layout with BodyPart and the two args.</summary>
        public const int TextureMarker = 100;

        #endregion

        #region Static Fields

        /// <summary>The four every function has, first.</summary>
        public static readonly SpellArgument[] Common = { P.Duration, P.Interval, P.ApplyOn, P.TargetList };

        private static readonly SpellArgument[] NoArguments = new SpellArgument[0];

        private static readonly Dictionary<SpellFunction, SpellArgument[]> Formats =
            new Dictionary<SpellFunction, SpellArgument[]>();

        #endregion

        #region Constructors and Destructors

        static SpellFormats()
        {
            Add(new SpellArgument[] { P.Stat, P.Min, P.Max, P.DamageType }, F.Hit, F.HitPerk);
            Add(new SpellArgument[] { P.TargetType, P.TargetInstance, P.AnimEffect, P.AnimFlag, P.Radius }, F.AnimEffect, F.DirectItemAnimEffect);
            Add(new SpellArgument[] { P.Texture, P.MeshEffect }, F.BackMesh, F.HeadMesh, F.Mesh, F.ShoulderMesh);
            Add(new SpellArgument[] { P.ItemType, P.ItemInstance, P.RelXPos, P.RelYPos, P.RelZPos }, F.Creation);
            Add(new SpellArgument[] { P.PoisonType, P.PoisonDifficulty, P.Stat, P.Min, P.Max, P.Radius, P.ItemId }, F.Poison);
            Add(new SpellArgument[] { P.RemoveType }, F.Remove53008);
            Add(new SpellArgument[] { P.TextID, P.Radius }, F.TextEffect);
            Add(new SpellArgument[] { P.AudioEffectID, P.AudioEffectDuration, P.AudioEffectRepeat, P.AudioEffectVolume, P.Radius }, F.AudioEffect);
            Add(new SpellArgument[] { P.Stat, P.Value, P.SkillType }, F.ModifyNanoStat);
            Add(new SpellArgument[] { P.PoisonType }, F.PoisonRemove);
            Add(new SpellArgument[] { P.Stat, P.Value, P.Radius, P.Icon, P.TimedLength }, F.ModifyTemp);
            Add(new SpellArgument[] { P.Action }, F.Criteria);
            Add(new SpellArgument[] { P.RelXPos, P.RelYPos, P.RelZPos, P.TeleportDest }, F.SaveChar, F.TeleportPerk, F.Unk53084, F.Unk53136, F.Unk53142);
            Add(new SpellArgument[] { P.Music }, F.PlayMusic, F.StopMusic);
            Add(new SpellArgument[] { P.Value }, F.CastNano, F.CastNanoIfPossible, F.CastNanoIfPossibleOnFightTarget, F.CreateApartment, F.DeleteQuest, F.FailQuest, F.NpcFreezeHateList, F.NpcHateListTarget, F.NpcMoveForward, F.NpcPushScript, F.NpcSetSneakMode, F.NpcToggleFightModeRegenrate, F.NpcTurnToTarget, F.RemoveNano, F.SetGovernType, F.Unk53085, F.Unk53207, F.Upload);
            Add(new SpellArgument[] { P.CatMeshEffect }, F.CatMesh);
            Add(new SpellArgument[] { P.Stat, P.TargetExpression, P.ToClient }, F.Expression);
            Add(new SpellArgument[] { P.Pos, P.Play, P.Speed }, F.Anim);
            Add(new SpellArgument[] { P.Action, P.Stat, P.Value }, F.AddSkill, F.CreateStat, F.LockPerk, F.LockSkill, F.Set);
            Add(new SpellArgument[] { P.Action, P.Value }, F.AddDifficulty);
            Add(new SpellArgument[] { P.Value, P.GfxLife, P.GfxSize, P.GfxRed, P.GfxGreen, P.GfxBlue, P.GfxFade }, F.GfxEffect);
            Add(new SpellArgument[] { P.TargetType, P.TargetInstance, P.AnimEffect, P.AnimFlag, P.Sex, P.Breed, P.Radius }, F.ItemAnimEffect);
            Add(new SpellArgument[] { P.WeaponEffect }, F.EndEffect, F.StartEffect);
            Add(new SpellArgument[] { P.GfxRed, P.GfxGreen, P.GfxBlue }, F.WeaponEffectColor);
            Add(new SpellArgument[] { P.ItemType, P.ItemInstance, P.BaseAmount, P.RegenAmount, P.RegenInterval, P.RarityValue, P.Cost }, F.AddShopItem);
            Add(new SpellArgument[] { Text(0), P.Arg1, P.Arg2, P.Arg3, P.Arg4, P.ToClient }, F.SystemText);
            Add(new SpellArgument[] { P.Stat, P.Value }, F.ChangeVariable2, F.ClearFlag, F.ModifyLvlScaling, F.ModifyPercentage, F.ModifyStat, F.Reduce, F.SetFlag, F.TempChange, F.ToggleFlag, F.Unk53205, F.Unk53237, F.Update);
            Add(new SpellArgument[] { P.Action, P.AnimEffect }, F.AnimAction);
            Add(new SpellArgument[] { Text(0) }, F.BodyMesh, F.DialogFeedback, F.Name, F.Unk53200);
            Add(NoArguments, F.CharmNpc, F.ClearHateList, F.Clone, F.CloseNpcDialog, F.Daze, F.Deploy, F.DestroyAllHumans, F.DestroyItem, F.DisableDefShield, F.EnableFlight, F.EnableRaidLockOnPf, F.EnterApartment, F.Fear, F.GenerateName, F.Mezz, F.MindControl, F.NextHead, F.NpcCloneTarget, F.NpcCreateOet, F.NpcDisableMovement, F.NpcEnableDieOfBoredom, F.NpcEnableGroundToAirCombat, F.NpcEnablePvpRules, F.NpcFakeAttackOnTarget, F.NpcHateListTargetAggroers, F.NpckillTarget, F.NpcPopScript, F.NpcSendPlaySync, F.NpcSetConfigStats, F.NpcStartSurrender, F.NpcStopMoving, F.NpcStoppedDuel, F.NpcStopSurrender, F.NpcSummonEnemy, F.NpcTargetHasItem, F.NpcTeleportToSpawnPoint, F.NpcTryGroupForm, F.NpcUseSpecialAttackItem, F.OpenBank, F.OpenNpcDialog, F.OrganizationGrid, F.Pacify, F.PrevHead, F.RecallToAnchor, F.RefreshModel, F.RemoveBuffs, F.ResetAllPerks, F.SaveHere, F.SetAnchor, F.Stun, F.SummonPlayer, F.SummonTeamMates, F.TeleportLastSave, F.Unk53094, F.Unk53106, F.Unk53135, F.Unk53241, F.Unk53245, F.Unk53246, F.Unk53255);
            Add(new SpellArgument[] { P.SeeSkin, P.BodyPart, P.MeshEffect }, F.StrTexture);
            Add(new SpellArgument[] { P.BodyPart, Text(0) }, F.StrMesh);
            Add(new SpellArgument[] { P.BodyPart, P.MeshEffect }, F.AttractorMesh);
            Add(new SpellArgument[] { P.RelXPos, P.RelYPos, P.RelZPos, P.Speed, P.Value }, F.WayPoint);
            Add(new SpellArgument[] { Text(0), P.Value }, F.FloatText, F.NpcSay, F.SetScriptConfig, F.Talk);
            Add(new SpellArgument[] { P.ItemType, P.ItemInstance, P.TeleportDest, P.Unk8 }, F.LineTeleport);
            Add(new SpellArgument[] { P.Value, P.TimedLength }, F.ChangeMesh);
            Add(new SpellArgument[] { P.Hash, P.Unk79, P.Unk80, P.BaseAmount, P.RegenAmount, P.RegenInterval, P.RarityValue, P.Cost }, F.AddShopItem2);
            Add(new SpellArgument[] { P.Unk92, P.Unk93, P.Radius, P.Value }, F.NpcSelectTarget);
            Add(new SpellArgument[] { P.Hash, P.HighItemIdMaxQl, P.TimeExist, P.Flags, P.RelXPos, P.RelYPos, P.RelZPos, P.Unk2, P.Unk1, P.Unk3 }, F.SpawnMonster);
            Add(new SpellArgument[] { P.Hash, P.HighItemIdMaxQl, P.HighItemIdMinQl, P.TimeExist, P.Flags, P.RelXPos, P.RelYPos, P.RelZPos }, F.SpawnItem);
            Add(new SpellArgument[] { P.Unk86, P.ScreenVfx, P.Flags, P.Unk89 }, F.AttractorEffect, F.AttractorEffect1, F.AttractorEffect2);
            Add(new SpellArgument[] { P.ItemId }, F.CastBuff, F.CastOnPets, F.CastOnPf, F.CastTeam);
            Add(new SpellArgument[] { P.Flags, P.TimedLength }, F.Disallow, F.ImplantAccess);
            Add(new SpellArgument[] { P.AudioEffectID }, F.Unk53072);
            Add(new SpellArgument[] { P.Stat, P.Min, P.Max, P.DamageType, P.Radius }, F.AoeDmg);
            Add(new SpellArgument[] { P.Unk119, P.Unk127 }, F.NpcFightSelected);
            Add(new SpellArgument[] { P.Unk23 }, F.NpcSocialAnim);
            Add(new SpellArgument[] { P.ScreenVfx, P.Flags, P.TimedLength }, F.ScreenEffect);
            Add(new SpellArgument[] { P.PfModelIdentityType, P.PfModelIdentityInstance, P.Unk4, P.Unk5, P.Unk6, P.Unk7 }, F.ProxyTeleportWithPetHandling, F.TeleportProxy);
            Add(new SpellArgument[] { P.PfModelIdentityType, P.PfModelIdentityInstance, P.Unk4, P.Unk5, P.Unk6, P.Unk7, P.Unk8 }, F.InstancedPlayerCity, F.Teleport);
            Add(new SpellArgument[] { P.ItemId, P.Radius }, F.CastPerk, F.Unk53152);
            Add(new SpellArgument[] { P.Unk101 }, F.AddBattleStationQueue, F.RegisterControlPoint, F.Unk53088);
            Add(new SpellArgument[] { P.ItemId, P.Value }, F.CastChance);
            Add(new SpellArgument[] { P.Unk125 }, F.NpcGetTargetHateList);
            Add(new SpellArgument[] { P.Unk122 }, F.NpcSetMaster);
            Add(new SpellArgument[] { P.Hash, P.HighItemIdMaxQl, P.HighItemIdMinQl }, F.Unk53093);
            Add(new SpellArgument[] { P.Unk118, P.Unk119, P.Unk127 }, F.NpcFollowSelected);
            Add(new SpellArgument[] { P.Hash2 }, F.EquipMonsterWeapon, F.Unk53099);
            Add(new SpellArgument[] { P.Unk110, P.Unk111, P.Unk112, P.Unk113, P.Unk114 }, F.Unk53101);
            Add(new SpellArgument[] { P.Unk109 }, F.NpcApplyNanoFormula);
            Add(new SpellArgument[] { P.Unk106, P.Unk107, P.Unk108 }, F.NpcSendCommand);
            Add(new SpellArgument[] { P.HighItemIdMaxQl, P.NanoSchool, P.Value }, F.Remove);
            Add(new SpellArgument[] { Text(0), P.Value, P.Unk117 }, F.InputBox);
            Add(new SpellArgument[] { P.Unk10, P.Unk11, P.Value, P.TimedLength, P.Radius }, F.Taunt);
            Add(new SpellArgument[] { P.Unk123 }, F.NpcClearSignal, F.RemoveTrigger);
            Add(new SpellArgument[] { P.Unk21 }, F.NpcCallForHelp);
            Add(new SpellArgument[] { P.Unk10, P.Unk11, P.Value }, F.Unk53123);
            Add(new SpellArgument[] { P.Value, P.Hash, P.HighItemIdMaxQl, P.HighItemIdMinQl, P.TimeExist, P.Flags, P.RelXPos, P.RelYPos, P.RelZPos }, F.RndSpawnItem);
            Add(new SpellArgument[] { P.Value, P.Hash, P.HighItemIdMaxQl, P.TimeExist, P.Flags, P.RelXPos, P.RelYPos, P.RelZPos, P.Unk2, P.Unk1, P.Unk3 }, F.RndSpawnMonster);
            Add(new SpellArgument[] { Text(0), Text(1), P.Value }, F.GenerateName53132, F.Text);
            Add(new SpellArgument[] { P.Unk123, P.Radius }, F.AreaTrigger);
            Add(new SpellArgument[] { P.Texture, P.BodyPart, P.SeeSkin, P.Value }, F.Unk53150);
            Add(new SpellArgument[] { P.Unk10, P.Unk122 }, F.Unk53156);
            Add(new SpellArgument[] { P.Unk11 }, F.Unk53157);
            Add(new SpellArgument[] { P.RelXPos, P.RelYPos, P.RelZPos, P.Unk123, P.Radius }, F.RemoteAreaTrigger);
            Add(new SpellArgument[] { P.NanoProperty, P.Value }, F.ResistNano);
            Add(new SpellArgument[] { P.Hash, P.HighItemIdMaxQl, P.TimeExist, P.Flags, P.RelXPos, P.RelYPos, P.RelZPos, P.Unk2, P.Unk1, P.Unk3, P.PetReq1, P.PetReq2, P.PetReq3, P.PetReqVal1, P.PetReqVal2, P.PetReqVal3 }, F.SummonPet);
            Add(new SpellArgument[] { P.Unk127 }, F.NpcSetStuckDetectScheme);
            Add(new SpellArgument[] { P.RelXPos, P.RelYPos, P.RelZPos, P.Unk134, P.Unk135, P.Unk136, P.ScreenVfx, P.Flags, P.Unk89 }, F.Tracer);
            Add(new SpellArgument[] { P.PetReqVal7 }, F.SummonPets);
            Add(new SpellArgument[] { P.Value, P.Hash, P.HighItemIdMaxQl, P.ItemId }, F.AddAction);
            Add(new SpellArgument[] { P.Value, P.Unk138, P.Unk139, P.Unk140 }, F.NpcToggleFov);
            Add(new SpellArgument[] { P.Stat, P.Min, P.Max, P.DamageType, P.Value }, F.DrainDmg);
            Add(new SpellArgument[] { P.Unk150 }, F.NpcMovementAction);
            Add(new SpellArgument[] { P.Hash, P.HighItemIdMaxQl, P.TimeExist, P.Flags, P.RelXPos, P.RelYPos, P.RelZPos, P.Unk2, P.Unk1, P.Unk3, P.Unk151 }, F.SpawnMonsterRot);
            Add(new SpellArgument[] { P.CatMeshEffect, P.AnimEffect }, F.Polymorph);
            Add(new SpellArgument[] { P.RelXPos, P.RelYPos, P.RelZPos, P.Value }, F.NpcSetMoveToTarget);
            Add(new SpellArgument[] { P.Unk118 }, F.NpcSetWanderingMode);
            Add(new SpellArgument[] { P.Stat }, F.NpcUniquePlayersInHateList);
            Add(new SpellArgument[] { P.Value, P.GfxLife, P.GfxSize, P.Unk12, P.Unk13, P.Unk14, P.Unk15, P.Unk16, P.Unk17, P.Unk, P.Unk19, P.Unk20, P.Unk86 }, F.AttractorGfx);
            Add(new SpellArgument[] { P.Unk162, P.Breed, P.Sex, P.Flags, P.Unk163, P.Unk164, P.Unk165, P.Unk166 }, F.ControlHate);
            Add(new SpellArgument[] { P.Type, P.Value }, F.NpcSendPetStatus, F.Unk53248);
            Add(new SpellArgument[] { P.Hash, P.HighItemIdMaxQl, P.RelXPos, P.RelYPos, P.RelZPos, P.Unk151, P.TimeExist, P.Unk168, P.Flags }, F.DelayedSpawnNpc);
            Add(new SpellArgument[] { P.RelXPos, P.RelZPos, P.TimeExist, Text(0) }, F.RunScript);
            Add(new SpellArgument[] { P.Value, P.ItemInstance }, F.AddDefProc, F.AddOffProc);
            Add(new SpellArgument[] { P.Hash, P.Unk9, P.Flags }, F.SpawnQuest);
            Add(new SpellArgument[] { P.Hash }, F.SolveQuest);
            Add(new SpellArgument[] { P.Value, P.Radius, P.TimeExist }, F.Knockback);
            Add(new SpellArgument[] { P.Unk6, P.Unk7, P.Unk90 }, F.CreateCityGuestKey);
            Add(new SpellArgument[] { P.Arg1 }, F.RemoveStrain);
            Add(new SpellArgument[] { P.Breed, P.Sex }, F.ChangeBreed);
            Add(new SpellArgument[] { P.Sex }, F.ChangeGender);
            Add(new SpellArgument[] { P.Speed }, F.Charge);
            Add(new SpellArgument[] { Text(0), P.TimeExist, P.RegenInterval, P.PfModelIdentityInstance, P.Flags }, F.Announcement);
            Add(new SpellArgument[] { Text(0), P.Radius }, F.Unk53247);
            Add(new SpellArgument[] { P.Flags, P.Value }, F.Transfer);
            Add(new SpellArgument[] { Text(0), Text(1), Text(2), P.Hash, P.HighItemIdMaxQl, P.Unk20, P.Arg1 }, F.SendMail);
            Add(new SpellArgument[] { P.Unk21, P.Flags }, F.EndFight);
            Add(new SpellArgument[] { P.Value, P.Flags }, F.Unk53254);
            Add(new SpellArgument[] { P.TargetInstance }, F.Unk53256);
            Add(NoArguments, F.ApplyTexture);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>The function's own arguments in the order sent, or null when the client has no format for it.</summary>
        public static SpellArgument[] GetArguments(SpellFunction function)
        {
            SpellArgument[] arguments;
            return Formats.TryGetValue(function, out arguments) ? arguments : null;
        }

        #endregion

        #region Methods

        private static SpellArgument Text(int index)
        {
            return SpellArgument.Text(index);
        }

        private static void Add(SpellArgument[] arguments, params SpellFunction[] functions)
        {
            foreach (var function in functions)
            {
                Formats[function] = arguments;
            }
        }

        #endregion
    }
}
