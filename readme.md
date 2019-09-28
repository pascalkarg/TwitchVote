# TwitchVote

Simple polling plugin that lets the twitch broadcaster decide which option has won and prints the display names of the users which voted for the winning option.

## Usage

General Command             | Purpose | 
--------            | -------- | 
`!vote <option>`    | Vote for `<option>`   | 
`!options`    | List all options of the current poll   | 

Broadcaster/Mod Commands |    |
--------            | -------- | 
`!newvote <option1> <option2> ..`    | Starts a new poll with the given options   | 
`!enablevote`    | Enable voting. `!newvote` automatically enables voting  | 
`!disablevote`    | Disables voting.   | 
`!winner <option>`    | Gets all users which voted for the given `<option>`   | 