using System;

namespace TwitchVote
{
    public readonly struct VoteInfo
    {
        public VoteInfo(string displayName, string vote)
        {
            if (string.IsNullOrEmpty(displayName))
                throw new ArgumentException("message", nameof(displayName));
            if (string.IsNullOrWhiteSpace(vote))
                throw new ArgumentException("message", nameof(vote));
            DisplayName = displayName;
            Vote = vote;
        }
        public string DisplayName { get; }
        public string Vote { get; }
    }
}
