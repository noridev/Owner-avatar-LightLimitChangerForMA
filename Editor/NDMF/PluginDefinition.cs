﻿using io.github.noridev;
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(PluginDefinition))]

namespace io.github.noridev
{
    internal sealed class PluginDefinition : Plugin<PluginDefinition>
    {
        public override string QualifiedName => "io.github.noridev.light-limit-changer";
        
        public override string DisplayName => "LightLimitChanger";

        private const string ModularAvatarQualifiedName = "nadena.dev.modular-avatar";

        public override Color? ThemeColor => new Color(0.0f, 0.2f, 0.8f);

        protected override void Configure()
        {
            var sequence =
                InPhase(BuildPhase.Transforming)
                .BeforePlugin(ModularAvatarQualifiedName);

            Passes.RunningPasses(sequence);
        }
    }

}
