using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank_of_Russia_tg_bot.StateManagement
{
    public class UserStateManager : IUserStateManager
    {
        private readonly ConcurrentDictionary<long, UserState> _userStates = new();

        public void SetCommandState(long userId, UserCommand command, string currency = null)
        {
            _userStates[userId] = new UserState
            {
                CurrentCommand = command,
                SelectedCurrency = currency
            };
        }

        public bool TryGetUserState(long userId, out UserState state)
        {
            return _userStates.TryGetValue(userId, out state);
        }

        public void ClearState(long userId)
        {
            _userStates.TryRemove(userId, out _);
        }
    }
}
