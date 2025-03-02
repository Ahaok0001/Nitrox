﻿using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxClient.Serialization;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class GameSettings_SerializeInputSettings_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameSettings.SerializeInputSettings(default(GameSettings.ISerializer)));

        public static void Postfix(GameSettings.ISerializer serializer)
        {
            ClientConfig cfg = ClientConfig.Load(NitroxUser.AppDataPath);
            KeyBindingManager keyBindingManager = new();
            string serializerFormat = "Input/Binding/{0}/{1}/{2}";

            foreach (GameInput.BindingSet bindingSet in Enum.GetValues(typeof(GameInput.BindingSet)))
            {
                foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
                {
                    Log.Debug($"Getting keybinding: {keyBinding.Device}, {keyBinding.Label} ({keyBinding.Button}), {bindingSet}");
                    string binding = GameInput.GetBinding(keyBinding.Device, keyBinding.Button, bindingSet);

                    // We need to assign the correct binding for primary and secondary binding sets to the relevant area of the config.
                    switch ((KeyBindingValues)keyBinding.Button)
                    {
                        case KeyBindingValues.CHAT when bindingSet == GameInput.BindingSet.Primary:
                            cfg.OpenChatKeybindPrimary = binding;
                            break;
                        case KeyBindingValues.FOCUS_DISCORD when bindingSet == GameInput.BindingSet.Primary:
                            cfg.FocusDiscordKeybindPrimary = binding;
                            break;
                        case KeyBindingValues.CHAT when bindingSet == GameInput.BindingSet.Secondary:
                            cfg.OpenChatKeybindSecondary = binding;
                            break;
                        case KeyBindingValues.FOCUS_DISCORD when bindingSet == GameInput.BindingSet.Secondary:
                            cfg.FocusDiscordKeybindSecondary = binding;
                            break;
                    }
                }
            }

            cfg.Serialize(NitroxUser.AppDataPath);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
