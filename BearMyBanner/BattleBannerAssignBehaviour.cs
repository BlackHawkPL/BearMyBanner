﻿using System;
using System.Collections.Generic;
using System.Linq;
using BearMyBanner.Wrappers;
using BearMyBanner.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BearMyBanner
{
    public class BattleBannerAssignBehaviour : MissionLogic
    {
        private readonly BannerAssignmentController _bannerAssignmentController;
        private bool _firstSpawnInitialized;

        public BattleBannerAssignBehaviour(IBMBSettings settings)
        {
            _bannerAssignmentController = new BannerAssignmentController(settings);
        }

        public override void OnCreated()
        {
            base.OnCreated();

            try
            {
                Mission mission = Mission;

                List<CharacterObject> characterTypes = new List<CharacterObject>();
                MBObjectManager.Instance.GetAllInstancesOfObjectType(ref characterTypes);
                var wrappedCharacters = characterTypes.Select(t => new MbCharacter(t)).ToList();
                _bannerAssignmentController.FilterAllowedBearerTypes(wrappedCharacters, mission.IsHideout());
            }
            catch (Exception ex)
            {
                Main.LogInMessageLog("BMB Error: " + ex.Message);
            }
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            try
            {
                _bannerAssignmentController.ProcessAgentOnBuild(new MbAgent(agent), GetBattleType(Mission));
            }
            catch (Exception ex)
            {
                Main.LogInMessageLog("BMB Error: " + ex.Message);
            }
        }

        private BattleType GetBattleType(Mission mission)
        {
            if (mission.IsFieldBattle)
                return BattleType.FieldBattle;
            if (mission.IsSiege())
                return BattleType.Siege;
            if (mission.IsHideout())
                return BattleType.Hideout;

            return BattleType.FieldBattle;
        }

        private void EquipAgentWithBanner(Agent agent)
        {
            if (((CharacterObject)agent.Character).IsArcher)
            {
                StripWeaponsFromArcher(agent);
            }

            MissionWeapon bannerWeapon = new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>("campaign_banner_small"), agent.Origin.Banner);
            agent.EquipWeaponToExtraSlotAndWield(ref bannerWeapon);
        }

        private static void StripWeaponsFromArcher(Agent agent)
        {
            EquipmentElement weaponElement0 = agent.SpawnEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon0);
            EquipmentElement weaponElement1 = agent.SpawnEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon1);
            EquipmentElement weaponElement2 = agent.SpawnEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon2);
            EquipmentElement weaponElement3 = agent.SpawnEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon3);
            //Clones the equipment without weapons. Apparently arrows are not a weapon, but it doesn't matter
            Equipment clonedEquipment = agent.SpawnEquipment.Clone(true);

            HashSet<ItemObject.ItemTypeEnum> forbiddenItemTypes = new HashSet<ItemObject.ItemTypeEnum>()
            {
                ItemObject.ItemTypeEnum.Arrows,
                ItemObject.ItemTypeEnum.Bolts,
                ItemObject.ItemTypeEnum.Bow,
                ItemObject.ItemTypeEnum.Crossbow
            };

            if (weaponElement0.Item != null && !forbiddenItemTypes.Contains(weaponElement0.Item.Type))
            {
                clonedEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon0, weaponElement0);
            }
            if (weaponElement1.Item != null && !forbiddenItemTypes.Contains(weaponElement1.Item.Type))
            {
                clonedEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon1, weaponElement1);
            }
            if (weaponElement2.Item != null && !forbiddenItemTypes.Contains(weaponElement2.Item.Type))
            {
                clonedEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon2, weaponElement2);
            }
            if (weaponElement3.Item != null && !forbiddenItemTypes.Contains(weaponElement3.Item.Type))
            {
                clonedEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon3, weaponElement3);
            }

            agent.ClearEquipment();//Maybe this is not needed
            agent.UpdateSpawnEquipmentAndRefreshVisuals(clonedEquipment);
        }

        public override void OnFormationUnitsSpawned(Team team)
        {
            base.OnFormationUnitsSpawned(team);//Use LINQ for team parties
            try
            {
                var agents = _bannerAssignmentController.AgentsThatShouldReceiveBanners
                    .OfType<MbAgent>()
                    .Select(a => a.WrappedAgent)
                    .Where(a => a.Team == team);

                foreach (var agent in agents)
                {
                    EquipAgentWithBanner(agent);
                }

                if (!_firstSpawnInitialized)
                {
                    _firstSpawnInitialized = true;
                    foreach (KeyValuePair<string, int> entry in _bannerAssignmentController.EquippedBannersByParty)
                    {
                        Main.LogInMessageLog(entry.Key + " received " + entry.Value + " banners");
                    }
                }
            }
            catch (Exception ex)
            {
                Main.LogInMessageLog("BMB Error: " + ex.Message);
            }
        }
    }
}
