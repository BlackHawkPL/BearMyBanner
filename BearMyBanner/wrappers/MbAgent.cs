﻿using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace BearMyBanner.Wrappers
{
    public class MbAgent : IAgent
    {
        public MbAgent(Agent wrappedAgent)
        {
            WrappedAgent = wrappedAgent;
            Character = new MbCharacter((CharacterObject)WrappedAgent.Character);
            var partyToWrap = ((PartyGroupAgentOrigin) WrappedAgent.Origin)?.Party;
            if (partyToWrap != null)
                PartyName = partyToWrap.Name.ToString();
        }

        public Agent WrappedAgent { get; }
        public bool IsAttacker => WrappedAgent.Team.IsAttacker;
        public bool IsDefender => WrappedAgent.Team.IsDefender;
        public ICharacter Character { get; }
        public string PartyName { get; }
    }
}