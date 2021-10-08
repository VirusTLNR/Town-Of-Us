using System;

namespace TownOfUs
{
    public enum TeamEnum
    {
        Crew,
        Neutral,
        Impostor,
    }

    public class RoleAttribute : Attribute
    {
        public TeamEnum TeamEnum { get; private set; }

        public RoleAttribute(TeamEnum teamEnum)
        {
            this.TeamEnum = teamEnum;
        }
    }
}