using System;
using System.Linq;
using Microsoft.Extensions.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchVote
{
    public class VoteBot
    {
        private readonly TwitchClient _client;
        private readonly TwitchInfo _options;

        private Poll? _currentPoll;

        public VoteBot(IOptions<TwitchInfo> options)
        {
            if (options is null)
                throw new NullReferenceException();

            _options = options.Value;

            var credentials = new ConnectionCredentials(_options.AccountName, _options.AccessToken);

            _client = new TwitchClient();
            _client.Initialize(credentials, _options.ChannelName);

            _client.OnLog += OnLog;
            _client.OnJoinedChannel += OnJoinedChannel;
            _client.OnConnected += OnConnected;

            _client.OnChatCommandReceived += HandleCommands;
            _client.OnLeftChannel += OnLeftChannel;
            _client.Connect();
        }

        private void HandleCommands(object sender, OnChatCommandReceivedArgs command)
        {
            string? message = null;

            switch(command.Command.CommandText.ToLower())
            {
                case "vote":
                    message = HandleVoteCommand(command);
                    break;
                case "newvote":
                    message = HandleNewVoteCommand(command);
                    break;
                case "enablevote":
                    message = HandleEnableVotingCommand(command);
                    break;
                case "disablevote":
                    message = HandleDisableVotingCommand(command);
                    break;
                case "options":
                    message = HandleOptionsCommand(command);
                    break;
                case "winner":
                    message = HandleWinnerCommand(command);
                    break;

            }

            // Send optional message if anything special occured.
            if(message is string)
            {
                _client.SendMessage(command.Command.ChatMessage.Channel, message);
            }
        }

        #region HelperMethods

        private bool IsBroadcasterOrModerator(ChatMessage chatMessage)
        {
            return chatMessage.IsBroadcaster || chatMessage.IsModerator;
        }

        #endregion

        #region CommandHandling

        private string? HandleOptionsCommand(OnChatCommandReceivedArgs command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (_currentPoll is null || _currentPoll.VoteState != VoteState.Enabled)
                return $"There is no active poll";

            return $"Vote with '!vote <option>'. The current options are " + string.Join(", ", _currentPoll.Options);
        }

        private string? HandleVoteCommand(OnChatCommandReceivedArgs command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (command.Command.ArgumentsAsList.Count != 1)
                return $"Your option was not recognized {command.Command.ChatMessage.DisplayName}";

            if (_currentPoll is null)
                return $"There is no active poll";

            if(!_currentPoll.Options.Any(p => p.Equals(command.Command.ArgumentsAsList[0])))
                return $"Your option was not recognized {command.Command.ChatMessage.DisplayName}";

            if (_currentPoll.Vote(new VoteInfo(command.Command.ChatMessage.DisplayName, command.Command.ArgumentsAsList[0])))
            {
                return $"{command.Command.ChatMessage.DisplayName} voted for {command.Command.ArgumentsAsList[0]}";
            }

            return $"Cou can only vote once {command.Command.ChatMessage.DisplayName}.";
        }

        private string? HandleNewVoteCommand(OnChatCommandReceivedArgs command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (!IsBroadcasterOrModerator(command.Command.ChatMessage))
                return "Not Authorized";

            if (command.Command.ArgumentsAsList.Count < 2)
                return "There need to be at least two options for a poll";

            _currentPoll = new Poll(command.Command.ArgumentsAsList);
            return "New Poll started. Vote with '!vote <option>'. The options are " + string.Join(", ", _currentPoll.Options);
        }

        private string? HandleEnableVotingCommand(OnChatCommandReceivedArgs command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (!IsBroadcasterOrModerator(command.Command.ChatMessage))
                return "Not Authorized";

            if (_currentPoll is null)
                return $"There is no active poll";

            if (_currentPoll.VoteState == VoteState.Enabled)
                return "Voting is already enabled";

            _currentPoll.EnableVoting();
            return "Voting for the poll has been enabled. Vote with '!vote <option>'. The options are " + string.Join(", ", _currentPoll.Options);

        }

        private string? HandleDisableVotingCommand(OnChatCommandReceivedArgs command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (!IsBroadcasterOrModerator(command.Command.ChatMessage))
                return "Not Authorized";

            if (_currentPoll is null)
                return $"There is no active poll";

            if (_currentPoll.VoteState == VoteState.Disabled)
                return "Voting is already disabled";


            _currentPoll.DisableVoting();
            return "The voting phase for the poll has ended.";
        }

        private string? HandleWinnerCommand(OnChatCommandReceivedArgs command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (!IsBroadcasterOrModerator(command.Command.ChatMessage))
                return "Not Authorized";

            if (_currentPoll is null)
                return $"There is no active poll";

            if (command.Command.ArgumentsAsList.Count != 1)
                return "There must be exact one winning option";

            var winners = _currentPoll.GetVoters(command.Command.ArgumentsAsList[0]);

            if (winners is null)
                return "Option was not recognized";

            var person = winners.Count() == 1 ? "Person" : "People";

            return $"{winners.Count()} {person} voted for {command.Command.ArgumentsAsList[0]}: {string.Join(", ", winners)}";
        }

        #endregion

        private void OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine($"[{e.DateTime.ToString()}] {e.BotUsername} > {e.Data}");
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine($"{e.BotUsername} connected to channel #{e.Channel}.");
        }
        private void OnLeftChannel(object sender, OnLeftChannelArgs e)
        {
            Console.WriteLine($"{e.BotUsername} disconnected from channel #{e.Channel}.");
        }
    }
}
