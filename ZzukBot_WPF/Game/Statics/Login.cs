﻿using System;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Helpers;
using ZzukBot.Mem;
using ZzukBot.Settings;

namespace ZzukBot.Game.Statics
{
    /// <summary>
    ///     Class about out of game related methods and fields
    /// </summary>
    public sealed class Login
    {
        private static readonly Lazy<Login> _instance = new Lazy<Login>(() => new Login());

        private Login()
        {
        }

        /// <summary>
        ///     Access to the class
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static Login Instance => _instance.Value;

        /// <summary>
        ///     Number of avaible characters (returns 0 if not at the character selection screen)
        /// </summary>
        /// <value>
        ///     The number character count.
        /// </value>
        public int NumCharacterCount => Memory.Reader.Read<int>(
            Offsets.CharacterScreen.NumCharacters);

        /// <summary>
        ///     The login state
        /// </summary>
        /// <value>
        ///     The state of the login.
        /// </value>
        public Enums.LoginStates LoginState
            =>
                (Enums.LoginStates)
                Enum.Parse(typeof(Enums.LoginStates),
                    Offsets.CharacterScreen.LoginState.ReadString());

        /// <summary>
        ///     Get the message displayed by the messagebox which appears as soon as you hit login
        /// </summary>
        public string GlueDialogText
        {
            get
            {
                var result = Lua.Instance.ExecuteWithResult("{0} = GlueDialogText:GetText()");
                return result[0];
            }
        }

        /// <summary>
        ///     Resets the out of game state back to the start screen
        /// </summary>
        public void ResetLogin()
        {
            Lua.Instance.Execute("arg1 = 'ESCAPE' GlueDialog_OnKeyDown()");
            Lua.Instance.Execute(
                "if RealmListCancelButton ~= nil then if RealmListCancelButton:IsVisible() then RealmListCancelButton:Click(); end end GlueDialogText:SetText('')");
        }

        /// <summary>
        ///     Login with the saved credentials
        /// </summary>
        public void DefaultServerLogin()
        {
            if (LoginState != Enums.LoginStates.login) return;
            Lua.Instance.Execute("DefaultServerLogin('" + Options.Values.AccountName + "', '" +
                                 Options.Values.AccountPassword +
                                 "');");
        }

        private int MaxCharacterCount => 0x00B42140.ReadAs<int>();

        private string GetCharacterNameAtPos(int index)
        {
            return 0xB42144.ReadAs<IntPtr>().Add(0x120 * index + 0x8).ReadString();
        }

        /// <summary>
        ///     Enters the world.
        /// </summary>
        public void EnterWorld(string characterName)
        {
            if (!Wait.For2("AntiEnterWorldCrash", 500, true)) return;
            if (!string.IsNullOrWhiteSpace(characterName))
            {
                int? charIndex = null;
                for (var i = 0; i < MaxCharacterCount; i++)
                {
                    var charName = GetCharacterNameAtPos(i);
                    if (!string.Equals(characterName, charName, StringComparison.InvariantCultureIgnoreCase)) continue;
                    charIndex = i;
                    break;
                }
                if (charIndex == null) return;
                Functions.SelectCharacterAtIndex(charIndex.Value);
            }
            Functions.EnterWorld();
        }
    }
}