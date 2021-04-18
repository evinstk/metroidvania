using Nez;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Audio
{
    class AudioManager : GlobalManager
    {
        FMOD.Studio.System _system;
        HashSet<string> _banks = new HashSet<string>();

        public override void OnEnabled()
        {
            FMOD.Studio.System.create(out _system);
            var systemResult = _system.initialize(8, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            Insist.IsTrue(systemResult == FMOD.RESULT.OK);
        }

        public override void Update()
        {
            _system.update();
        }

        public void LoadBank(string bankName)
        {
            if (!_banks.Contains(bankName))
            {
                var bankKey = $"{ContentPath.Root}{bankName}";
                _system.loadBankFile(bankKey, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out var bank);
                Insist.IsTrue(bank.handle.ToInt32() != 0);
                bank.loadSampleData();
                _banks.Add(bankName);
            }
        }

        public FMOD.Studio.EventInstance LoadSound(string bankName, string evt)
        {
            LoadBank(bankName);
            _system.getEvent($"event:/{Path.GetFileNameWithoutExtension(bankName)}/{evt}", out var desc);
            desc.createInstance(out var instance);
            return instance;
        }
    }
}
