using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TwitchVote
{
    public class Poll
    {
        private readonly IDictionary<string, string> _votes = new ConcurrentDictionary<string, string>();

        public Poll(IEnumerable<string> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (!(options.Count() >= 2))
                throw new ArgumentException("There need to be atleast two option for a valid poll", nameof(options));

            Options = options;
            VoteState = VoteState.Enabled;
        }

        public IEnumerable<string> Options { get; }

        public VoteState VoteState { get; private set; }

        public bool Vote(VoteInfo vote)
        {
            // Make sure voting is enabled
            if (VoteState != VoteState.Enabled)
                return false;

            if (!Options.Any(p => p.Equals(vote.Vote, StringComparison.OrdinalIgnoreCase)))
                return false;
            
            return _votes.TryAdd(vote.DisplayName, vote.Vote);
        }

        public void EnableVoting()
        {
            VoteState = VoteState.Enabled;
        }

        public void DisableVoting()
        {
            VoteState = VoteState.Disabled;
        }

        /// <summary>
        /// Gets all voters of the current poll that voted for the given option.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>All voters that voted for <paramref name="option"/>, <c>null</c> if the option does not exist.</returns>
        public IEnumerable<string>? GetVoters(string option)
        {
            if (!Options.Any(p => p.Equals(option, StringComparison.OrdinalIgnoreCase)))
                return null;

            return _votes.Where(p => p.Value.Equals(option, StringComparison.OrdinalIgnoreCase)).Select(p => p.Key);
        }
    }
}
