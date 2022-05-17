using System.Collections.Generic;

namespace FarmCraft.Community.Tests.Config
{
    public class TestSettings
    {
        public Dictionary<string, string> ConnectionStrings { get; set; }
        public int DefaultActorWaitSeconds { get; set; }
        public int TokenMinuteDuration { get; set; }
        public string TokenIssuer { get; set; }
    }
}
