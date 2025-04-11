using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank_of_Russia_tg_bot.StateManagement
{
    public interface IUserStateManager
    {
        void SetCommandState(long userId, UserCommand command, string currency = null);
        bool TryGetUserState(long userId, out UserState state);
        void ClearState(long userId);
    }

    public enum UserCommand
    {
        None,
        KeyRate,
        ExchangeRate
    }

    public class UserState
    {
        public UserCommand CurrentCommand { get; set; }
        public string SelectedCurrency { get; set; }
    }
}
